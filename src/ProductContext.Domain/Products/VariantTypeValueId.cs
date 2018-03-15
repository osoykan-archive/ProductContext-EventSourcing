using System.Collections.Generic;

using Value;

namespace ProductContext.Domain.Products
{
    public class VariantTypeValueId : ValueType<VariantTypeValueId>
    {
        public readonly string Id;

        public VariantTypeValueId(string id)
        {
            Id = id;
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality() => new List<object> { Id };

        public override string ToString() => Id;

        public static implicit operator string(VariantTypeValueId self) => self.Id;

        public static implicit operator VariantTypeValueId(string value) => new VariantTypeValueId(value);
    }
}
