using NUcmSerializer;
using Xunit;

namespace NUcmSerializerTests
{
    public class UcmNamedTagAttributesTests
    {
        [Fact]
        public void TestPropertyName()
        {
            Assert.Equal(string.Empty, new UcmElementAttribute("").Name);
        }
    }

    public class UcmElementAttributeTests
    {
        [Fact]
        public void TestConstructor()
        {
            new UcmElementAttribute(string.Empty);
            new UcmElementAttribute(null);
            new UcmElementAttribute();
        }
    }

    public class UcmArrayAttributeTests
    {
        UcmArrayAttribute attribute;

        public UcmArrayAttributeTests()
        {
            attribute = new UcmArrayAttribute();
        }

        [Fact]
        public void TestConstructor()
        {
            new UcmArrayAttribute(string.Empty, true);
            new UcmArrayAttribute(null, false);
            new UcmArrayAttribute(true);
            new UcmArrayAttribute(null);
            new UcmArrayAttribute();
        }

        [Fact]
        public void TestPropertyInline()
        {
            bool flag = true;

            attribute.Inline = flag;
            Assert.Equal(flag, attribute.Inline);
        }

        [Fact]
        public void TestPropertyTagElements()
        {
            bool flag = true;

            attribute.TagElements = flag;
            Assert.Equal(flag, attribute.TagElements);
        }
    }

    public class UcmSectionAttributeTests
    {
        UcmSectionAttribute attribute;

        public UcmSectionAttributeTests()
        {
            attribute = new UcmSectionAttribute();
        }

        [Fact]
        public void TestConstructor()
        {
            new UcmSectionAttribute(string.Empty, "id");
            new UcmSectionAttribute(null, null);
            new UcmSectionAttribute(null);
            new UcmSectionAttribute();
        }

        [Fact]
        public void TestPropertyIdentifier()
        {
            string s = "module0";

            attribute.Identifier = s;
            Assert.Equal(s, attribute.Identifier);
            attribute.Identifier = null;
            Assert.Null(attribute.Identifier);
        }
    }

    public class UcmExclusiveAttributeTests
    {
        UcmExclusiveAttribute attribute;

        public UcmExclusiveAttributeTests()
        {
            attribute = new UcmExclusiveAttribute();
        }

        [Fact]
        public void TestConstructor()
        {
            new UcmExclusiveAttribute(string.Empty);
            new UcmExclusiveAttribute(null);
            new UcmExclusiveAttribute();
        }

        [Fact]
        public void TestPropertyNamespace()
        {
            string s = "internal";

            attribute.Namespace = s;
            Assert.Equal(s, attribute.Namespace);
            attribute.Namespace = null;
            Assert.Null(attribute.Namespace);
        }
    }

    public class UcmEnumAttributeTests
    {
        UcmEnumAttribute attribute;

        public UcmEnumAttributeTests()
        {
            attribute = new UcmEnumAttribute();
        }

        [Fact]
        public void TestConstructor()
        {
            new UcmEnumAttribute(string.Empty);
            new UcmEnumAttribute(null);
            new UcmEnumAttribute();
        }

        [Fact]
        public void TestPropertyName()
        {
            string s = "constants";

            attribute.Name = s;
            Assert.Equal(s, attribute.Name);
            attribute.Name = null;
            Assert.Null(attribute.Name);
        }
    }
}
