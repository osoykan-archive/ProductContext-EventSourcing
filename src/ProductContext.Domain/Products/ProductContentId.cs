using System;
using System.Collections.Generic;

using Value;

namespace ProductContext.Domain.Products
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

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality() => new List<object> { Id };

        public static implicit operator string(ProductContentId self) => self.Id;

        public static implicit operator ProductContentId(string value) => new ProductContentId(value);
    }
}
