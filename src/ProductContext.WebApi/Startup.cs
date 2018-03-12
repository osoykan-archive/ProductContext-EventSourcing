using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

using AggregateSource;
using AggregateSource.EventStore;
using AggregateSource.EventStore.Resolvers;

using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NodaTime;

using ProductContext.Domain.Products;
using ProductContext.Domain.Projections;
using ProductContext.Framework;

using Projac.Connector.NetCore;

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
                    new EventReaderConfiguration(new SliceSize(10), defaultSerializer, new PassThroughStreamNameResolver(), new FixedStreamUserCredentialsResolver(new UserCredentials("admin", "changeit"))));

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

            var handlers = new List<ConnectedProjectionHandler<SqlConnection>>();
            handlers.AddRange(new ProductProjector());

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
                EventAppeared(projectFunc, deserializer),
                LiveProcessingStarted(projectFunc, deserializer),
                SubscriptionDropped(projectFunc, deserializer));
        }

        private Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception> SubscriptionDropped(Func<object, Task> projector, DefaultEventDeserializer deserializer) =>
            async (subscription, reason, ex) =>
            {
                subscription.Stop();

                await InitProjections();
            };

        private Action<EventStoreCatchUpSubscription> LiveProcessingStarted(Func<object, Task> projector, DefaultEventDeserializer deserializer) =>
            subscription =>
            {
            };

        private Func<EventStoreCatchUpSubscription, ResolvedEvent, Task> EventAppeared(Func<object, Task> projector, DefaultEventDeserializer deserializer) =>
            async (subscription, e) =>
            {
                // pass system events ;)
                if (e.OriginalEvent.EventType.StartsWith("$"))
                {
                    return;
                }

                await projector(deserializer.Deserialize(e));
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
