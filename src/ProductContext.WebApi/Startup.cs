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

using ProductContext.Common;
using ProductContext.Common.Bus;
using ProductContext.Domain.Aggregates;
using ProductContext.Domain.CommandHandlers;
using ProductContext.Domain.Projections;

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

                var productContentRepository = new AsyncRepository<ProductContent>(
                    ProductContent.Factory,
                    concurrentUnitOfWork,
                    esConnection,
                    new EventReaderConfiguration(new SliceSize(10), defaultSerializer, new PassThroughStreamNameResolver(), new FixedStreamUserCredentialsResolver(new UserCredentials("admin", "changeit"))));
                )
                ;

                Func<DateTime> getDatetime = () => SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
                var productCommandHandlers = new ProductCommandHandlers(productRepository, getDatetime);
                var productContentCommandHandlers = new ProductContentCommandHandlers(productContentRepository, getDatetime);

                bus.Subscribe(productCommandHandlers);
                bus.Subscribe(productContentCommandHandlers);

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
            IEventStoreConnection esConnection = Defaults.GetConnection().GetAwaiter().GetResult();

            string projectionsConnectionString = Configuration["Data:Projections:ConnectionString"];
            string eventStoreConnectionString = Configuration["Data:EventStore:ConnectionString"];

            LoggingHelper.Capture(Console.Out);

            var handlers = new List<ConnectedProjectionHandler<SqlConnection>>();
            handlers.AddRange(new ProductProjector());

            var projector = new ConnectedProjector<IEventStoreConnection>(
                Resolve.WhenEqualToHandlerMessageType(handlers.ToArray())
                );

            var deserializer = new DefaultEventDeserializer();

            esConnection.SubscribeToAllFrom(Position.Start, new CatchUpSubscriptionSettings(100, 100, true, true), (subscription, resolvedEvent) =>
            {
                IEnumerable<object> @events = deserializer.Deserialize(resolvedEvent);

                foreach (object @event in @events)
                {  projector.ProjectAsync(@event)

                }
              
            });
        }
    }
}
