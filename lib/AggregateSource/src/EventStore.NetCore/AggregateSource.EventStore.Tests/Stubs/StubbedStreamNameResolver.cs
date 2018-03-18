namespace AggregateSource.EventStore.Tests.Stubs
{
    public class StubbedStreamNameResolver : IStreamNameResolver
    {
        public static readonly IStreamNameResolver Instance = new StubbedStreamNameResolver();

        private StubbedStreamNameResolver()
        {
        }

        public string Resolve(string identifier) => null;
    }
}
