using System.Collections.Generic;

namespace ProductContext.Domain.Projections
{
    public class ProductDocument
    {
        public ProductDocument()
        {
            Variants = new List<ProductVariantDocument>();
            Contents = new List<ProductContentDocument>();
        }

        public string ProductId { get; set; }

        public string Code { get; set; }

        public int BrandId { get; set; }

        public int BusinessUnitId { get; set; }

        public long Position { get; set; }

        public List<ProductContentDocument> Contents { get; set; }

        public List<ProductVariantDocument> Variants { get; set; }
    }
}
