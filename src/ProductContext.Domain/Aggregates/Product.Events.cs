using ProductContext.Domain.Events;
using ProductContext.Domain.Values;

namespace ProductContext.Domain.Aggregates
{
    public partial class Product
    {
        private void When(ProductCreated @event)
        {
            ProductId = new ProductId(@event.ProductId);
            Detail = new ProductDetail(@event.GenderId, @event.AgeGroupId, @event.BusinessUnitId);
        }
    }
}
