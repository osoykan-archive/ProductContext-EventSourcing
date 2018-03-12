using System.Collections.Generic;

using ProductContext.Domain.Entities;
using ProductContext.Domain.Enums;
using ProductContext.Domain.Events;

namespace ProductContext.Domain.Aggregates
{
    public partial class ProductContent
    {
        private void When(ProductContentCreated @event)
        {
            ProductId = new ProductId(@event.ProductId);
            ProductContentId = new ProductContentId(@event.ProductContentId);
            Description = @event.Description;
            Status = (ProductContentStatus)@event.ProductContentStatus;
            IsOnBlackList = @event.IsOnBlackList;
            KnownDefiners = new List<ProductContentKnownDefiner>();
        }

        private void When(KnownDefinerAddedToProductContent @event)
        {
            var entity = new ProductContentKnownDefiner(ApplyChange);
            entity.Route(@event);
            KnownDefiners.Add(entity);
        }
    }
}
