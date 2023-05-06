using System;
using System.Collections.Generic;
using NUcmSerializer;
using Xunit;

namespace NUcmSerializerTests
{
    public class TypeHelperTests
    {
        [Fact]
        public void TestIsSimpleType()
        {
            Assert.True(TypeHelper.IsSimpleType(typeof(uint)));
            Assert.True(TypeHelper.IsSimpleType(typeof(string)));
            Assert.True(TypeHelper.IsSimpleType(typeof(PCM_RATE)));
            Assert.True(TypeHelper.IsSimpleType(typeof(Dictionary<,>.Enumerator)));
            Assert.False(TypeHelper.IsSimpleType(typeof(Section)));
        }

        [Fact]
        public void TestIsSimpleArrayType()
        {
            Assert.True(TypeHelper.IsSimpleArrayType(typeof(uint[])));
            Assert.True(TypeHelper.IsSimpleArrayType(typeof(string[])));
            Assert.True(TypeHelper.IsSimpleArrayType(typeof(PCM_RATE[])));
            Assert.True(TypeHelper.IsSimpleArrayType(typeof(Dictionary<int, int>.Enumerator[])));
            Assert.False(TypeHelper.IsSimpleArrayType(typeof(Section[])));

            Assert.False(TypeHelper.IsSimpleArrayType(typeof(uint)));
            Assert.False(TypeHelper.IsSimpleArrayType(typeof(string)));
            Assert.False(TypeHelper.IsSimpleArrayType(typeof(PCM_RATE)));
            Assert.False(TypeHelper.IsSimpleArrayType(typeof(Dictionary<,>.Enumerator)));
            Assert.False(TypeHelper.IsSimpleArrayType(typeof(Section)));
        }

        [Fact]
        public void TestIsSimpleTupleType()
        {
            Assert.True(TypeHelper.IsSimpleTupleType(typeof(Tuple<,>)));
            Assert.True(TypeHelper.IsSimpleTupleType(typeof(Tuple<uint, string>)));
            Assert.True(TypeHelper.IsSimpleTupleType(typeof(Tuple<bool, DAPM_EVENT>)));

            Assert.False(TypeHelper.IsSimpleTupleType(typeof(Tuple<>)));
            Assert.False(TypeHelper.IsSimpleTupleType(typeof(Tuple<,,>)));
            Assert.False(TypeHelper.IsSimpleTupleType(typeof(Tuple<ushort, Section>)));
            Assert.False(TypeHelper.IsSimpleTupleType(typeof(VendorTuples<>)));
            Assert.False(TypeHelper.IsSimpleTupleType(typeof(string)));
            Assert.False(TypeHelper.IsSimpleTupleType(typeof(Section)));
        }

        [Fact]
        public void TestIsVendorArrayType()
        {
            Assert.False(TypeHelper.IsVendorArrayType(typeof(uint[])));
            Assert.False(TypeHelper.IsVendorArrayType(typeof(string[])));
            Assert.False(TypeHelper.IsVendorArrayType(typeof(PCM_RATE[])));
            Assert.False(TypeHelper.IsVendorArrayType(typeof(Dictionary<int, int>.Enumerator[])));
            Assert.True(TypeHelper.IsVendorArrayType(typeof(Section[])));

            Assert.False(TypeHelper.IsVendorArrayType(typeof(uint)));
            Assert.False(TypeHelper.IsVendorArrayType(typeof(string)));
            Assert.False(TypeHelper.IsVendorArrayType(typeof(PCM_RATE)));
            Assert.False(TypeHelper.IsVendorArrayType(typeof(Dictionary<,>.Enumerator)));
            Assert.False(TypeHelper.IsVendorArrayType(typeof(Section)));
        }

        [Fact]
        public void TestGetTypeTokenType()
        {
            Assert.Equal(TokenType.Element, TypeHelper.GetTypeTokenType(typeof(string)));
            Assert.Equal(TokenType.Element, TypeHelper.GetTypeTokenType(typeof(Guid)));
            Assert.Equal(TokenType.Element, TypeHelper.GetTypeTokenType(typeof(bool)));
            Assert.Equal(TokenType.Element, TypeHelper.GetTypeTokenType(typeof(byte)));
            Assert.Equal(TokenType.Element, TypeHelper.GetTypeTokenType(typeof(ushort)));
            Assert.Equal(TokenType.Element, TypeHelper.GetTypeTokenType(typeof(uint)));

            Assert.Equal(TokenType.Array, TypeHelper.GetTypeTokenType(typeof(string[])));
            Assert.Equal(TokenType.Array, TypeHelper.GetTypeTokenType(typeof(Guid[])));
            Assert.Equal(TokenType.Array, TypeHelper.GetTypeTokenType(typeof(bool[])));
            Assert.Equal(TokenType.Array, TypeHelper.GetTypeTokenType(typeof(byte[])));
            Assert.Equal(TokenType.Array, TypeHelper.GetTypeTokenType(typeof(ushort[])));
            Assert.Equal(TokenType.Array, TypeHelper.GetTypeTokenType(typeof(uint[])));

            Assert.Equal(TokenType.Tuple, TypeHelper.GetTypeTokenType(typeof(Tuple<,>)));
            Assert.Equal(TokenType.Tuple, TypeHelper.GetTypeTokenType(typeof(Tuple<uint, string>)));
            Assert.Equal(TokenType.Tuple, TypeHelper.GetTypeTokenType(typeof(Tuple<bool, DAPM_EVENT>)));

            Assert.Equal(TokenType.VendorArray, TypeHelper.GetTypeTokenType(typeof(Section[])));
            Assert.Equal(TokenType.Section, TypeHelper.GetTypeTokenType(typeof(Section)));
            Assert.Equal(TokenType.Section, TypeHelper.GetTypeTokenType(typeof(VendorTuples<>)));
            Assert.Equal(TokenType.Section, TypeHelper.GetTypeTokenType(typeof(SectionManifest)));

            Assert.Equal(TokenType.None, TypeHelper.GetTypeTokenType(typeof(List<>)));
            Assert.Equal(TokenType.None, TypeHelper.GetTypeTokenType(typeof(TypeHelper)));
        }

        [Fact]
        public void TestGetObjectGenericPropertyValue()
        {
            var tuple = new Tuple<int, string>(0x51, "value");

            Assert.Equal(0x51, TypeHelper.GetObjectGenericPropertyValue(tuple, 0));
            Assert.Equal("value", TypeHelper.GetObjectGenericPropertyValue(tuple, 1));
            Assert.NotEqual(0x51, TypeHelper.GetObjectGenericPropertyValue(tuple, 1));
            Assert.NotEqual("value", TypeHelper.GetObjectGenericPropertyValue(tuple, 0));

            Assert.Null(TypeHelper.GetObjectGenericPropertyValue(tuple, 999));
            Assert.Null(TypeHelper.GetObjectGenericPropertyValue(string.Empty, 6));
            Assert.Null(TypeHelper.GetObjectGenericPropertyValue(new SectionCC(), 0));
        }

        [Fact]
        public void TestGetObjectGenericPropertyValueT()
        {
            var tuple = new Tuple<int, string>(0x51, "value");

            Assert.Equal(0x51, TypeHelper.GetObjectGenericPropertyValue<int>(tuple, 0));
            Assert.Equal("value", TypeHelper.GetObjectGenericPropertyValue<string>(tuple, 1));
            Assert.Throws<InvalidCastException>(() => TypeHelper.GetObjectGenericPropertyValue<int>(tuple, 1));
            Assert.Throws<InvalidCastException>(() => TypeHelper.GetObjectGenericPropertyValue<string>(tuple, 0));

            Assert.Null(TypeHelper.GetObjectGenericPropertyValue<object>(tuple, 999));
            Assert.Null(TypeHelper.GetObjectGenericPropertyValue<string>(string.Empty, 6));
            Assert.Null(TypeHelper.GetObjectGenericPropertyValue<Section>(new SectionCC(), 0));
        }

        [Fact]
        public void TestConvertFromString()
        {
            string s = "ba9712c4-1ecc-4943-992e-70b51a402170";
            Guid guid = new Guid(s);

            s = "0xc4, 0x12, 0x97, 0xba, 0xcc, 0x1e, 0x43, 0x49, 0x99, 0x2e, 0x70, 0xb5, 0x1a, 0x40, 0x21, 0x70";

            Assert.Equal(guid, TypeHelper.ConvertFromString(typeof(Guid), s));
            Assert.Equal(PCM_FORMAT.S24_LE, TypeHelper.ConvertFromString(typeof(PCM_FORMAT), "S24_LE"));
            Assert.Equal(TPLG_MCLK.CI, TypeHelper.ConvertFromString(typeof(TPLG_MCLK), "codec_mclk_in"));
            Assert.Equal(0xDEADBEEFL, TypeHelper.ConvertFromString(typeof(long), "0xDEADBEEF"));
            Assert.Null(TypeHelper.ConvertFromString(typeof(Section), "string"));
            Assert.Null(TypeHelper.ConvertFromString(typeof(Array), "new[]"));
            Assert.Null(TypeHelper.ConvertFromString(typeof(object), null));
        }
    }

    public class ExtensionMethodsTests
    {
        [Fact]
        public void TestToBytes()
        {
            string s = "ba9712c4-1ecc-4943-992e-70b51a402170";
            Guid guid = new Guid(s);

            s = "0xc4, 0x12, 0x97, 0xba, 0xcc, 0x1e, 0x43, 0x49, 0x99, 0x2e, 0x70, 0xb5, 0x1a, 0x40, 0x21, 0x70";
            Assert.Equal(guid.ToByteArray(), ExtensionMethods.ToBytes(s));
            s = "196, 18, 151, 186, 204, 30, 67, 73, 153, 46, 112, 181, 26, 64, 33, 112";
            Assert.Equal(guid.ToByteArray(), ExtensionMethods.ToBytes(s));
            s = "196, 0x12, 151, 0xba, 204, 0x1e, 67, 0x49, 153, 0x2e, 112, 0xb5, 26, 0x40, 33, 0x70";
            Assert.Equal(guid.ToByteArray(), ExtensionMethods.ToBytes(s));
        }

        [Fact]
        public void TestToUInts16()
        {
            ushort[] arr = { 196, 18, 151, 186, 204, 30, 67, 73, 153, 46, 112, 181, 26, 64, 33, 112 };
            string s;

            s = "0xc4, 0x12, 0x97, 0xba, 0xcc, 0x1e, 0x43, 0x49, 0x99, 0x2e, 0x70, 0xb5, 0x1a, 0x40, 0x21, 0x70";
            Assert.Equal(arr, ExtensionMethods.ToUInts16(s));
            s = "196, 18, 151, 186, 204, 30, 67, 73, 153, 46, 112, 181, 26, 64, 33, 112";
            Assert.Equal(arr, ExtensionMethods.ToUInts16(s));
            s = "196, 0x12, 151, 0xba, 204, 0x1e, 67, 0x49, 153, 0x2e, 112, 0xb5, 26, 0x40, 33, 0x70";
            Assert.Equal(arr, ExtensionMethods.ToUInts16(s));
        }

        [Fact]
        public void TestToUInts32()
        {
            uint[] arr = { 196, 18, 151, 186, 204, 30, 67, 73, 153, 46, 112, 181, 26, 64, 33, 112 };
            string s;

            s = "0xc4, 0x12, 0x97, 0xba, 0xcc, 0x1e, 0x43, 0x49, 0x99, 0x2e, 0x70, 0xb5, 0x1a, 0x40, 0x21, 0x70";
            Assert.Equal(arr, ExtensionMethods.ToUInts32(s));
            s = "196, 18, 151, 186, 204, 30, 67, 73, 153, 46, 112, 181, 26, 64, 33, 112";
            Assert.Equal(arr, ExtensionMethods.ToUInts32(s));
            s = "196, 0x12, 151, 0xba, 204, 0x1e, 67, 0x49, 153, 0x2e, 112, 0xb5, 26, 0x40, 33, 0x70";
            Assert.Equal(arr, ExtensionMethods.ToUInts32(s));
        }

        [Fact]
        public void TestGetAttributeOfTypeT()
        {
            Assert.Null(ExtensionMethods.GetAttributeOfType<UcmNamedTagAttribute>(PCM_RATE.KNOT));
            Assert.Null(ExtensionMethods.GetAttributeOfType<UcmSectionAttribute>(CTL_ELEM_ACCESS.OWNER));
            Assert.NotNull(ExtensionMethods.GetAttributeOfType<UcmEnumAttribute>(CTL_ELEM_ACCESS.OWNER));

            // Null when no name for Enum value exists
            Assert.Null(ExtensionMethods.GetAttributeOfType<UcmExclusiveAttribute>((TPLG_BCLK)999));
        }
    }
}
