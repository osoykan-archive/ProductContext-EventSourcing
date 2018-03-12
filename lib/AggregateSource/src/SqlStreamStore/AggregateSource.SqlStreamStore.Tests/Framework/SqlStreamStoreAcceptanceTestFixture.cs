using System;
using System.Threading.Tasks;
using SqlStreamStore;
using SqlStreamStore.Infrastructure;

namespace AggregateSource.SqlStreamStore.Tests
{
    public abstract class StreamStoreAcceptanceTestFixture : IDisposable
    {
        public abstract Task<IStreamStore> GetStreamStore();

        public GetUtcNow GetUtcNow { get; set; } = SystemClock.GetUtcNow;

        public virtual void Dispose()
        { }

        public abstract long MinPosition { get; }
    }
}
