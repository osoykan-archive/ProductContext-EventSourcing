using System;

using AggregateSource;

using ProductContext.Domain.Events;
using ProductContext.Domain.Values;

namespace ProductContext.Domain.Aggregates
{
    public partial class Product : AggregateRootEntity
    {
        public static readonly Func<Product> Factory = () => new Product();

        private Product()
        {
            Register<ProductCreated>(When);
        }

        public ProductId ProductId { get; private set; }

        public ProductDetail Detail { get; private set; }

        public static Product Create(string id, int brandId, string code, int genderId, int ageGroupId, int businessUnitId)
        {
            var aggregate = Factory();
            aggregate.ApplyChange(
                new ProductCreated(id, code, brandId, genderId, ageGroupId, businessUnitId)
                );

            return aggregate;
        }
    }
}
