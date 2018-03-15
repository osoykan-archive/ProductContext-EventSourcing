using System.Collections.Generic;

using Value;

namespace ProductContext.Domain.Products
{
    public class ProductId : ValueType<ProductId>
    {
        public readonly string Id;

        public ProductId(string id) => Id = id;

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality() => new List<object> { Id };

        public override string ToString() => Id;

        public static implicit operator string(ProductId self) => self.Id;

        public static implicit operator ProductId(string value) => new ProductId(value);
    }
}
