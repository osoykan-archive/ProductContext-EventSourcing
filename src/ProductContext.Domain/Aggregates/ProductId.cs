using System;
using System.Collections.Generic;

using Value;

namespace ProductContext.Domain.Aggregates
{
    public class ProductId : ValueType<ProductId>
    {
        public readonly string Id;

        public ProductId()
        {
            Id = Guid.NewGuid().ToString();
        }

        public ProductId(string id)
        {
            Id = id;
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            return new List<object> { Id };
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
