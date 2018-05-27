using System;
using System.Linq;
using System.Threading.Tasks;
using AggregateSource;
using AggregateSource.EventStore;
using AggregateSource.EventStore.Resolvers;
using AggregateSource.EventStore.Snapshots;
using Couchbase.Core;
using EventStore.ClientAPI;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using ProductContext.Domain.Products;
using ProductContext.Domain.Products.Snapshots;
using ProductContext.Domain.Projections;
using ProductContext.Framework;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Swagger;

namespace ProductContext.WebApi
{
    public class Startup
    {
        private static readonly GetSnapshotStreamName s_getSnapshotStreamName =
            (type, id) => $"{s_sGetStreamName(type, id)}-Snapshot";

        private static readonly GetStreamName s_sGetStreamName = (type, id) => $"{type.Name}-{id}";
        private static readonly Now s_sNow = () => SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            HostingEnvironment = env;
        }

        private IConfiguration Configuration { get; }

        private IHostingEnvironment HostingEnvironment { get; }

        public void ConfigureServices(IServiceCollection services) =>
            ConfigureServicesAsync(services).GetAwaiter().GetResult();

        private async Task ConfigureServicesAsync(IServiceCollection services)
        {
            ConfigureMvc(services);
            ConfigureLogging(services);
            ConfigureApplication(services);

            InitProjections().Wait();
        }

        private static void ConfigureMvc(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info {Title = "Event Sourced Product Creation", Version = "v1"});
            });
        }

        private void ConfigureApplication(IServiceCollection services)
        {
            services.AddMediatR(typeof(ProductCommandHandlers).Assembly);
            services.AddSingleton(p => GetEsConnection().GetAwaiter().GetResult());
            services.AddTransient<IEventDeserializer, DefaultEventDeserializer>();
            services.AddScoped<ConcurrentUnitOfWork>();
            services.AddScoped<AsyncRepository<Product>>();
            services.AddTransient(p => new EventReaderConfiguration(
                new SliceSize(500),
                p.GetService<IEventDeserializer>(),
                new TypedStreamNameResolver(typeof(Product),
                    p.GetService<GetStreamName>()),
                new NoStreamUserCredentialsResolver()));

            services.AddScoped<AsyncSnapshotableRepository<Product>>();

            services.AddTransient<IAsyncSnapshotReader, AsyncSnapshotReader>();
            services.AddTransient(p => new SnapshotReaderConfiguration(
                new DefaultSnapshotDeserializer(),
                new SnapshotableStreamNameResolver(typeof(Product),
                    p.GetService<GetSnapshotStreamName>()),
                new NoStreamUserCredentialsResolver()));

            services.AddSingleton(s_sGetStreamName);
            services.AddSingleton(s_getSnapshotStreamName);
            services.AddSingleton(s_sNow);
            services.AddTransient(provider => Product.Factory);
        }

        private void ConfigureLogging(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                var loggerCfg = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .Enrich.FromLogContext();

                if (HostingEnvironment.IsDevelopment())
                {
                    loggerCfg
                        .WriteTo.Console();
                }

                Log.Logger = loggerCfg.CreateLogger();
                builder.AddSerilog(Log.Logger);
            });
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

        private async Task<IEventStoreConnection> GetEsConnection() => await Defaults.GetEsConnection(
            Configuration["Data:EventStore:Username"],
            Configuration["Data:EventStore:Password"],
            Configuration["Data:EventStore:Url"]);

        private Func<IBucket> GetCouchbaseBucket() => Defaults.GetCouchbaseBucket(nameof(ProductContext),
            Configuration["Data:Couchbase:Username"],
            Configuration["Data:Couchbase:Password"],
            Configuration["Data:Couchbase:Url"]);

        private async Task InitProjections()
        {
            var esConnection = await GetEsConnection();

            var getBucket = GetCouchbaseBucket();

            Func<string, Task<Aggregate>> getProductAggregate = async streamId =>
            {
                var defaultSerializer = new DefaultEventDeserializer();
                var concurrentUnitOfWork = new ConcurrentUnitOfWork();

                var productRepository = new AsyncRepository<Product>(
                    Product.Factory,
                    concurrentUnitOfWork,
                    esConnection,
                    new EventReaderConfiguration(
                        new SliceSize(500),
                        defaultSerializer,
                        new TypedStreamNameResolver(typeof(Product), s_sGetStreamName),
                        new NoStreamUserCredentialsResolver()));

                await productRepository.GetAsync(streamId);

                return concurrentUnitOfWork.GetChanges().First();
            };

            await ProjectionManagerBuilder.With
                .Connection(esConnection)
                .Deserializer(new DefaultEventDeserializer())
                .CheckpointStore(new CouchbaseCheckpointStore(getBucket))
                .Snaphotter(
                    new EventStoreSnapshotter<Aggregate, ProductSnapshot>(
                        getProductAggregate,
                        () => esConnection,
                        e => e.Event.EventNumber > 0 && e.Event.EventNumber % 5 == 0,
                        stream => $"{stream}-Snapshot",
                        s_sNow))
                .Projections(
                    ProjectorDefiner.For<ProductProjection>()
                ).Activate(getBucket);
        }
    }
}