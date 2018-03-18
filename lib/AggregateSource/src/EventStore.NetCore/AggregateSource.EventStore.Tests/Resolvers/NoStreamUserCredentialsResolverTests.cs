using System;

using AggregateSource.EventStore.Resolvers;

using NUnit.Framework;

namespace AggregateSource.EventStore.Tests.Resolvers
{
    [TestFixture]
    public class NoStreamUserCredentialsResolverTests
    {
        private NoStreamUserCredentialsResolver _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new NoStreamUserCredentialsResolver();
        }

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
