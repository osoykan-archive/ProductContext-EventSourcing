using System;
using System.Threading.Tasks;

namespace ProductContext.Framework
{
    internal interface IMessageHandler
    {
        Task<bool> TryHandleAsync(Message message);

        bool IsSame<T>(object handler);
    }

    internal class MessageHandler<T> : IMessageHandler where T : Message
    {
        private readonly IHandle<T> _handler;

        public MessageHandler(IHandle<T> handler) => _handler = handler ?? throw new ArgumentNullException(nameof(handler));

        public async Task<bool> TryHandleAsync(Message message)
        {
            if (message is T msg)
            {
                await _handler.HandleAsync(msg);
                return true;
            }

            return false;
        }

        public bool IsSame<T2>(object handler) => ReferenceEquals(_handler, handler) && typeof(T) == typeof(T2);
    }
}
