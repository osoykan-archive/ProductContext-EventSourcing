using System;
using System.Collections.Generic;

using Value;

namespace ProductContext.Domain.Aggregates
{
    public class ProductContentVariantId : ValueType<ProductContentVariantId>
    {
        public readonly string Id;

        public ProductContentVariantId()
        {
            Id = Guid.NewGuid().ToString();
        }

        public ProductContentVariantId(string id)
        {
            Id = id;
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            return new List<object> { Id };
        }
    }
}
