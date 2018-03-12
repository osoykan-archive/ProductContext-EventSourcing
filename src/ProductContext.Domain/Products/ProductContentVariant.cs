using System;

using AggregateSource;

namespace ProductContext.Domain.Products
{
    public class ProductContentVariant : Entity
    {
        public ProductContentVariant(Action<object> applier) : base(applier)
        {
        }

        public ProductContentId ProductContentId { get; private set; }

        public ProductContentVariantId ProductContentVariantId { get; private set; }

        public ProductId ProductId { get; private set; }

        public int ItemNumber { get; private set; }

        public string Barcode { get; private set; }
    }
}
