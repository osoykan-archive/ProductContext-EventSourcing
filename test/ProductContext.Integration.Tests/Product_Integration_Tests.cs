using System;
using System.Threading.Tasks;

using Couchbase.Core;

using EventStore.ClientAPI;

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
    }
}
