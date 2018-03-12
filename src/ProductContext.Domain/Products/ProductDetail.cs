using System.Collections.Generic;

using Value;

namespace ProductContext.Domain.Products
{
    public class ProductDetail : ValueType<ProductDetail>
    {
        public readonly int AgeGroupId;
        public readonly int BusinessUnitId;
        public readonly string Composition;
        public readonly string Description;
        public readonly int GenderId;
        public readonly string Material;
        public readonly string Properties;
        public readonly string SampleModelSize;
        public readonly string SampleSize;
        public readonly int StockUnitId;
        public readonly string SupplierProductCode;
        public readonly string Title;
        public readonly int TrendLevelId;
        public readonly decimal Weight;

        public ProductDetail(int genderId, int ageGroupId, int businessUnitId)
        {
            GenderId = genderId;
            AgeGroupId = ageGroupId;
            BusinessUnitId = businessUnitId;
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality() => new List<object>
        {
            GenderId,
            BusinessUnitId,
            AgeGroupId,
            Composition,
            Description,
            Material,
            Properties,
            SampleModelSize,
            SampleSize,
            StockUnitId,
            SupplierProductCode,
            Title,
            TrendLevelId,
            Weight
        };
    }
}
