using System;

using AggregateSource;

using ProductContext.Domain.Contracts;

namespace ProductContext.Domain.Products
{
    public class ProductContent : Entity
    {
        public ProductContent(Action<object> applier) : base(applier)
        {
            Register<Events.V1.ContentAddedToProduct>(When);
        }

        public ProductId ProductId { get; private set; }

        public ProductContentId ProductContentId { get; private set; }

        public ProductContentVariantValue VariantValue { get; private set; }

        public string Description { get; private set; }

        public Enums.ProductContentStatus Status { get; private set; }

        private void When(Events.V1.ContentAddedToProduct @event)
        {
            ProductContentId = new ProductContentId(@event.ProductContentId);
            ProductId = new ProductId(@event.ProductId);
            VariantValue = new ProductContentVariantValue(ProductId, new VariantTypeValueId(@event.VariantTypeValueId), (Enums.VariantType)@event.VariantType);
            Status = (Enums.ProductContentStatus)@event.ProductContentStatus;
            Description = @event.Description;
        }
    }
}
