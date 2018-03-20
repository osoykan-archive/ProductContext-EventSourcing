using System;

using Xunit;

namespace ProductContext.Framework.Tests
{
    public class TypedStreamNameResolverTests
    {
        [Fact]
        public void should_return_type_with_composing_type()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            Type type = typeof(TypedStreamNameResolver);
            string identifier = Guid.NewGuid().ToString();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var resolver = new TypedStreamNameResolver(type, (type1, id) => $"{type1.Name}-{id}");

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            Assert.Equal(resolver.Resolve(identifier), $"{type.Name}-{identifier}");
        }
    }
}
