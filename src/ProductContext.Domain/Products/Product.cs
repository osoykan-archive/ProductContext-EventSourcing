using System;
using System.Collections.Generic;
using System.Linq;

using AggregateSource;

using ProductContext.Domain.Contracts;
using ProductContext.Domain.Extensions;
using ProductContext.Domain.Products.Snapshots;

namespace ProductContext.Domain.Products
{
    public class Product : AggregateRootEntity, ISnapshotable
    {
        public static readonly Func<Product> Factory = () => new Product();

        private Product()
        {
            Register<Events.V1.ProductCreated>(When);
            Register<Events.V1.VariantAddedToProduct>(When);
            Register<Events.V1.ContentAddedToProduct>(When);
        }

        public ProductId ProductId { get; private set; }

        public int BusinessUnitId { get; private set; }

        public string Code { get; private set; }

        public int BrandId { get; private set; }

        public List<ProductContent> Contents { get; private set; }

        public List<ProductVariant> Variants { get; private set; }

        public void RestoreSnapshot(object state)
        {
            var snapshot = (ProductSnapshot)state;

            Variants = snapshot.Variants.Restore(ApplyChange);
            Contents = snapshot.Contents.Restore(ApplyChange);
            Code = snapshot.Code;
            BrandId = snapshot.BrandId;
            ProductId = snapshot.ProductId;
        }

        public object TakeSnapshot() => new ProductSnapshot
        {
            BrandId = BrandId,
            Variants = Variants.ToSnapshot(),
            ProductId = ProductId,
            Contents = Contents.ToSnapshot(),
            BusinessUnitId = BusinessUnitId,
            Code = Code
        };

        public static Product Create(string id, int brandId, string code, int businessUnitId)
        {
            Product aggregate = Factory();
            aggregate.ApplyChange(
                new Events.V1.ProductCreated(id, code, brandId, businessUnitId)
                );

            return aggregate;
        }

        private void When(Events.V1.ContentAddedToProduct @event)
        {
            var content = new ProductContent(ApplyChange);
            content.Route(@event);
            Contents.Add(content);
        }

        private void When(Events.V1.VariantAddedToProduct @event)
        {
            var variant = new ProductVariant(ApplyChange);
            variant.Route(@event);
            Variants.Add(variant);
        }

        private void When(Events.V1.ProductCreated @event)
        {
            ProductId = new ProductId(@event.ProductId);
            BusinessUnitId = @event.BusinessUnitId;
            Code = @event.ProductCode;
            BrandId = @event.BrandId;
            Contents = new List<ProductContent>();
            Variants = new List<ProductVariant>();
        }

        public void AddContent(string contentId, string description, string variantTypeValueId)
        {
            if (Contents.Any(x => x.VariantValue.VariantTypeValueId == (VariantTypeValueId)variantTypeValueId))
            {
                return;
            }

            if (Contents.Any(x => x.ProductContentId == contentId))
            {
                return;
            }

            ApplyChange(
                new Events.V1.ContentAddedToProduct(ProductId.Id,
                    contentId,
                    description,
                    variantTypeValueId,
                    (int)Enums.ProductContentStatus.Draft,
                    (int)Enums.VariantType.Color)
                );
        }

        public void AddVariant(string contentId, string variantId, string barcode, string variantTypeValueId)
        {
            ProductContent content = Contents.FirstOrDefault(x => x.ProductContentId == contentId);
            if (content == null)
            {
                throw new InvalidOperationException("Content not found for Variant creation");
            }

            if (Variants.Any(x => x.ProductVariantId == variantId)) { return; }

            if (Variants.Any(x => x.Barcode == barcode)) { return; }

            if (Variants.Any(x => x.VariantValue.VariantTypeValueId == variantTypeValueId)) { return; }

            ApplyChange(
                new Events.V1.VariantAddedToProduct(ProductId.Id, contentId, variantId, barcode, variantTypeValueId, (int)Enums.VariantType.Size)
                );
        }
    }
}
