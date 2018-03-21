using System;
using System.Collections.Generic;
using System.Linq;

using ProductContext.Domain.Products;
using ProductContext.Domain.Products.Snapshots;

namespace ProductContext.Domain.Extensions
{
    public static class ProductSnapshotExtensions
    {
        public static List<ProductContentSnapshot> ToSnapshot(this IEnumerable<ProductContent> items)
        {
            return items.Select(x => new ProductContentSnapshot
            {
                VariantValue = new ProductContentVariantValueSnapshot
                {
                    ProductId = x.ProductId,
                    VariantType = (int)x.VariantValue.VariantType,
                    VariantTypeValueId = x.VariantValue.VariantTypeValueId
                },
                ProductId = x.ProductId,
                ProductContentId = x.ProductContentId,
                Description = x.Description,
                Status = (int)x.Status
            }).ToList();
        }

        public static List<ProductVariantSnapshot> ToSnapshot(this IEnumerable<ProductVariant> items)
        {
            return items.Select(x => new ProductVariantSnapshot
            {
                Barcode = x.Barcode,
                ProductContentId = x.ProductContentId,
                ProductId = x.ProductId,
                ProductVariantId = x.ProductVariantId,
                VariantValue = new ProductVariantTypeValueSnapshot
                {
                    ProductId = x.VariantValue.ProductId,
                    VariantType = (int)x.VariantValue.VariantType,
                    VariantTypeValueId = x.VariantValue.VariantTypeValueId
                }
            }).ToList();
        }

        public static List<ProductVariant> Restore(this IEnumerable<ProductVariantSnapshot> items, Action<object> applier)
        {
            return items.Select(x =>
            {
                var variant = new ProductVariant(applier);
                variant.Route(
                    new Events.V1.VariantAddedToProduct(
                        x.ProductId,
                        x.ProductContentId,
                        x.ProductVariantId,
                        x.Barcode,
                        x.VariantValue.VariantTypeValueId,
                        x.VariantValue.VariantType)
                );
                return variant;
            }).ToList();
        }

        public static List<ProductContent> Restore(this IEnumerable<ProductContentSnapshot> items, Action<object> applier)
        {
            return items.Select(x =>
            {
                var content = new ProductContent(applier);
                content.Route(
                    new Events.V1.ContentAddedToProduct(
                        x.ProductId,
                        x.ProductContentId,
                        x.Description,
                        x.VariantValue.VariantTypeValueId,
                        x.Status,
                        x.VariantValue.VariantType)
                );
                return content;
            }).ToList();
        }
    }
}
