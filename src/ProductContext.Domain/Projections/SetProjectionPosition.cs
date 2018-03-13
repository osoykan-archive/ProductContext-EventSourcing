namespace ProductContext.Domain.Projections
{
    public class SetProjectionPosition
    {
        public readonly long Position;

        public SetProjectionPosition(long position) => Position = position;
    }
}
