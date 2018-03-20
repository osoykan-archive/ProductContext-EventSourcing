using FluentAssertions;

using Xunit;

namespace ProductContext.Framework.Tests
{
    public class TypeExtensionsTests
    {
        [Fact]
        public void type_name_should_not_contain_assembly_information()
        {
            typeof(TypeExtensions).TypeQualifiedName().Should().Be("ProductContext.Framework.TypeExtensions, ProductContext.Framework");
        }
    }
}
