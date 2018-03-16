using System;

using AggregateSource;

using ProductContext.Domain.Contracts;

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

        public ProductVariantTypeValue VariantValue { get; private set; }

        public string Barcode { get; private set; }

        private void When(Events.V1.VariantAddedToProduct @event)
        {
            ProductContentId = new ProductContentId(@event.ProductContentId);
            ProductVariantId = new ProductVariantId(@event.ProductVariantId);
            ProductId = new ProductId(@event.ProductId);
            VariantValue = new ProductVariantTypeValue(@event.ProductId, @event.VariantTypeValueId, (Enums.VariantType)@event.VariantType);
            Barcode = @event.Barcode;
        }
    }
}
