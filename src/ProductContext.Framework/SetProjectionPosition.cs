using EventStore.ClientAPI;

namespace ProductContext.Framework
{
    public class SetProjectionPosition
    {
        public readonly Position? Position;

        public SetProjectionPosition(Position? position) => Position = position;
    }
}
