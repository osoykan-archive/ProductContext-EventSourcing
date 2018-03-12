using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductContext.Common.Bus.Tests.Helpers
{
    public class TestHandler<T> : IHandle<T> where T : Message
    {
        public readonly List<T> HandledMessages = new List<T>();

        public async Task HandleAsync(T message)
        {
            await Task.Run(() => HandledMessages.Add(message));
        }
    }
}