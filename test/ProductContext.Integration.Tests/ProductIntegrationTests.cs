using System;
using System.Threading.Tasks;

using Couchbase.Core;

using EventStore.ClientAPI;

using FluentAssertions;

using ProductContext.Domain.Contracts;
using ProductContext.Framework;

using Xunit;

namespace ProductContext.Integration.Tests
{
    public class ProductIntegrationTests : ProductIntegrationTestBase
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
            Func<IBucket> getBucket = Defaults.GetCouchbaseBucket(nameof(ProductContext), "Administrator", "password", "http://localhost:8091");

            IBucket bucket = getBucket();

            Assert.NotNull(bucket);
        }

        [Fact]
        public async Task product_creation_integraiton_test()
        {
            await Bus.PublishAsync(new Commands.V1.CreateProduct { BrandId = 1, BusinessUnitId = 1, Code = "CODE123" });

            WaitUntilProjected(x => x.Code == "CODE123");
        }
    }
}
