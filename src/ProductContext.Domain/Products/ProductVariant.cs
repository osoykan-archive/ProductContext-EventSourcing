using System;

using AggregateSource;

namespace ProductContext.Domain.Products
{
    public class ProductVariant : Entity
    {
        public ProductVariant(Action<object> applier) : base(applier)
        {
            Register<Events.V1.VariantAddedToProduct>(When);
        }

        public ProductContentId ProductContentId { get; private set; }

        public ProductVariantId ProductVariantId { get; private set; }

        public ProductId ProductId { get; private set; }

        public string Barcode { get; private set; }

        private void When(Events.V1.VariantAddedToProduct @event)
        {
            ProductContentId = new ProductContentId(@event.ProductContentId);
            ProductVariantId = new ProductVariantId(@event.ProductVariantId);
            ProductId = new ProductId(@event.ProductId);
            Barcode = @event.Barcode;
        }
    }
}
