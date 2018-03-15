using System.Collections.Generic;

using Value;

namespace ProductContext.Domain.Products
{
    public class ProductVariantId : ValueType<ProductVariantId>
    {
        public readonly string Id;

        public ProductVariantId(string id)
        {
            Id = id;
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality() => new List<object> { Id };

        public static implicit operator string(ProductVariantId self) => self.Id;

        public static implicit operator ProductVariantId(string value) => new ProductVariantId(value);
    }
}
