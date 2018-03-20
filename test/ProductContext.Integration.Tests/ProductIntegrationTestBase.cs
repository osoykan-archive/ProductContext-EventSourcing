using System;
using System.Linq;
using System.Linq.Expressions;

using AggregateSource;
using AggregateSource.EventStore;
using AggregateSource.EventStore.Resolvers;
using AggregateSource.EventStore.Snapshots;

using Couchbase.Core;
using Couchbase.Linq;

using EventStore.ClientAPI;

using ProductContext.Domain.Contracts;
using ProductContext.Domain.Products;
using ProductContext.Domain.Projections;
using ProductContext.Framework;

namespace ProductContext.Integration.Tests
{
    public class ProductIntegrationTestBase
    {
        public readonly Func<IBucket> GetBucket;
        public readonly AsyncRepository<Product> Repository;

        public ProductIntegrationTestBase()
        {
            IEventStoreConnection esConnection = Defaults.GetEsConnection("admin", "changeit", "tcp://admin:changeit@127.0.0.1:1113").GetAwaiter().GetResult();

            Bus = new InMemoryBus("bus");
            var defaultSerializer = new DefaultEventDeserializer();
            var defaultSnapshotDeserializer = new DefaultSnapshotDeserializer();
            var concurrentUnitOfWork = new ConcurrentUnitOfWork();

            Repository = new AsyncRepository<Product>(
                Product.Factory,
                concurrentUnitOfWork,
                esConnection,
                new EventReaderConfiguration(
                    new SliceSize(500),
                    defaultSerializer,
                    new PassThroughStreamNameResolver(),
                    new NoStreamUserCredentialsResolver()));

            var productSnapshotableRepository = new AsyncSnapshotableRepository<Product>(
                Product.Factory,
                concurrentUnitOfWork,
                esConnection,
                new EventReaderConfiguration(new SliceSize(500), defaultSerializer, new PassThroughStreamNameResolver(), new NoStreamUserCredentialsResolver()),
                new AsyncSnapshotReader(esConnection, new SnapshotReaderConfiguration(defaultSnapshotDeserializer, new PassThroughStreamNameResolver(), new NoStreamUserCredentialsResolver())));

            DateTime GetDatetime() => DateTime.UtcNow;

            var productCommandHandlers = new ProductCommandHandlers(
                (type, id) => $"{type.Name}-{id}",
                Repository,
                productSnapshotableRepository,
                GetDatetime);

            Bus.Subscribe<Commands.V1.CreateProduct>(productCommandHandlers);
            Bus.Subscribe<Commands.V1.AddVariantToProduct>(productCommandHandlers);
            Bus.Subscribe<Commands.V1.AddContentToProduct>(productCommandHandlers);

            GetBucket = Defaults.GetCouchbaseBucket(nameof(ProductContext), "Administrator", "password", "http://localhost:8091");

            ProjectionManagerBuilder.With
                                    .Connection(esConnection)
                                    .Deserializer(new DefaultEventDeserializer())
                                    .CheckpointStore(new CouchbaseCheckpointStore(GetBucket))
                                    .Projections(
                                        ProjectorDefiner.For<ProductProjection>()
                                    ).Activate(GetBucket).GetAwaiter().GetResult();
        }

        public IBus Bus { get; }

        public void CreateIndex()
        {
            using (IBucket bucket = GetBucket())
            {
                bucket.Query<dynamic>("CREATE PRIMARY INDEX `ProductIndex` ON `ProductContext` USING GSI;");
            }
        }

        public void WaitUntilProjected(Expression<Func<ProductDocument, bool>> filter)
        {
            CreateIndex();
            ProductDocument doc;
            do
            {
                using (IBucket bucket = GetBucket())
                {
                    doc = new BucketContext(bucket).Query<ProductDocument>().FirstOrDefault(filter);
                }
            }
            while (doc == null);
        }
    }
}
