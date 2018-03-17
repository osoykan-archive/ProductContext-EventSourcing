using System.Collections.Generic;

using AggregateSource.EventStore.Builders;

using NUnit.Framework;

namespace AggregateSource.EventStore.Snapshots
{
    [TestFixture]
    public class SnapshotTests
    {
        private SnapshotBuilder _sutBuilder;

        private static IEnumerable<object> StateObjects
        {
            get
            {
                yield return null;
                yield return new object();
            }
        }

        [SetUp]
        public void SetUp()
        {
            _sutBuilder = SnapshotBuilder.Default;
        }

        private Snapshot CreateSut() => _sutBuilder.Build();

        private Snapshot CreateSutWithVersion(int version) => _sutBuilder.WithVersion(version).Build();

        private Snapshot CreateSutWithState(object state) => _sutBuilder.WithState(state).Build();

        [Test]
        public void DoesNotEqualNull()
        {
            Assert.IsFalse(CreateSut().Equals(null));
        }

        [Test]
        public void TwoInstancesAreEqualIfTheyHaveTheSameVersionAndState()
        {
            Assert.That(
                CreateSut(),
                Is.EqualTo(CreateSut()));
        }

        [Test]
        public void TwoInstancesAreEqualIfTheyHaveTheSameVersionAndStateIsNullForBoth()
        {
            Assert.That(
                CreateSutWithState(null),
                Is.EqualTo(CreateSutWithState(null)));
        }

        [Test]
        public void TwoInstancesAreNotEqualIfTheirStateDiffers()
        {
            Assert.That(
                CreateSutWithState(new object()),
                Is.Not.EqualTo(CreateSutWithState(new object())));
        }

        [Test]
        public void TwoInstancesAreNotEqualIfTheirVersionDiffers()
        {
            Assert.That(
                CreateSutWithVersion(1),
                Is.Not.EqualTo(CreateSutWithVersion(2)));
        }

        [Test]
        public void TwoInstancesHaveDifferentHashCodesIfTheirStateDiffers()
        {
            Assert.That(
                CreateSutWithState(new object()).GetHashCode(),
                Is.Not.EqualTo(CreateSutWithState(new object()).GetHashCode()));
        }

        [Test]
        public void TwoInstancesHaveDifferentHashCodesIfTheirVersionDiffers()
        {
            Assert.That(
                CreateSutWithVersion(1).GetHashCode(),
                Is.Not.EqualTo(CreateSutWithVersion(2).GetHashCode()));
        }

        [Test]
        public void TwoInstancesHaveTheSameHashCodeIfTheyHaveTheSameVersionAndState()
        {
            Assert.That(
                CreateSut().GetHashCode(),
                Is.EqualTo(CreateSut().GetHashCode()));
        }

        [Test]
        public void TwoInstancesHaveTheSameHashCodeIfTheyHaveTheSameVersionAndStateIsNullForBoth()
        {
            Assert.That(
                CreateSutWithState(null).GetHashCode(),
                Is.EqualTo(CreateSutWithState(null).GetHashCode()));
        }

        [Test]
        [Combinatorial]
        public void UsingConstructorReturnsInstanceWithExpectedProperties(
            [Values(int.MinValue, -1, 0, 1, int.MaxValue)]
            int version,
            [ValueSource("StateObjects")] object state)
        {
            var sut = _sutBuilder.WithVersion(version).WithState(state).Build();

            Assert.That(sut.Version, Is.EqualTo(version));
            Assert.That(sut.State, Is.EqualTo(state));
        }
    }
}
