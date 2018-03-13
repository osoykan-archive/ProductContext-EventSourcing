using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

using AggregateSource;
using AggregateSource.EventStore;
using AggregateSource.EventStore.Resolvers;

using EventStore.ClientAPI;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NodaTime;

using ProductContext.Domain.Products;
using ProductContext.Domain.Projections;
using ProductContext.Framework;

using Projac;

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
            Func<SqlConnection> getConnection = () => new SqlConnection(projectionsConnectionString);

            var projector = new Projector<SqlConnection>(
                Resolve.WhenEqualToHandlerMessageType(new ProductProjection().Handlers.Concat(new CheckpointProjection().Handlers).ToArray()
                ));
            await SetupProjectionsDb(projector, new SqlConnection(projectionsConnectionString)).ConfigureAwait(false);

            await ProjectionManagerBuilder.With
                                          .Connection(esConnection)
                                          .Deserializer(new DefaultEventDeserializer())
                                          .CheckpointStore(new CheckpointStore(getConnection))
                                          .Projections(
                                              new ProjectorDefiner().From<ProductProjection>()
                                          ).Activate(getConnection);
        }

        private static async Task SetupProjectionsDb(Projector<SqlConnection> projector, SqlConnection connection)
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
