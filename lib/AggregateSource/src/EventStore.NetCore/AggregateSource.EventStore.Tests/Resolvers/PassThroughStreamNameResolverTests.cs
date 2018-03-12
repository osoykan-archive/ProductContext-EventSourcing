using System;

using AggregateSource.EventStore.NetCore;
using AggregateSource.EventStore.NetCore.Resolvers;

using NUnit.Framework;

namespace AggregateSource.EventStore.Resolvers
{
    [TestFixture]
    public class PassThroughStreamNameResolverTests
    {
        [SetUp]
        public void SetUp()
        {
            _sut = new PassThroughStreamNameResolver();
        }

        private PassThroughStreamNameResolver _sut;

        [Test]
        public void IsStreamNameResolver()
        {
            Assert.That(_sut, Is.InstanceOf<IStreamNameResolver>());
        }

        [Test]
        public void ResolveIdentifierCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Resolve(null));
        }

        [Test]
        public void ResolveReturnsExpectedResult()
        {
            const string identifier = "Id/DE3094E3-B9E8-4AD0-98FF-4945C4ED4823";
            var result = _sut.Resolve(identifier);
            Assert.That(result, Is.EqualTo(identifier));
        }
    }
}
