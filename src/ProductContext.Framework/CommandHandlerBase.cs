using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AggregateSource;
using AggregateSource.EventStore;
using AggregateSource.EventStore.Snapshots;

using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;

using Newtonsoft.Json;

namespace ProductContext.Framework
{
    public class CommandHandlerBase<T> where T : AggregateRootEntity, ISnapshotable
    {
        private readonly Func<DateTime> _getDateTime;
        private readonly GetSnapshotStreamName _getSnapshotStreamName;
        private readonly GetStreamName _getStreamName;
        private readonly AsyncRepository<T> _repository;
        private readonly AsyncSnapshotableRepository<T> _snapshotableRepository;

        public CommandHandlerBase(
            GetStreamName getStreamName,
            GetSnapshotStreamName getSnapshotStreamName,
            AsyncRepository<T> repository,
            AsyncSnapshotableRepository<T> snapshotableRepository,
            Func<DateTime> getDateTime)
        {
            _repository = repository;
            _getDateTime = getDateTime;
            _getStreamName = getStreamName;
            _getSnapshotStreamName = getSnapshotStreamName;
            _snapshotableRepository = snapshotableRepository;
        }

        protected string GetId(string from) => _getStreamName(typeof(T), from);

        protected async Task Add(Func<AsyncRepository<T>, Task> when)
        {
            await when(_repository);

            await AppendToStream();
        }

        protected async Task Update(string id, Func<T, Task> when)
        {
            string stream = _getStreamName(typeof(T), id);

            T loadedAggregate = await _repository.GetAsync(stream);

            await when(loadedAggregate).ConfigureAwait(false);

            await AppendToStream();
        }

        protected async Task UpdateUsingSnapshot(string id, Func<T, Task> when)
        {
            string stream = _getSnapshotStreamName(typeof(T), id);

            T snapshot = await _snapshotableRepository.GetAsync(stream);

            await when(snapshot);

            await AppendToStream();
        }

        private async Task AppendToStream()
        {
            foreach (Aggregate aggregate in _repository.UnitOfWork.GetChanges())
            {
                EventData[] changes = aggregate.Root.GetChanges()
                                               .Select(@event => new EventData(
                                                   Guid.NewGuid(),
                                                   @event.GetType().TypeQualifiedName(),
                                                   true,
                                                   Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
                                                   Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new EventMetadata()
                                                   {
                                                       TimeStamp = _getDateTime(),
                                                       AggregateType = typeof(T).Name,
                                                       AggregateAssemblyQualifiedName = typeof(T).AssemblyQualifiedName
                                                   }))
                                                   )).ToArray();
                try
                {
                    await _repository.Connection.AppendToStreamAsync(aggregate.Identifier, aggregate.ExpectedVersion, changes);
                }
                catch (WrongExpectedVersionException)
                {
                    StreamEventsSlice page = await _repository.Connection.ReadStreamEventsBackwardAsync(aggregate.Identifier, -1, 1, false);
                    throw new WrongExpectedStreamVersionException(
                        $"Failed to append stream {aggregate.Identifier} with expected version {aggregate.ExpectedVersion}. " +
                        $"{(page.Status == SliceReadStatus.StreamNotFound ? "Stream not found!" : $"Current Version: {page.LastEventNumber}")}");
                }
            }
        }
    }
}
