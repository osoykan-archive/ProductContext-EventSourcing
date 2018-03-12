using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AggregateSource;
using AggregateSource.EventStore;

using EventStore.ClientAPI;

using Newtonsoft.Json;

namespace ProductContext.Common
{
    public class CommandHandlerBase<T> where T : AggregateRootEntity
    {
        private readonly Func<DateTime> _getDateTime;
        private readonly AsyncRepository<T> _repository;

        public CommandHandlerBase(AsyncRepository<T> repository, Func<DateTime> getDateTime)
        {
            _repository = repository;
            _getDateTime = getDateTime;
        }

        protected async Task Add(Action<AsyncRepository<T>> when)
        {
            when(_repository);

            foreach (Aggregate aggregate in _repository.UnitOfWork.GetChanges())
            {
                await _repository.Connection.AppendToStreamAsync(
                    $"{typeof(T).Name.ToLower()}-{aggregate.Identifier}",
                    aggregate.ExpectedVersion,
                    aggregate.Root.GetChanges()
                             .Select(@event => new EventData(
                                 Guid.NewGuid(),
                                 @event.GetType().TypeQualifiedName(),
                                 true,
                                 Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
                                 Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
                                 {
                                     timeStamp = _getDateTime()
                                 }))
                                 )).ToArray()).ConfigureAwait(false);
            }
        }

        protected async Task Update(string id, Func<T, Task> when)
        {
            T loadedAggregate = await _repository.GetAsync(id).ConfigureAwait(false);

            await when(loadedAggregate).ConfigureAwait(false);

            foreach (Aggregate aggregate in _repository.UnitOfWork.GetChanges())
            {
                await _repository.Connection.AppendToStreamAsync(
                    $"{typeof(T).Name.ToLower()}-{aggregate.Identifier}",
                    aggregate.ExpectedVersion,
                    aggregate.Root.GetChanges()
                             .Select(@event => new EventData(
                                 Guid.NewGuid(),
                                 @event.GetType().TypeQualifiedName(),
                                 true,
                                 Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
                                 Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
                                 {
                                     timeStamp = _getDateTime()
                                 }))
                                 )).ToArray()).ConfigureAwait(false);
            }
        }
    }
}
