using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductContext.Common.Bus
{
    /// <summary>
    /// Synchronously dispatches messages to zero or more subscribers.
    /// Subscribers are responsible for handling exceptions
    /// </summary>
    public class InMemoryBus : IBus, IPublisher, ISubscriber, IHandle<Message>
    {
        public static InMemoryBus CreateTest()
        {
            return new InMemoryBus();
        }

        public string Name { get; private set; }
        private readonly List<IMessageHandler> _handlers;

        private InMemoryBus() : this("Test")
        {
        }

        public InMemoryBus(string name)
        {
            Name = name;
            _handlers = new List<IMessageHandler>();
        }

        public void Subscribe<T>(IHandle<T> handler) where T : Message
        {
            Ensure.NotNull(handler, "handler");

            var handlers = _handlers;
            if (!handlers.Any(x => x.IsSame<T>(handler)))
                handlers.Add(new MessageHandler<T>(handler, handler.GetType().Name));
        }

        public void Unsubscribe<T>(IHandle<T> handler) where T : Message
        {
            Ensure.NotNull(handler, "handler");

            var handlers = _handlers;
            var messageHandler = handlers.FirstOrDefault(x => x.IsSame<T>(handler));
            if (messageHandler != null)
                handlers.Remove(messageHandler);
        }

        public async Task HandleAsync(Message message)
        {
            await PublishAsync(message).ConfigureAwait(false);
        }

        public async Task PublishAsync(Message message)
        {
            var handlers = _handlers;
            for (int i = 0, n = handlers.Count; i < n; ++i)
            {
                var handler = handlers[i];
                await handler.TryHandleAsync(message).ConfigureAwait(false);
            }
        }
    }
}
