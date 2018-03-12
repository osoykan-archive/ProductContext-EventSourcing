using System;

using AggregateSource;

using ProductContext.Domain.Aggregates;
using ProductContext.Domain.Events;

namespace ProductContext.Domain.Entities
{
    public class ProductContentVariantListing : Entity
    {
        public ProductContentVariantListing(Action<object> applier) : base(applier)
        {
            Register<ListingAddedToProductContentVariant>(@event =>
            {
                ProductId = new ProductId(@event.ProductId);
                ProductContentVariantId = new ProductContentVariantId(@event.ProductContentVariantId);
                ListingId = @event.ListingId;
            });
        }

        public ProductId ProductId { get; private set; }

        public ProductContentVariantId ProductContentVariantId { get; private set; }

        public string ListingId { get; private set; }
    }
}
