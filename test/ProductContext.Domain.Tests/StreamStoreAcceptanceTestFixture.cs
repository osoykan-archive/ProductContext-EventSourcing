using System;
using System.Threading.Tasks;

using SqlStreamStore;
using SqlStreamStore.Infrastructure;

namespace ProductContext.Domain.Tests
{
    public abstract class StreamStoreAcceptanceTestFixture : IDisposable
    {
        public GetUtcNow GetUtcNow { get; set; } = SystemClock.GetUtcNow;

        public abstract long MinPosition { get; }

        public virtual void Dispose()
        {
        }

        public abstract Task<IStreamStore> GetStreamStore();
    }
}
