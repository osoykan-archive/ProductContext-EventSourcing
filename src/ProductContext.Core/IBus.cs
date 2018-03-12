namespace ProductContext.Framework
{
    public interface IBus : IPublisher, ISubscriber
    {
        string Name { get; }
    }
}
