using System.Threading.Tasks;

namespace ProductContext.Framework
{
    public interface IHandle<T> where T : Message
    {
        Task HandleAsync(T message);
    }
}
