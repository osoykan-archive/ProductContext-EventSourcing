using System;
using System.Threading.Tasks;

using Couchbase;
using Couchbase.Core;

using EventStore.ClientAPI;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ProductContext.Framework
{
    public class CouchbaseCheckpointStore : ICheckpointStore
    {
        private readonly Func<IBucket> _getSession;

        public CouchbaseCheckpointStore(Func<IBucket> getConnection) => _getSession = getConnection;

        public async Task<T> GetLastCheckpoint<T>(string projection)
        {
            using (IBucket conn = _getSession())
            {
                IOperationResult<CheckpointDocument> position = await conn.GetAsync<CheckpointDocument>(GetCheckpointDocumentId(projection));
                if (position.Value != null)
                {
                    return ((JObject)position.Value.Checkpoint).ToObject<T>();
                }

                return default(T);
            }
        }

        public async Task SetLastCheckpoint<T>(string projection, T checkpoint)
        {
            using (IBucket session = _getSession())
            {
                string id = GetCheckpointDocumentId(projection);
                IOperationResult<CheckpointDocument> doc = await session.GetAsync<CheckpointDocument>(id);

                if (doc.Value != null)
                {
                    doc.Value.Checkpoint = checkpoint;
                }
                else
                {
                    await session.UpsertAsync(new Document<CheckpointDocument>
                    {
                        Content = new CheckpointDocument { Checkpoint = checkpoint },
                        Id = id
                    });
                }
            }
        }

        public static string GetCheckpointDocumentId(string projection) => $"checkpoints/{projection.ToLowerInvariant()}";
    }

    public class CheckpointDocument
    {
        public object Checkpoint { get; set; }
    }

    public interface ICheckpointStore
    {
        Task<T> GetLastCheckpoint<T>(string projection);

        Task SetLastCheckpoint<T>(string projection, T checkpoint);
    }
}
