using System.Threading.Tasks;

namespace ProductContext.Framework
{
    public interface IPublisher
    {
        Task PublishAsync(Message message);
    }
}
