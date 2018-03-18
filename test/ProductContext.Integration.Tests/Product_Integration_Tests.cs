using System;
using System.Threading.Tasks;

using AggregateSource;
using AggregateSource.EventStore;
using AggregateSource.EventStore.Resolvers;

using Couchbase.Core;
using Couchbase.Search;

using EventStore.ClientAPI;

using ProductContext.Domain.Contracts;
using ProductContext.Domain.Products;
using ProductContext.Domain.Projections;
using ProductContext.Framework;

using Xunit;

namespace ProductContext.Integration.Tests
{
    public class Product_Integration_Tests : IClassFixture<EventStoreFixture>, IClassFixture<CouchbaseFixture>
    {
        [Fact]
        public async Task docker_event_store_should_work()
        {
            IEventStoreConnection esConnection = await Defaults.GetEsConnection("admin", "changeit", "tcp://admin:changeit@127.0.0.1:1113");

            Assert.NotNull(esConnection);
        }

        [Fact]
        public async Task docker_couchbase_should_work()
        {
            Func<IBucket> getBucket = Defaults.GetCouchbaseBucket(nameof(ProductContext), "Administrator", "123456", "http://localhost:8091");

            IBucket bucket = getBucket();

            Assert.NotNull(bucket);
        }

        [Fact]
        public async Task product_creation_integraiton_test()
        {
            IEventStoreConnection esConnection = await Defaults.GetEsConnection("admin", "changeit", "tcp://admin:changeit@127.0.0.1:1113");

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

            DateTime GetDatetime()
            {
                return DateTime.UtcNow;
            }

            var productCommandHandlers = new ProductCommandHandlers((type, id) => $"{type.Name}-{id}", productRepository, GetDatetime);
            bus.Subscribe<Commands.V1.CreateProduct>(productCommandHandlers);
            bus.Subscribe<Commands.V1.AddVariantToProduct>(productCommandHandlers);
            bus.Subscribe<Commands.V1.AddContentToProduct>(productCommandHandlers);

            Func<IBucket> getBucket = Defaults.GetCouchbaseBucket(nameof(ProductContext), "Administrator", "123456", "http://localhost:8091");

            await ProjectionManagerBuilder.With
                                          .Connection(esConnection)
                                          .Deserializer(new DefaultEventDeserializer())
                                          .CheckpointStore(new CouchbaseCheckpointStore(getBucket))
                                          .Projections(
                                              ProjectorDefiner.For<ProductProjection>()
                                          ).Activate(getBucket);


            await bus.PublishAsync(new Commands.V1.CreateProduct() { BrandId = 1, BusinessUnitId = 1, Code = "CODE123" });
        }
    }
}
