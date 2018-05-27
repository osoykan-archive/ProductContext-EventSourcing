using System;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using ProductContext.Domain.Contracts;
using ProductContext.Framework;
using Xunit;

namespace ProductContext.Integration.Tests
{
    public class ProductIntegrationTests : ProductIntegrationTestBase
    {
        [Fact]
        public async Task docker_couchbase_should_work()
        {
            var getBucket = Defaults.GetCouchbaseBucket(nameof(ProductContext), "Administrator", "password",
                "http://localhost:8091");

            var bucket = getBucket();

            Assert.NotNull(bucket);
        }

        [Fact]
        public async Task docker_event_store_should_work()
        {
            var esConnection =
                await Defaults.GetEsConnection("admin", "changeit", "tcp://admin:changeit@127.0.0.1:1113");

            Assert.NotNull(esConnection);
        }

        [Fact]
        public async Task product_content_add_shoudl_work_on_existing_product()
        {
            var document = Query(x => x.Code == "CODE123");

            var contentId = Guid.NewGuid().ToString();
            await Mediator.Send(new Commands.V1.AddContentToProduct
            {
                ProductId = document.ProductId,
                Description = "%100 Cotton",
                VariantTypeValueId = Guid.NewGuid().ToString(),
                ProductContentId = contentId
            });

            await WaitUntilProjected(x => x.Contents.Any(c => c.ProductContentId == contentId));
        }

        [Fact]
        public async Task product_creation_integraiton_test()
        {
            var productId = new Faker().Random.Uuid().ToString();
            var code = new Faker().Commerce.Product();

            await Mediator.Send(new Commands.V1.CreateProduct
            {
                ProductId = productId,
                BrandId = 1,
                BusinessUnitId = 1,
                Code = code
            });

            await WaitUntilProjected(x => x.ProductId == productId);
        }

        [Fact]
        public async Task product_variant_add_should_work_on_existing_content_and_product()
        {
            var doc = Query(x => x.Contents.Any());

            var variantId = Guid.NewGuid().ToString();
            await Mediator.Send(new Commands.V1.AddVariantToProduct
            {
                ProductId = doc.ProductId,
                VariantId = variantId,
                Barcode = Guid.NewGuid().ToString(),
                ContentId = doc.Contents.First().ProductContentId,
                VariantTypeValueId = Guid.NewGuid().ToString()
            });

            await WaitUntilProjected(x => x.Variants.Any(v => v.ProductVariantId == variantId));
        }
    }
}