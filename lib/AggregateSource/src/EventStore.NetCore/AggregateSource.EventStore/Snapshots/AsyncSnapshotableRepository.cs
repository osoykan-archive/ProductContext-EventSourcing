using System;
using System.Linq;
using System.Threading.Tasks;

using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace AggregateSource.EventStore.Snapshots
{
	/// <summary>
	///     Represents an asynchronous, virtual collection of <typeparamref name="TAggregateRoot" />.
	/// </summary>
	/// <typeparam name="TAggregateRoot">The type of the aggregate root in this collection.</typeparam>
	public class AsyncSnapshotableRepository<TAggregateRoot> : IAsyncRepository<TAggregateRoot>
		where TAggregateRoot : IAggregateRootEntity, ISnapshotable
	{
		private readonly IAsyncSnapshotReader _reader;
		private readonly Func<TAggregateRoot> _rootFactory;

		/// <summary>
		///     Initializes a new instance of the <see cref="AsyncRepository{TAggregateRoot}" /> class.
		/// </summary>
		/// <param name="rootFactory">The aggregate root entity factory.</param>
		/// <param name="unitOfWork">The unit of work to interact with.</param>
		/// <param name="connection">The event store connection to use.</param>
		/// <param name="configuration">The event store configuration to use.</param>
		/// <param name="reader">The snapshot reader to use.</param>
		/// <exception cref="System.ArgumentNullException">
		///     Thrown when the <paramref name="rootFactory" /> or
		///     <paramref name="unitOfWork" /> or <paramref name="connection" /> or <paramref name="configuration" /> or
		///     <paramref name="reader" /> is null.
		/// </exception>
		public AsyncSnapshotableRepository(
			Func<TAggregateRoot> rootFactory,
			ConcurrentUnitOfWork unitOfWork,
			IEventStoreConnection connection, EventReaderConfiguration configuration,
			IAsyncSnapshotReader reader)
		{
			_rootFactory = rootFactory ?? throw new ArgumentNullException(nameof(rootFactory));
			UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			Connection = connection ?? throw new ArgumentNullException(nameof(connection));
			Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_reader = reader ?? throw new ArgumentNullException(nameof(reader));
		}

		public ConcurrentUnitOfWork UnitOfWork { get; }

		public IEventStoreConnection Connection { get; }

		public EventReaderConfiguration Configuration { get; }

		/// <summary>
		///     Gets the aggregate root entity associated with the specified aggregate identifier.
		/// </summary>
		/// <param name="identifier">The aggregate identifier.</param>
		/// <returns>An instance of <typeparamref name="TAggregateRoot" />.</returns>
		/// <exception cref="AggregateNotFoundException">Thrown when an aggregate is not found.</exception>
		public async Task<TAggregateRoot> GetAsync(string identifier)
		{
			Optional<TAggregateRoot> result = await GetOptionalAsync(identifier);
			if (!result.HasValue)
			{
				throw new AggregateNotFoundException(identifier, typeof(TAggregateRoot));
			}

			return result.Value;
		}

		/// <summary>
		///     Attempts to get the aggregate root entity associated with the aggregate identifier.
		/// </summary>
		/// <param name="identifier">The aggregate identifier.</param>
		/// <returns>The found <typeparamref name="TAggregateRoot" />, or empty if not found.</returns>
		public async Task<Optional<TAggregateRoot>> GetOptionalAsync(string identifier)
		{
			if (UnitOfWork.TryGet(identifier, out Aggregate aggregate))
			{
				return new Optional<TAggregateRoot>((TAggregateRoot)aggregate.Root);
			}

			Optional<Snapshot> snapshot = await _reader.ReadOptionalAsync(identifier);
			var version = StreamPosition.Start;
			if (snapshot.HasValue)
			{
				version = snapshot.Value.Version + 1;
			}

			UserCredentials streamUserCredentials = Configuration.StreamUserCredentialsResolver.Resolve(identifier);
			string streamName = Configuration.StreamNameResolver.Resolve(identifier);
			StreamEventsSlice slice =
				await
					Connection.ReadStreamEventsForwardAsync(streamName, version, Configuration.SliceSize, false,
						streamUserCredentials);
			if (slice.Status == SliceReadStatus.StreamDeleted || slice.Status == SliceReadStatus.StreamNotFound)
			{
				return Optional<TAggregateRoot>.Empty;
			}

			TAggregateRoot root = _rootFactory();
			if (snapshot.HasValue)
			{
				root.RestoreSnapshot(snapshot.Value.State);
			}

			root.Initialize(slice.Events.Select(resolved => Configuration.Deserializer.Deserialize(resolved)));
			while (!slice.IsEndOfStream)
			{
				slice =
					await
						Connection.ReadStreamEventsForwardAsync(streamName, slice.NextEventNumber, Configuration.SliceSize,
							false, streamUserCredentials);
				root.Initialize(slice.Events.Select(resolved => Configuration.Deserializer.Deserialize(resolved)));
			}

			aggregate = new Aggregate(identifier, (int)slice.LastEventNumber, root);
			UnitOfWork.Attach(aggregate);
			return new Optional<TAggregateRoot>(root);
		}

		/// <summary>
		///     Adds the aggregate root entity to this collection using the specified aggregate identifier.
		/// </summary>
		/// <param name="identifier">The aggregate identifier.</param>
		/// <param name="root">The aggregate root entity.</param>
		public void Add(string identifier, TAggregateRoot root)
		{
			UnitOfWork.Attach(new Aggregate(identifier, (int)ExpectedVersion.NoStream, root));
		}
	}
}
