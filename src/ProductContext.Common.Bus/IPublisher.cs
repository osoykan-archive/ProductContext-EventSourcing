using System.Threading.Tasks;

namespace ProductContext.Common.Bus
{
    public interface IPublisher
    {
        Task PublishAsync(Message message);
    }
}
