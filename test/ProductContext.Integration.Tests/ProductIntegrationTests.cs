using System;
using System.Linq;
using System.Threading.Tasks;

using AggregateSource;

using Bogus;

using Couchbase.Core;

using EventStore.ClientAPI;

using ProductContext.Domain.Contracts;
using ProductContext.Domain.Projections;
using ProductContext.Framework;

using Xunit;

namespace ProductContext.Integration.Tests
{
	public class ProductIntegrationTests : ProductIntegrationTestBase
	{
		[Fact]
		public async Task Docker_Couchbase_Should_Work()
		{
			Func<IBucket> getBucket = Defaults.GetCouchbaseBucket(nameof(ProductContext), "Administrator", "password", "http://localhost:8091");

			IBucket bucket = getBucket();

			Assert.NotNull(bucket);
		}

		[Fact]
		public async Task Docker_EventStore_Should_Work()
		{
			IEventStoreConnection esConnection = await Defaults.GetEsConnection("admin", "changeit", "tcp://admin:changeit@127.0.0.1:1113");
			Assert.NotNull(esConnection);
		}

		[Fact]
		public async Task AddProductContent_Should_Append_A_Content_To_Existing_Product()
		{
			//-----------------------------------------------------------------------------------------------------------
			// Arrange
			//-----------------------------------------------------------------------------------------------------------
			await InitializeFixture();
			string productId = Guid.NewGuid().ToString();
			string contentId = Guid.NewGuid().ToString();
			string productCode = new Faker().Commerce.ProductAdjective();

			await Mediator.Send(new Commands.V1.CreateProduct
			{
				ProductId = productId,
				BrandId = 1,
				BusinessUnitId = 1,
				Code = productCode
			});

			//-----------------------------------------------------------------------------------------------------------
			// Act
			//-----------------------------------------------------------------------------------------------------------
			ProductDocument document = Query(x => x.ProductId == productId);
			await Mediator.Send(new Commands.V1.AddContentToProduct
			{
				ProductId = document.ProductId,
				Description = "%100 Cotton",
				VariantTypeValueId = Guid.NewGuid().ToString(),
				ProductContentId = contentId
			});

			//-----------------------------------------------------------------------------------------------------------
			// Assert
			//-----------------------------------------------------------------------------------------------------------
			await WaitUntilProjected(x => x.Contents.Any(c => c.ProductContentId == contentId));
		}

		[Fact]
		public async Task CreateProductCommand_Should_Create_Product()
		{
			//-----------------------------------------------------------------------------------------------------------
			// Arrange
			//-----------------------------------------------------------------------------------------------------------
			await InitializeFixture();
			string productId = new Faker().Random.Uuid().ToString();
			string code = new Faker().Commerce.Product();

			//-----------------------------------------------------------------------------------------------------------
			// Act
			//-----------------------------------------------------------------------------------------------------------
			await Mediator.Send(new Commands.V1.CreateProduct
			{
				ProductId = productId,
				BrandId = 1,
				BusinessUnitId = 1,
				Code = code
			});

			//-----------------------------------------------------------------------------------------------------------
			// Assert
			//-----------------------------------------------------------------------------------------------------------
			await WaitUntilProjected(x => x.ProductId == productId);
		}

		[Fact]
		public async Task AddProductVariant_Should_Add_New_Variant_To_Existing_Product()
		{
			//-----------------------------------------------------------------------------------------------------------
			// Arrange
			//-----------------------------------------------------------------------------------------------------------
			await InitializeFixture();
			string productId = Guid.NewGuid().ToString();
			string contentId = Guid.NewGuid().ToString();
			string productCode = new Faker().Commerce.ProductAdjective();
			string variantId = Guid.NewGuid().ToString();
			string barcode = Guid.NewGuid().ToString();
			string variantTypeValueId = Guid.NewGuid().ToString();
			const string contentDescription = "%100 Cotton";

			await Mediator.Send(new Commands.V1.CreateProduct
			{
				ProductId = productId,
				BrandId = 1,
				BusinessUnitId = 1,
				Code = productCode
			});
			await WaitUntilProjected(p => p.ProductId == productId);

			await Mediator.Send(new Commands.V1.AddContentToProduct
			{
				ProductId = productId,
				Description = contentDescription,
				VariantTypeValueId = variantTypeValueId,
				ProductContentId = contentId
			});
			await WaitUntilProjected(p => p.ProductId == productId && p.Contents.Any(x => x.ProductContentId == contentId));

			//-----------------------------------------------------------------------------------------------------------
			// Act
			//-----------------------------------------------------------------------------------------------------------
			await Mediator.Send(new Commands.V1.AddVariantToProduct
			{
				ProductId = productId,
				VariantId = variantId,
				Barcode = barcode,
				ContentId = contentId,
				VariantTypeValueId = variantTypeValueId
			});

			//-----------------------------------------------------------------------------------------------------------
			// Assert
			//-----------------------------------------------------------------------------------------------------------
			await WaitUntilProjected(p => p.ProductId == productId && p.Variants.Any(v => v.ProductVariantId == variantId));
		}
	}
}
