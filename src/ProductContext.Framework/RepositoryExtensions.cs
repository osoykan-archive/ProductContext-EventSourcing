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
	public static class RepositoryExtensions
	{
		public static async Task AddWhen<T>(this AsyncRepository<T> repo, T aggregate, Now now, Func<T, AsyncRepository<T>, Task> when) where T : IAggregateRootEntity
		{
			await when(aggregate, repo);
			await AppendToStream<T>(repo.UnitOfWork, repo.Connection, now, repo.Configuration.StreamNameResolver);
		}

		public static async Task UpdateWhen<T>(this AsyncSnapshotableRepository<T> repo, string identifier, Now now, Func<T, Task> when) where T : IAggregateRootEntity, ISnapshotable
		{
			T aggregate = await repo.GetAsync(identifier);
			await when(aggregate);

			await AppendToStream<T>(repo.UnitOfWork, repo.Connection, now, repo.Configuration.StreamNameResolver);
		}

		private static async Task AppendToStream<T>(ConcurrentUnitOfWork uow, IEventStoreConnection connection, Now now, IStreamNameResolver getStreamName) where T : IAggregateRootEntity
		{
			foreach (Aggregate aggregate in uow.GetChanges())
			{
				EventData[] changes = aggregate.Root.GetChanges()
				                               .Select(@event => new EventData(
					                               Guid.NewGuid(),
					                               @event.GetType().TypeQualifiedName(),
					                               true,
					                               Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
					                               Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new EventMetadata
					                               {
						                               TimeStamp = now(),
						                               AggregateType = typeof(T).Name,
						                               AggregateAssemblyQualifiedName = typeof(T).AssemblyQualifiedName,
						                               IsSnapshot = false
					                               }))
				                               )).ToArray();
				try
				{
					await connection.AppendToStreamAsync(getStreamName.Resolve(aggregate.Identifier), aggregate.ExpectedVersion, changes);
				}
				catch (WrongExpectedVersionException)
				{
					StreamEventsSlice page = await connection.ReadStreamEventsBackwardAsync(aggregate.Identifier, -1, 1, false);
					throw new WrongExpectedStreamVersionException(
						$"Failed to append stream {aggregate.Identifier} with expected version {aggregate.ExpectedVersion}. " +
						$"{(page.Status == SliceReadStatus.StreamNotFound ? "Stream not found!" : $"Current Version: {page.LastEventNumber}")}");
				}
			}
		}
	}
}
