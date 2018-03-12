using System;
using System.Collections.Generic;

using Value;

namespace ProductContext.Domain.Entities
{
    public class ProductContentKnownDefinerId : ValueType<ProductContentKnownDefinerId>
    {
        public readonly string Id;

        public ProductContentKnownDefinerId()
        {
            Id = Guid.NewGuid().ToString();
        }

        public ProductContentKnownDefinerId(string id)
        {
            Id = id;
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            return new List<object> { Id };
        }
    }
}
