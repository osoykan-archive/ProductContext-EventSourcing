using System;

using AggregateSource;

namespace ProductContext.Domain.Products
{
    public partial class Product : AggregateRootEntity
    {
        public static readonly Func<Product> Factory = () => new Product();

        private Product()
        {
            Register<Events.V1.ProductCreated>(When);
        }

        public ProductId ProductId { get; private set; }

        public ProductDetail Detail { get; private set; }

        public static Product Create(string id, int brandId, string code, int genderId, int ageGroupId, int businessUnitId)
        {
            var aggregate = Factory();
            aggregate.ApplyChange(
                new Events.V1.ProductCreated(id, code, brandId, genderId, ageGroupId, businessUnitId)
                );

            return aggregate;
        }

        private void When(Events.V1.ProductCreated @event)
        {
            ProductId = new ProductId(@event.ProductId);
            Detail = new ProductDetail(@event.GenderId, @event.AgeGroupId, @event.BusinessUnitId);
        }
    }
}
