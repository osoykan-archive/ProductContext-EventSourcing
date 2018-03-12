using System.Threading.Tasks;

namespace ProductContext.Common.Bus
{
    public interface IHandle<T> where T : Message
    {
        Task HandleAsync(T message);
    }
}