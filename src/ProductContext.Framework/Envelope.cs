namespace ProductContext.Framework
{
    public class Envelope<TMessage>
    {
        public Envelope(TMessage message, long position)
        {
            Message = message;
            Position = position;
        }

        public TMessage Message { get; }

        public long Position { get; }
    }
}
