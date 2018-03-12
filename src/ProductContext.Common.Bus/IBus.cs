namespace ProductContext.Common.Bus
{
    public interface IBus : IPublisher, ISubscriber
    {
        string Name { get; }
    }
}
