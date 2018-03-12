using System;

using AggregateSource;

using ProductContext.Domain.Shared;

namespace ProductContext.Domain.Products
{
    public class ProductContent : Entity
    {
        public ProductContent(Action<object> applier) : base(applier)
        {
        }

        public ProductId ProductId { get; private set; }

        public ProductContentId ProductContentId { get; private set; }

        public string Description { get; private set; }

        public ProductContentStatus Status { get; private set; }

        public bool IsOnBlackList { get; private set; }
    }
}
