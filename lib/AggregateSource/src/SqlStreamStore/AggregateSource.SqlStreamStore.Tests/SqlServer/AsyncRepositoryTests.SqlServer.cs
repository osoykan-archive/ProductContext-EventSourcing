using System;
using System.Threading.Tasks;
using FluentAssertions;
using SqlStreamStore.Streams;
using StreamStoreStore.Json;
using Xunit;
using Xunit.Abstractions;

namespace AggregateSource.SqlStreamStore.Tests.SqlServer
{
    // ReSharper disable UnusedVariable
    public class Construction
    {
        [Fact]
        public async Task FactoryCanNotBeNull()
        {
            using (var fixture = new MsSqlStreamStoreFixture("dbo"))
            {
                using (var store = await fixture.GetStreamStore())
                {
                    Assert.Throws<ArgumentNullException>(
                        () => new AsyncRepository<AggregateRootEntityStub>(
                            null,
                            new ConcurrentUnitOfWork(),
                            store,
                            new EventDeserializer()));
                }
            }
        }

        [Fact]
        public async Task ConcurrentUnitOfWorkCanNotBeNull()
        {
            using (var fixture = new MsSqlStreamStoreFixture("dbo"))
            {
                using (var store = await fixture.GetStreamStore())
                {
                    Assert.Throws<ArgumentNullException>(() =>
                        new AsyncRepository<AggregateRootEntityStub>(
                            AggregateRootEntityStub.Factory, null,
                            store,
                            new EventDeserializer()));
                }
            }
        }

        [Fact]
        public void EventStoreConnectionCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new AsyncRepository<AggregateRootEntityStub>(
                    AggregateRootEntityStub.Factory,
                    new ConcurrentUnitOfWork(),
                    null,
                    new EventDeserializer()));
        }
    }

    public class WithEmptyStoreAndEmptyUnitOfWork
    {
        private AggregateRootEntityStub _root;
        private Model _model;

        public WithEmptyStoreAndEmptyUnitOfWork()
        {
            _model = new Model();
            _root = AggregateRootEntityStub.Factory();
        }

        [Fact]
        public async Task GetAsyncThrows()
        {
            using (var fixture = new MsSqlStreamStoreFixture("dbo"))
            {
                using (var store = await fixture.GetStreamStore())
                {
                    var sut = new RepositoryScenarioBuilder(store)
                        .BuildForAsyncRepository();

                    var exception =
                        await Assert.ThrowsAsync<AggregateNotFoundException>(async () =>
                        {
                            var _ = await sut.GetAsync(_model.UnknownIdentifier);
                        });
                    Assert.Equal(exception.Identifier, _model.UnknownIdentifier);
                    Assert.Equal(exception.ClrType, typeof(AggregateRootEntityStub));
                }
            }
        }

        [Fact]
        public async Task GetOptionalAsyncReturnsEmpty()
        {
            using (var fixture = new MsSqlStreamStoreFixture("dbo"))
            {
                using (var store = await fixture.GetStreamStore())
                {
                    var sut = new RepositoryScenarioBuilder(store)
                        .BuildForAsyncRepository();

                    var result = await sut.GetOptionalAsync(_model.UnknownIdentifier);

                    Assert.Equal(result, Optional<AggregateRootEntityStub>.Empty);
                }
            }
        }

        [Fact]
        public async Task AddAttachesToUnitOfWork()
        {
            using (var fixture = new MsSqlStreamStoreFixture("dbo"))
            {
                using (var store = await fixture.GetStreamStore())
                {
                    var sut = new RepositoryScenarioBuilder(store)
                        .BuildForAsyncRepository();

                    sut.Add(_model.KnownIdentifier, _root);

                    Aggregate aggregate;
                    var result = sut.UnitOfWork.TryGet(_model.KnownIdentifier, out aggregate);
                    Assert.True(result);
                    Assert.Equal(aggregate.Identifier, _model.KnownIdentifier);
                    Assert.Equal(aggregate.Root, _root);
                }
            }
        }
    }

    public class WithEmptyStoreAndFilledUnitOfWork
    {
        private AggregateRootEntityStub _root;
        private Model _model;

        public WithEmptyStoreAndFilledUnitOfWork()
        {
            _model = new Model();
            _root = AggregateRootEntityStub.Factory();
        }

        [Fact]
        public async Task GetAsyncThrowsForUnknownId()
        {
            using (var fixture = new MsSqlStreamStoreFixture("dbo"))
            {
                using (var store = await fixture.GetStreamStore())
                {
                    var sut = new RepositoryScenarioBuilder(store)
                        .ScheduleAttachToUnitOfWork(new Aggregate(_model.KnownIdentifier, 0, _root))
                        .BuildForAsyncRepository();

                    var exception =
                        await Assert.ThrowsAsync<AggregateNotFoundException>(async () =>
                        {
                            var _ = await sut.GetAsync(_model.UnknownIdentifier);
                        });
                    Assert.Equal(exception.Identifier, (_model.UnknownIdentifier));
                    Assert.Equal(exception.ClrType, typeof(AggregateRootEntityStub));
                }
            }
        }

        [Fact]
        public async Task GetAsyncReturnsRootOfKnownId()
        {
            using (var fixture = new MsSqlStreamStoreFixture("dbo"))
            {
                using (var store = await fixture.GetStreamStore())
                {
                    var sut = new RepositoryScenarioBuilder(store)
                        .ScheduleAttachToUnitOfWork(new Aggregate(_model.KnownIdentifier, 0, _root))
                        .BuildForAsyncRepository();

                    var result = await sut.GetAsync(_model.KnownIdentifier);

                    Assert.Same(result, _root);
                }
            }
        }

        [Fact]
        public async Task GetOptionalAsyncReturnsEmptyForUnknownId()
        {
            using (var fixture = new MsSqlStreamStoreFixture("dbo"))
            {
                using (var store = await fixture.GetStreamStore())
                {
                    var sut = new RepositoryScenarioBuilder(store)
                        .ScheduleAttachToUnitOfWork(new Aggregate(_model.KnownIdentifier, 0, _root))
                        .BuildForAsyncRepository();

                    var result = await sut.GetOptionalAsync(_model.UnknownIdentifier);

                    Assert.Equal(result, Optional<AggregateRootEntityStub>.Empty);
                }
            }
        }

        [Fact]
        public async Task GetOptionalAsyncReturnsRootForKnownId()
        {
            using (var fixture = new MsSqlStreamStoreFixture("dbo"))
            {
                using (var store = await fixture.GetStreamStore())
                {
                    var sut = new RepositoryScenarioBuilder(store)
                        .ScheduleAttachToUnitOfWork(new Aggregate(_model.KnownIdentifier, 0, _root))
                        .BuildForAsyncRepository();

                    var result = await sut.GetOptionalAsync(_model.KnownIdentifier);

                    Assert.Equal(result, new Optional<AggregateRootEntityStub>(_root));
                }
            }
        }
    }

    public class WithStreamPresentInStore
    {
        private Model _model;
        private readonly ITestOutputHelper _output;
        private readonly IDisposable _logCapture;

        public WithStreamPresentInStore(ITestOutputHelper output)
        {
            _model = new Model();
            _output = output;
            _logCapture = CaptureLogs(_output);
        }

        private IDisposable CaptureLogs(ITestOutputHelper testOutputHelper)
        {
            return LoggingHelper.Capture(testOutputHelper);
        }

        [Fact]
        public void Can_serialize_and_deserialize_an_event()
        {
            var eventStub = new EventStub(1);
            var streamMessage = new NewStreamMessage(Guid.NewGuid(),
                eventStub.GetType().AssemblyQualifiedName,
                SimpleJson.SerializeObject(eventStub),
                "\"metadata\"");

            var jsonData = streamMessage.JsonData;
            var data = SimpleJson.DeserializeObject<EventStub>(jsonData);

            Assert.Equal(eventStub.Value, data.Value);
        }

        [Fact]
        public async Task GetAsyncThrowsForUnknownId()
        {
            using (var fixture = new MsSqlStreamStoreFixture("dbo"))
            {
                using (var store = await fixture.GetStreamStore())
                {
                    var sut = new RepositoryScenarioBuilder(store)
                        .ScheduleAppendToStream(_model.KnownIdentifier, new EventStub(1))
                        .BuildForAsyncRepository();

                    var exception =
                        await Assert.ThrowsAsync<AggregateNotFoundException>(async () =>
                        {
                            var _ = await sut.GetAsync(_model.UnknownIdentifier);
                        });
                    Assert.Equal(exception.Identifier, _model.UnknownIdentifier);
                    Assert.Equal(exception.ClrType, typeof(AggregateRootEntityStub));
                }
            }
        }

        [Fact]
        public async Task GetReturnsRootOfKnownId()
        {
            using (var fixture = new MsSqlStreamStoreFixture("dbo"))
            {
                using (var store = await fixture.GetMsSqlStreamStore())
                {
                    var sut = new RepositoryScenarioBuilder(store)
                        .ScheduleAppendToStream(_model.KnownIdentifier, new EventStub(1))
                        .BuildForAsyncRepository();

                    var result = await sut.GetAsync(_model.KnownIdentifier);

                    result.RecordedEvents.ShouldBeEquivalentTo(new[] { new EventStub(1) });
                }
            }
        }

        [Fact]
        public async Task GetOptionalAsyncReturnsEmptyForUnknownId()
        {
            using (var fixture = new MsSqlStreamStoreFixture("dbo"))
            {
                using (var store = await fixture.GetStreamStore())
                {
                    var sut = new RepositoryScenarioBuilder(store)
                        .ScheduleAppendToStream(_model.KnownIdentifier, new EventStub(1))
                        .BuildForAsyncRepository();

                    var result = await sut.GetOptionalAsync(_model.UnknownIdentifier);

                    Assert.Equal(result, Optional<AggregateRootEntityStub>.Empty);
                }
            }
        }

        [Fact]
        public async Task GetOptionalAsyncReturnsRootForKnownId()
        {
            using (var fixture = new MsSqlStreamStoreFixture("dbo"))
            {
                using (var store = await fixture.GetMsSqlStreamStore())
                {
                    var sut = new RepositoryScenarioBuilder(store)
                        .ScheduleAppendToStream(_model.KnownIdentifier, new EventStub(1))
                        .BuildForAsyncRepository();

                    var result = await sut.GetOptionalAsync(_model.KnownIdentifier);

                    Assert.True(result.HasValue);
                    result.Value.RecordedEvents.ShouldBeEquivalentTo(new[] { new EventStub(1) });

                }
            }
        }
    }

    public class WithDeletedStreamInStore
    {
        private Model _model;
        public WithDeletedStreamInStore()
        {
            _model = new Model();
        }

        [Fact]
        public async Task GetAsyncThrowsForUnknownId()
        {
            using (var fixture = new MsSqlStreamStoreFixture("dbo"))
            {
                using (var store = await fixture.GetStreamStore())
                {
                    var sut = new RepositoryScenarioBuilder(store)
                        .ScheduleAppendToStream(_model.KnownIdentifier, new EventStub(1))
                        .ScheduleDeleteStream(_model.KnownIdentifier)
                        .BuildForAsyncRepository();

                    var exception =
                        await Assert.ThrowsAsync<AggregateNotFoundException>(async () =>
                        {
                            var _ = await sut.GetAsync(_model.UnknownIdentifier);
                        });
                    Assert.Equal(exception.Identifier, _model.UnknownIdentifier);
                    Assert.Equal(exception.ClrType, typeof(AggregateRootEntityStub));
                }
            }
        }

        [Fact]
        public async Task GetAsyncThrowsForKnownId()
        {
            using (var fixture = new MsSqlStreamStoreFixture("dbo"))
            {
                using (var store = await fixture.GetStreamStore())
                {
                    var sut = new RepositoryScenarioBuilder(store)
                        .ScheduleAppendToStream(_model.KnownIdentifier, new EventStub(1))
                        .ScheduleDeleteStream(_model.KnownIdentifier)
                        .BuildForAsyncRepository();

                    var exception =
                        await Assert.ThrowsAsync<AggregateNotFoundException>(async () =>
                        {
                            var _ = await sut.GetAsync(_model.KnownIdentifier);
                        });
                    Assert.Equal(exception.Identifier, _model.KnownIdentifier);
                    Assert.Equal(exception.ClrType, typeof(AggregateRootEntityStub));
                }
            }
        }

        [Fact]
        public async Task GetOptionalAsyncReturnsEmptyForUnknownId()
        {
            using (var fixture = new MsSqlStreamStoreFixture("dbo"))
            {
                using (var store = await fixture.GetStreamStore())
                {
                    var sut = new RepositoryScenarioBuilder(store)
                        .ScheduleAppendToStream(_model.KnownIdentifier, new EventStub(1))
                        .ScheduleDeleteStream(_model.KnownIdentifier)
                        .BuildForAsyncRepository();

                    var result = await sut.GetOptionalAsync(_model.UnknownIdentifier);

                    Assert.Equal(result, Optional<AggregateRootEntityStub>.Empty);
                }
            }
        }

        [Fact]
        public async Task GetOptionalAsyncReturnsEmptyForKnownId()
        {
            using (var fixture = new MsSqlStreamStoreFixture("dbo"))
            {
                using (var store = await fixture.GetStreamStore())
                {
                    var sut = new RepositoryScenarioBuilder(store)
                        .ScheduleAppendToStream(_model.KnownIdentifier, new EventStub(1))
                        .ScheduleDeleteStream(_model.KnownIdentifier)
                        .BuildForAsyncRepository();

                    var result = await sut.GetOptionalAsync(_model.KnownIdentifier);

                    Assert.Equal(result, Optional<AggregateRootEntityStub>.Empty);
                }
            }
        }
    }
    // ReSharper restore UnusedVariable
}