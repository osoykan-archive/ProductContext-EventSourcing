namespace ProductContext.Domain.Projections
{
    public class EventProjected
    {
        public readonly string Projection;
        public readonly string Position;

        public EventProjected(string projection, string position)
        {
            Projection = projection;
            Position = position;
        }
    }
}
