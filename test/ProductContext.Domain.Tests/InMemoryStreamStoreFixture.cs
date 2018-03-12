using System.Threading.Tasks;

using SqlStreamStore;

namespace ProductContext.Domain.Tests
{
    public class InMemoryStreamStoreFixture : StreamStoreAcceptanceTestFixture
    {
        public override long MinPosition => 0;

        public override Task<IStreamStore> GetStreamStore()
        {
            IStreamStore streamStore = new InMemoryStreamStore(() => GetUtcNow());
            return Task.FromResult(streamStore);
        }
    }
}
