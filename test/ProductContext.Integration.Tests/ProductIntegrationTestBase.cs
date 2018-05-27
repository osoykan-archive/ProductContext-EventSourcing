using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AggregateSource;
using AggregateSource.EventStore;
using AggregateSource.EventStore.Resolvers;
using AggregateSource.EventStore.Snapshots;
using Couchbase.Core;
using Couchbase.Linq;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using ProductContext.Domain.Products;
using ProductContext.Domain.Products.Snapshots;
using ProductContext.Domain.Projections;
using ProductContext.Framework;

namespace ProductContext.Integration.Tests
{
    public class ProductIntegrationTestBase
    {
        private static readonly GetSnapshotStreamName s_getSnapshotStreamName =
            (type, id) => $"{s_getStreamName(type, id)}-Snapshot";

        private static readonly GetStreamName s_getStreamName = (type, id) => $"{type.Name}-{id}";
        private static readonly Now s_now = () => SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
        private readonly Func<IBucket> _getBucket;
        private readonly IServiceProvider _serviceProvider;

        protected IMediator Mediator;

        public ProductIntegrationTestBase()
        {
            var esConnection = Defaults
                .GetEsConnection("admin", "changeit", "tcp://admin:changeit@127.0.0.1:1113").GetAwaiter().GetResult();

            _getBucket = Defaults.GetCouchbaseBucket(nameof(ProductContext), "Administrator", "password",
                "http://localhost:8091");

            var services = new ServiceCollection();
            services.AddMediatR(typeof(ProductCommandHandlers).Assembly);

            services.AddMediatR(typeof(ProductCommandHandlers).Assembly);
            services.AddSingleton(p => esConnection);
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

            services.AddSingleton(s_getStreamName);
            services.AddSingleton(s_getSnapshotStreamName);
            services.AddSingleton(s_now);
            services.AddTransient(provider => Product.Factory);

            _serviceProvider = services.BuildServiceProvider();
            Mediator = The<IMediator>();

            Func<string, Task<Aggregate>> getProductAggregate = async streamId =>
            {
                var productRepository = new AsyncRepository<Product>(
                    Product.Factory,
                    new ConcurrentUnitOfWork(),
                    esConnection,
                    new EventReaderConfiguration(
                        new SliceSize(500),
                        new DefaultEventDeserializer(),
                        new PassThroughStreamNameResolver(),
                        new NoStreamUserCredentialsResolver()));

                await productRepository.GetAsync(streamId);
                productRepository.UnitOfWork.TryGet(streamId, out var aggregate);
                return aggregate;
            };

            ProjectionManagerBuilder.With
                .Connection(esConnection)
                .Deserializer(new DefaultEventDeserializer())
                .CheckpointStore(new CouchbaseCheckpointStore(_getBucket))
                .Snaphotter(
                    new EventStoreSnapshotter<Aggregate, ProductSnapshot>(
                        getProductAggregate,
                        () => esConnection,
                        e => e.Event.EventNumber > 0 && e.Event.EventNumber % 1 == 0,
                        stream => $"{stream}-Snapshot",
                        s_now
                    )
                )
                .Projections(
                    ProjectorDefiner.For<ProductProjection>()
                ).Activate(_getBucket).GetAwaiter().GetResult();
        }

        protected T The<T>() => _serviceProvider.CreateScope().ServiceProvider.GetService<T>();

        protected ProductDocument Query(Expression<Func<ProductDocument, bool>> filter)
        {
            ProductDocument doc;
            do
            {
                using (var bucket = _getBucket())
                {
                    try
                    {
                        doc = new BucketContext(bucket).Query<ProductDocument>().FirstOrDefault(filter);
                    }
                    catch (Exception exception)
                    {
                        doc = null;
                    }
                }
            } while (doc == null);

            return doc;
        }

        protected Task WaitUntilProjected(Expression<Func<ProductDocument, bool>> filter)
        {
            var taskCompletionSource = new TaskCompletionSource<ProductDocument>();

            ProductDocument doc;
            do
            {
                using (var bucket = _getBucket())
                {
                    try
                    {
                        doc = new BucketContext(bucket).Query<ProductDocument>().FirstOrDefault(filter);
                        taskCompletionSource.SetResult(doc);
                    }
                    catch (Exception ex)
                    {
                        doc = null;
                    }
                }
            } while (doc == null);

            return taskCompletionSource.Task;
        }
    }
}