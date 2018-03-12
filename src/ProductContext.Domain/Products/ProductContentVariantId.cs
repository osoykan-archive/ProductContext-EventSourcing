using System;
using System.Collections.Generic;

using Value;

namespace ProductContext.Domain.Products
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

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality() => new List<object> { Id };

        public static implicit operator string(ProductContentVariantId self) => self.Id;

        public static implicit operator ProductContentVariantId(string value) => new ProductContentVariantId(value);
    }
}
