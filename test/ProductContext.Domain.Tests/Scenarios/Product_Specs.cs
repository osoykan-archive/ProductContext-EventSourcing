using System;

using AggregateSource.Testing;

using ProductContext.Domain.Contracts;
using ProductContext.Domain.Products;

using Xunit;

namespace ProductContext.Domain.Tests.Scenarios
{
    public class Product_Specs
    {
        [Fact]
        public void should_create_a_product()
        {
            string productId = Guid.NewGuid().ToString();
            var message = new Events.V1.ProductCreated(productId, "PRDCT1234", 1, 2);
            new ConstructorScenarioFor<Product>(() =>
                    Product.Create(
                        message.ProductId,
                        message.BrandId,
                        message.ProductCode,
                        message.BusinessUnitId)
                ).Then(message)
                 .Assert();
        }

        [Fact]
        public void should_accept_content_adding_to_product()
        {
            string productId = Guid.NewGuid().ToString();
            string productContentId = Guid.NewGuid().ToString();
            const string contentDescription = "%100 Cotton TShirt";
            string variantTypeValueId = Guid.NewGuid().ToString();
            const string code = "PRD12";

            new CommandScenarioFor<Product>(Product.Factory)
                .Given(new Events.V1.ProductCreated(productId, code, 1, 2))
                .When(x => x.AddContent(productContentId, contentDescription, variantTypeValueId))
                .Then(new Events.V1.ContentAddedToProduct(productId, productContentId, contentDescription, variantTypeValueId, (int)Enums.ProductContentStatus.Draft, (int)Enums.VariantType.Color))
                .Assert();
        }

        [Fact]
        public void variant_should_be_added_to_existing_product_and_content()
        {
            string productId = Guid.NewGuid().ToString();
            string productContentId = Guid.NewGuid().ToString();
            const string contentDescription = "%100 Cotton TShirt";
            string colorId = Guid.NewGuid().ToString();
            string variantId = Guid.NewGuid().ToString();
            const string code = "PRD12";
            const string barcode = "BARCODE123";
            string sizeId = Guid.NewGuid().ToString();

            new CommandScenarioFor<Product>(Product.Factory)
                .Given(
                    new Events.V1.ProductCreated(productId, code, 1, 2),
                    new Events.V1.ContentAddedToProduct(productId, productContentId, contentDescription, colorId, (int)Enums.ProductContentStatus.Draft, (int)Enums.VariantType.Color)
                )
                .When(sut => sut.AddVariant(productContentId, variantId, barcode, sizeId))
                .Then(new Events.V1.VariantAddedToProduct(productId, productContentId, variantId, barcode, sizeId, (int)Enums.VariantType.Size))
                .Assert();
        }

        [Fact]
        public void adding_variant_to_product_but_not_existing_content_should_throw_exception()
        {
            string productId = Guid.NewGuid().ToString();
            string productContentId = Guid.NewGuid().ToString();
            string variantId = Guid.NewGuid().ToString();
            const string code = "PRD12";
            const string barcode = "BARCODE123";
            string sizeId = Guid.NewGuid().ToString();

            new CommandScenarioFor<Product>(Product.Factory)
                .Given(
                    new Events.V1.ProductCreated(productId, code, 1, 2)
                )
                .When(sut => sut.AddVariant(productContentId, variantId, barcode, sizeId))
                .Throws(new InvalidOperationException("Content not found for Variant creation"))
                .Assert();
        }

        [Fact]
        public void adding_content_product_but_already_existed_color_should_be_idempotent()
        {
            string productId = Guid.NewGuid().ToString();
            const string code = "PRD12";
            string firstContentId = Guid.NewGuid().ToString();
            const string firstContentDesc = "%100 Cotton TShirt";
            string colorId = Guid.NewGuid().ToString();

            string secContentId = Guid.NewGuid().ToString();
            const string secContentDesc = "%50 Cotton TShirt";

            new CommandScenarioFor<Product>(Product.Factory)
                .Given(
                    new Events.V1.ProductCreated(productId, code, 1, 2),
                    new Events.V1.ContentAddedToProduct(productId, firstContentId, firstContentDesc, colorId, (int)Enums.ProductContentStatus.Draft, (int)Enums.VariantType.Color)
                ).When(sut => sut.AddContent(secContentId, secContentDesc, colorId))
                .ThenNone()
                .Assert();
        }

        [Fact]
        public void add_content_with_same_stream_id_should_be_idempotent()
        {
            string productId = Guid.NewGuid().ToString();
            const string code = "PRD12";
            string firstContentId = Guid.NewGuid().ToString();
            const string firstContentDesc = "%100 Cotton TShirt";
            string colorId = Guid.NewGuid().ToString();

            string secContentId = firstContentId;
            var secContentDesc = "%50 Cotton TShirt";
            string secColorId = Guid.NewGuid().ToString();

            new CommandScenarioFor<Product>(Product.Factory)
                .Given(
                    new Events.V1.ProductCreated(productId, code, 1, 2),
                    new Events.V1.ContentAddedToProduct(productId, firstContentId, firstContentDesc, colorId, (int)Enums.ProductContentStatus.Draft, (int)Enums.VariantType.Color)
                ).When(sut => sut.AddContent(secContentId, secContentDesc, secColorId))
                .ThenNone()
                .Assert();
        }

        [Fact]
        public void add_existing_variant_to_product_shold_be_idempotent()
        {
            string productId = Guid.NewGuid().ToString();
            const string code = "PRD12";
            string contentId = Guid.NewGuid().ToString();
            const string contentDesc = "%100 Cotton TShirt";
            string colorId = Guid.NewGuid().ToString();
            string variantId = Guid.NewGuid().ToString();
            const string barcode = "BARCODE123";
            string sizeId = Guid.NewGuid().ToString();

            new CommandScenarioFor<Product>(Product.Factory)
                .Given(
                    new Events.V1.ProductCreated(productId, code, 1, 2),
                    new Events.V1.ContentAddedToProduct(productId, contentId, contentDesc, colorId, (int)Enums.ProductContentStatus.Draft, (int)Enums.VariantType.Color),
                    new Events.V1.VariantAddedToProduct(productId, contentId, variantId, barcode, sizeId, (int)Enums.VariantType.Size)
                ).When(sut => sut.AddVariant(contentId, variantId, barcode, sizeId))
                .ThenNone()
                .Assert();
        }

        [Fact]
        public void adding_same_barcode_to_the_existing_product_while_variant_creation_should_be_itempotent()
        {
            string productId = Guid.NewGuid().ToString();
            const string code = "PRD12";
            string contentId = Guid.NewGuid().ToString();
            const string contentDesc = "%100 Cotton TShirt";
            string colorId = Guid.NewGuid().ToString();
            string variantId = Guid.NewGuid().ToString();
            const string barcode = "BARCODE123";
            string sizeId = Guid.NewGuid().ToString();

            string anotherVariantId = Guid.NewGuid().ToString();

            new CommandScenarioFor<Product>(Product.Factory)
                .Given(
                    new Events.V1.ProductCreated(productId, code, 1, 2),
                    new Events.V1.ContentAddedToProduct(productId, contentId, contentDesc, colorId, (int)Enums.ProductContentStatus.Draft, (int)Enums.VariantType.Color),
                    new Events.V1.VariantAddedToProduct(productId, contentId, variantId, barcode, sizeId, (int)Enums.VariantType.Size)
                ).When(sut => sut.AddVariant(contentId, anotherVariantId, barcode, sizeId))
                .ThenNone()
                .Assert();
        }

        [Fact]
        public void adding_same_size_to_the_existing_product_while_variant_creation_should_be_itempotent()
        {
            string productId = Guid.NewGuid().ToString();
            const string code = "PRD12";
            string contentId = Guid.NewGuid().ToString();
            const string contentDesc = "%100 Cotton TShirt";
            string colorId = Guid.NewGuid().ToString();
            string variantId = Guid.NewGuid().ToString();
            const string barcode = "BARCODE123";
            string sizeId = Guid.NewGuid().ToString();

            string anotherVariantId = Guid.NewGuid().ToString();
            const string anotherBaroce = "BARCODE1234";

            new CommandScenarioFor<Product>(Product.Factory)
                .Given(
                    new Events.V1.ProductCreated(productId, code, 1, 2),
                    new Events.V1.ContentAddedToProduct(productId, contentId, contentDesc, colorId, (int)Enums.ProductContentStatus.Draft, (int)Enums.VariantType.Color),
                    new Events.V1.VariantAddedToProduct(productId, contentId, variantId, barcode, sizeId, (int)Enums.VariantType.Size)
                ).When(sut => sut.AddVariant(contentId, anotherVariantId, anotherBaroce, sizeId))
                .ThenNone()
                .Assert();
        }
    }
}
