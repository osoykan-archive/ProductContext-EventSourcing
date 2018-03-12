using ProductContext.Common.Bus;

namespace ProductContext.Domain.Commands
{
    public class CreateProductContent : Message
    {
        public string ProductId { get; set; }
    }
}
