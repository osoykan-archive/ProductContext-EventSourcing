namespace ProductContext.Domain.Contracts
{
    public class Enums
    {
        public enum VariantType
        {
            Size,
            Color
        }
        public enum ProductContentStatus
        {
            Draft = 1,
            Preparing = 2,
            Approved = 3
        }
    }
}
