using System;

using AggregateSource;

using ProductContext.Domain.Aggregates;
using ProductContext.Domain.Enums;
using ProductContext.Domain.Events;

namespace ProductContext.Domain.Entities
{
    public class ProductContentKnownDefiner : Entity
    {
        public ProductContentKnownDefiner(Action<object> applier) : base(applier)
        {
            Register<KnownDefinerAddedToProductContent>(@event =>
            {
                ProductContentId = new ProductContentId(@event.ProductContentId);
                ProductContentKnownDefinerId = new ProductContentKnownDefinerId(@event.ProductContentKnownDefinerId);
                Value = @event.Value;
                ProductContentKnownDefinerType = (ProductContentKnownDefinerType)@event.ProductContentKnownDefinerType;
            });
        }

        public ProductContentKnownDefinerId ProductContentKnownDefinerId { get; private set; }

        public ProductContentId ProductContentId { get; private set; }

        public string Value { get; private set; }

        public ProductContentKnownDefinerType ProductContentKnownDefinerType { get; private set; }
    }
}
