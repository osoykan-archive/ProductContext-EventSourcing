using System.Collections.Generic;

using ProductContext.Domain.Contracts;

using Value;

namespace ProductContext.Domain.Products
{
    public class ProductVariantTypeValue : ValueType<ProductVariantTypeValue>
    {
        public ProductVariantTypeValue(ProductId productId, VariantTypeValueId variantTypeValueId, Enums.VariantType variantType)
        {
            ProductId = productId;
            VariantTypeValueId = variantTypeValueId;
            VariantType = variantType;
        }

        public ProductId ProductId { get; }

        public VariantTypeValueId VariantTypeValueId { get; }

        public Enums.VariantType VariantType { get; }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality() => new List<object> { ProductId, VariantTypeValueId, VariantType };
    }
}
