using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

using AggregateSource;
using AggregateSource.EventStore;
using AggregateSource.EventStore.Resolvers;

using Dapper;

using EventStore.ClientAPI;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using NodaTime;

using ProductContext.Domain.Products;
using ProductContext.Domain.Projections;
using ProductContext.Framework;

using Projac.Connector.NetCore;

using Serilog;

using Swashbuckle.AspNetCore.Swagger;

namespace ProductContext.WebApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                                            .SetBasePath(env.ContentRootPath)
                                            .AddJsonFile("appsettings.json", false, true)
                                            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                                            .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services) => ConfigureServicesAsync(services).GetAwaiter().GetResult();

        private async Task ConfigureServicesAsync(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new Info { Title = "Event Sourced Product Creation", Version = "v1" }); });

            services.AddSingleton<IBus>(_ =>
            {
                IEventStoreConnection esConnection = Defaults.GetConnection().GetAwaiter().GetResult();

                var bus = new InMemoryBus("bus");
                var defaultSerializer = new DefaultEventDeserializer();
                var concurrentUnitOfWork = new ConcurrentUnitOfWork();

                var productRepository = new AsyncRepository<Product>(
                    Product.Factory,
                    concurrentUnitOfWork,
                    esConnection,
                    new EventReaderConfiguration(
                        new SliceSize(500),
                        defaultSerializer,
                        new PassThroughStreamNameResolver(),
                        new NoStreamUserCredentialsResolver()));

                Func<DateTime> getDatetime = () => SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
                var productCommandHandlers = new ProductCommandHandlers(productRepository, getDatetime);

                bus.Subscribe(productCommandHandlers);

                return bus;
            });

            InitProjections().Wait();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger()
               .UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); })
               .UseMvc();
        }

        private async Task InitProjections()
        {
            IEventStoreConnection esConnection = await Defaults.GetConnection();

            string projectionsConnectionString = Configuration["Data:Projections:ConnectionString"];
            var connection = new SqlConnection(projectionsConnectionString);

            var projectionMapper = new ProjectionTypeMapper();
            projectionMapper.HandledBy<Events.V1.ProductCreated, ProductProjection>();

            var handlers = new List<ConnectedProjectionHandler<SqlConnection>>();
            handlers.AddRange(new ProductProjector());
            handlers.AddRange(new CheckpointProjector());

            var projector = new ConnectedProjector<SqlConnection>(
                Resolve.WhenEqualToHandlerMessageType(handlers.ToArray())
            );

            await SetupProjectionsDb(projector, connection).ConfigureAwait(false);

            Func<object, Task> projectFunc = async @event =>
            {
                await projector.ProjectAsync(connection, @event);
            };

            var deserializer = new DefaultEventDeserializer();

            esConnection.SubscribeToAllFrom(
                Position.Start, 
                new CatchUpSubscriptionSettings(10000, 500, true, false),
                EventAppeared(projectFunc, deserializer, projectionMapper),
                LiveProcessingStarted(projectFunc, deserializer, projectionMapper),
                SubscriptionDropped(projectFunc, deserializer, projectionMapper));
        }

        private Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception> SubscriptionDropped(
            Func<object, Task> projector,
            DefaultEventDeserializer deserializer,
            ProjectionTypeMapper projectionMapper) =>
            async (subscription, reason, ex) =>
            {
                // TODO: Reevaluate stopping subscriptions when issues with reconnect get fixed.
                // https://github.com/EventStore/EventStore/issues/1127
                // https://groups.google.com/d/msg/event-store/AdKzv8TxabM/VR7UDIRxCgAJ

                subscription.Stop();

                switch (reason)
                {
                    case SubscriptionDropReason.UserInitiated:
                        Log.Debug("{projection} projection stopped gracefully.", subscription);
                        break;
                    case SubscriptionDropReason.SubscribingError:
                    case SubscriptionDropReason.ServerError:
                    case SubscriptionDropReason.ConnectionClosed:
                    case SubscriptionDropReason.CatchUpError:
                    case SubscriptionDropReason.ProcessingQueueOverflow:
                    case SubscriptionDropReason.EventHandlerException:
                        Log.Error(
                            "{projection} projection stopped because of a transient error ({reason}). " +
                            "Attempting to restart...",
                            ex, subscription.SubscriptionName, reason);
                        await Task.Run(InitProjections);
                        break;
                    default:
                        Log.Fatal(
                            "{projection} projection stopped because of an internal error ({reason}). " +
                            "Please check your logs for details.",
                            ex, subscription.SubscriptionName, reason);
                        break;
                }
            };

        private Action<EventStoreCatchUpSubscription> LiveProcessingStarted(Func<object, Task> projector, DefaultEventDeserializer deserializer, ProjectionTypeMapper projectionMapper) =>
            _ => Log.Debug("projection has caught up, now processing live!");

        private Func<EventStoreCatchUpSubscription, ResolvedEvent, Task> EventAppeared(Func<object, Task> projector, DefaultEventDeserializer deserializer, ProjectionTypeMapper projectionMapper) =>
            async (subscription, e) =>
            {
                // pass system events ;)
                if (e.OriginalEvent.EventType.StartsWith("$"))
                {
                    return;
                }

                await projector(deserializer.Deserialize(e));

                await projector(new EventProjected(projectionMapper.WhoHandlesMe(e.Event.EventType).Name, JsonConvert.SerializeObject(e.OriginalPosition)));
            };

        private static async Task SetupProjectionsDb(ConnectedProjector<SqlConnection> projector, SqlConnection connection)
        {
            await projector.ProjectAsync(connection, new object[]
            {
                new DropCheckpointSchema(),
                new CreateCheckpointSchema(),
                new DropProductSchema(),
                new CreateProductSchema()
            }).ConfigureAwait(false);
        }
    }
}
