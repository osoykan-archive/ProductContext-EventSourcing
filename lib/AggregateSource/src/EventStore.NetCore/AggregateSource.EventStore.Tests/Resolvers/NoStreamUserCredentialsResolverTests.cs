using System;

using AggregateSource.EventStore.NetCore;
using AggregateSource.EventStore.NetCore.Resolvers;

using NUnit.Framework;

namespace AggregateSource.EventStore.Resolvers
{
    [TestFixture]
    public class NoStreamUserCredentialsResolverTests
    {
        [SetUp]
        public void SetUp()
        {
            _sut = new NoStreamUserCredentialsResolver();
        }

        private NoStreamUserCredentialsResolver _sut;

        [Test]
        public void IsStreamUserCredentialsResolver()
        {
            Assert.That(_sut, Is.InstanceOf<IStreamUserCredentialsResolver>());
        }

        [Test]
        public void ResolveIdentifierCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Resolve(null));
        }

        [Test]
        public void ResolveReturnsExpectedResult()
        {
            var identifier = Guid.NewGuid().ToString();

            var result = _sut.Resolve(identifier);

            Assert.That(result, Is.Null);
        }
    }
}
