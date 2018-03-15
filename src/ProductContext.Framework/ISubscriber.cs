namespace ProductContext.Framework
{
    public interface ISubscriber
    {
        void Subscribe<T>(IHandle<T> handler) where T : Message;

        void Unsubscribe<T>(IHandle<T> handler) where T : Message;
    }
}
