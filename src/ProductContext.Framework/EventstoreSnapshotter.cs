using System;
using System.Text;
using System.Threading.Tasks;

using AggregateSource;
using AggregateSource.EventStore;

using EventStore.ClientAPI;

using Newtonsoft.Json;

namespace ProductContext.Framework
{
    public class EventStoreSnapshotter<TAggregate> : ISnapshotter where TAggregate : AggregateRootEntity
    {
        private readonly AsyncRepository<TAggregate> _repository;

        public EventStoreSnapshotter(AsyncRepository<TAggregate> repository) => _repository = repository;

        public bool ShouldTakeSnapshot(Type aggregateType) => true;

        public async Task Take(string stream)
        {
            TAggregate aggregate = await _repository.GetAsync(stream);

            object snapshot = ((ISnapshotable)aggregate).TakeSnapshot();

            var changes = new EventData(
                Guid.NewGuid(),
                aggregate.GetType().TypeQualifiedName(),
                true,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(snapshot)),
                null
                );

            string snapshotStream = $"{stream}-Snapshot";
            await _repository.Connection.AppendToStreamAsync(snapshotStream, ExpectedVersion.Any, changes);
        }
    }
}
