using System;
using System.Collections.Generic;

using Value;

namespace ProductContext.Domain.Aggregates
{
    public class ProductContentId : ValueType<ProductContentId>
    {
        public readonly string Id;

        public ProductContentId()
        {
            Id = Guid.NewGuid().ToString();
        }

        public ProductContentId(string id)
        {
            Id = id;
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            return new List<object> { Id };
        }
    }
}
