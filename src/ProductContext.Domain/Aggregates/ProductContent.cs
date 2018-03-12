using System;
using System.Collections.Generic;

using AggregateSource;

using ProductContext.Domain.Entities;
using ProductContext.Domain.Enums;
using ProductContext.Domain.Events;

namespace ProductContext.Domain.Aggregates
{
    public partial class ProductContent : AggregateRootEntity
    {
        public static readonly Func<ProductContent> Factory = () => new ProductContent();

        private ProductContent()
        {
            Register<ProductContentCreated>(When);
            Register<KnownDefinerAddedToProductContent>(When);
        }

        public ProductId ProductId { get; private set; }

        public ProductContentId ProductContentId { get; private set; }

        public string Description { get; private set; }

        public ProductContentStatus Status { get; private set; }

        public bool IsOnBlackList { get; private set; }

        public List<ProductContentKnownDefiner> KnownDefiners { get; private set; }

        public static ProductContent Create(string productId, string productContentId, string description)
        {
            ProductContent aggregte = Factory();

            aggregte.ApplyChange(
                new ProductContentCreated(productId, productContentId, description, (int)ProductContentStatus.Draft, false)
                );

            return aggregte;
        }

        public void AddKnownDefiner(string productContentKnownDefinerId, ProductContentKnownDefinerType definerType, string value)
        {
            ApplyChange(
                new KnownDefinerAddedToProductContent(productContentKnownDefinerId, ProductContentId.Id, (int)definerType, value, value.GetHashCode().ToString())
                );
        }
    }
}
