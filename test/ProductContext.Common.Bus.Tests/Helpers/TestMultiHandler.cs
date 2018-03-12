using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductContext.Common.Bus.Tests.Helpers
{
    public class TestMultiHandler : IHandle<TestMessage>, IHandle<TestMessage2>, IHandle<TestMessage3>
    {
        public readonly List<Message> HandledMessages = new List<Message>();

        public async Task HandleAsync(TestMessage message)
        {
            await Task.Run(() => HandledMessages.Add(message));
        }

        public async Task HandleAsync(TestMessage2 message)
        {
            await Task.Run(() => HandledMessages.Add(message));
        }

        public async Task HandleAsync(TestMessage3 message)
        {
            await Task.Run(() => HandledMessages.Add(message));
        }
    }
}