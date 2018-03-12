using System;

using AggregateSource.Testing;

using ProductContext.Domain.Aggregates;
using ProductContext.Domain.Events;

using Xunit;

namespace ProductContext.Domain.Tests.Scenarios
{
    public class when_creating_a_product
    {
        [Fact]
        public void should_create_a_product()
        {
            string productId = Guid.NewGuid().ToString();
            var message = new ProductCreated(productId, "PRDCT1234", 1, 1, 1, 6);
            new ConstructorScenarioFor<Product>(() =>
                    Product.Create(
                        message.ProductId,
                        message.BrandId,
                        message.ProductCode,
                        message.GenderId,
                        message.AgeGroupId,
                        message.BusinessUnitId)
                ).Then(message)
                 .Assert();
        }
    }
}
