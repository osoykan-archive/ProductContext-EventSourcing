using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductContext.Framework
{
    /// <summary>
    ///     Synchronously dispatches messages to zero or more subscribers.
    ///     Subscribers are responsible for handling exceptions
    /// </summary>
    public class InMemoryBus : IBus, IPublisher, ISubscriber, IHandle<Message>
    {
        private readonly List<IMessageHandler> _handlers;

        public InMemoryBus() => _handlers = new List<IMessageHandler>();

        public void Subscribe<T>(IHandle<T> handler) where T : Message
        {
            Ensure.NotNull(handler, "handler");

            List<IMessageHandler> handlers = _handlers;
            if (!handlers.Any(x => x.IsSame<T>(handler)))
            {
                handlers.Add(new MessageHandler<T>(handler));
            }
        }

        public void Unsubscribe<T>(IHandle<T> handler) where T : Message
        {
            Ensure.NotNull(handler, "handler");

            List<IMessageHandler> handlers = _handlers;
            IMessageHandler messageHandler = handlers.FirstOrDefault(x => x.IsSame<T>(handler));
            if (messageHandler != null)
            {
                handlers.Remove(messageHandler);
            }
        }

        public async Task PublishAsync(Message message)
        {
            List<IMessageHandler> handlers = _handlers;
            for (int i = 0, n = handlers.Count; i < n; ++i)
            {
                IMessageHandler handler = handlers[i];
                await handler.TryHandleAsync(message);
            }
        }

        public async Task HandleAsync(Message message)
        {
            await PublishAsync(message);
        }
    }
}
