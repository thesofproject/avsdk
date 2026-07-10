//
// Copyright (c) 2026, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.IO;
using System.Xml.Serialization;
using Xunit;

namespace nhltdecode.tests
{
    public static class TestXml
    {
        public static string Serialize<T>(IXmlSerializable obj)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);
                return writer.ToString();
            }
        }

        public static T Deserialize<T>(string xml)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var reader = new StringReader(xml))
                return (T)serializer.Deserialize(reader);
        }

        public static T SerializeDeserialize<T>(IXmlSerializable obj)
        {
            string xml = Serialize<T>(obj);

            return Deserialize<T>(xml);
        }
    }

    public class HexUInt8Tests
    {
        [Fact]
        public void CastFromU8()
        {
            HexUInt8 actual = (byte)0xAB;

            Assert.Equal(new HexUInt8(0xAB), actual);
        }

        [Fact]
        public void CastToU8()
        {
            byte actual = new HexUInt8(0xAB);

            Assert.Equal<byte>(0xAB, actual);
        }

        [Fact]
        public void GetSchema()
        {
            IXmlSerializable h = new HexUInt8(0);

            Assert.Null(h.GetSchema());
        }

        [Fact]
        public void WriteReadXml()
        {
            HexUInt8 expected = new HexUInt8(0xAB);
            HexUInt8 actual = TestXml.SerializeDeserialize<HexUInt8>(expected);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestToString()
        {
            Assert.Equal("0x0A", new HexUInt8(0x0A).ToString());
            Assert.NotEqual("0xff", new HexUInt8(0xFF).ToString());
        }
    }

    public class HexUInt16Tests
    {
        [Fact]
        public void CastFromU16()
        {
            HexUInt16 actual = (ushort)0xABCD;

            Assert.Equal(new HexUInt16(0xABCD), actual);
        }

        [Fact]
        public void CastToU16()
        {
            ushort actual = new HexUInt16(0xABCD);

            Assert.Equal<ushort>(0xABCD, actual);
        }

        [Fact]
        public void GetSchema()
        {
            IXmlSerializable h = new HexUInt16(0);

            Assert.Null(h.GetSchema());
        }

        [Fact]
        public void WriteReadXml()
        {
            HexUInt16 expected = new HexUInt16(0xABCD);
            HexUInt16 actual = TestXml.SerializeDeserialize<HexUInt16>(expected);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestToString()
        {
            Assert.Equal("0x0A0B", new HexUInt16(0x0A0B).ToString());
            Assert.NotEqual("0xffff", new HexUInt16(0xFFFF).ToString());
        }
    }

    public class HexUInt32Tests
    {
        [Fact]
        public void CastFromU32()
        {
            HexUInt32 actual = (uint)0xABCDEF01;

            Assert.Equal(new HexUInt32(0xABCDEF01), actual);
        }

        [Fact]
        public void CastToU32()
        {
            uint actual = new HexUInt32(0xABCDEF01);

            Assert.Equal<uint>(0xABCDEF01, actual);
        }

        [Fact]
        public void GetSchema()
        {
            IXmlSerializable h = new HexUInt32(0);

            Assert.Null(h.GetSchema());
        }

        [Fact]
        public void WriteReadXml()
        {
            HexUInt32 expected = new HexUInt32(0xDEADBEEF);
            HexUInt32 actual = TestXml.SerializeDeserialize<HexUInt32>(expected);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestToString()
        {
            Assert.Equal("0x0A0B0C0D", new HexUInt32(0x0A0B0C0D).ToString());
            Assert.NotEqual("0xffffffff", new HexUInt32(0xFFFFFFFF).ToString());
        }
    }

    public class HexUInt64Tests
    {
        [Fact]
        public void CastFromU64()
        {
            HexUInt64 actual = (ulong)0xABCDEF01CAFEBABE;

            Assert.Equal(new HexUInt64(0xABCDEF01CAFEBABE), actual);
        }

        [Fact]
        public void CastToU64()
        {
            ulong actual = new HexUInt64(0xABCDEF01CAFEBABE);

            Assert.Equal<ulong>(0xABCDEF01CAFEBABE, actual);
        }

        [Fact]
        public void GetSchema()
        {
            IXmlSerializable h = new HexUInt64(0);

            Assert.Null(h.GetSchema());
        }

        [Fact]
        public void WriteReadXml()
        {
            HexUInt64 expected = new HexUInt64(0xDEADBEEFCAFEBABE);
            HexUInt64 actual = TestXml.SerializeDeserialize<HexUInt64>(expected);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestToString()
        {
            Assert.Equal("0x00000000A0B0C0D0", new HexUInt64(0x0A0B0C0D0).ToString());
            Assert.NotEqual("0x0000000fffffffff", new HexUInt64(0xFFFFFFFFF).ToString());
        }
    }

    public class HexBLOBTests
    {
        [Fact]
        public void ParseNybble_LowerCase()
        {
            //
            // Thanks to HexTable, BytesToHexString() always emits upper case,
            // so lower case branch in ParseNybble() remains unused.  Technically,
            // that branch could be removed but for portability, leave as is.
            //
            HexBLOB expected = new HexBLOB(new byte[] { 0x0D, 0x0C, 0x0B, 0x0A });
            HexBLOB actual = TestXml.Deserialize<HexBLOB>("<HexBLOB>0a0b0c0d</HexBLOB>");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ParseNybble_Throws()
        {
            // The ArgumentException will be an InnerException of InvalidOperationException.
            Assert.Throws<InvalidOperationException>(() => TestXml.Deserialize<HexBLOB>("<HexBLOB>0g</HexBLOB>"));
        }

        [Fact]
        public void HexStringToBytes_Throws()
        {
            // The ArgumentException will be an InnerException of InvalidOperationException.
            Assert.Throws<InvalidOperationException>(() => TestXml.Deserialize<HexBLOB>("<HexBLOB>0a0</HexBLOB>"));
        }

        [Fact]
        public void CastFromByteArray()
        {
            byte[] bytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            HexBLOB expected = new HexBLOB(bytes);
            HexBLOB actual = bytes;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CastToByteArray()
        {
            byte[] expected = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            HexBLOB blob = new HexBLOB(expected);
            byte[] actual = blob;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Length()
        {
            var blob = new HexBLOB(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 });

            Assert.Equal(5, blob.Length);
            Assert.Equal(0, default(HexBLOB).Length);
            Assert.Equal(0, new HexBLOB(new byte[0]).Length);
        }

        [Fact]
        public void GetSchema()
        {
            IXmlSerializable s = new HexBLOB(new byte[] { });

            Assert.Null(s.GetSchema());
        }

        [Fact]
        public void WriteXml_Null()
        {
            string expected = "<HexBLOB />";
            string actual = TestXml.Serialize<HexBLOB>(default(HexBLOB));

            Assert.Contains(expected, actual);
        }

        [Fact]
        public void WriteReadXml_SingleByte()
        {
            HexBLOB expected = new HexBLOB(new byte[] { 0xAB });
            HexBLOB actual = TestXml.SerializeDeserialize<HexBLOB>(expected);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WriteReadXml_MultiRow()
        {
            byte[] bytes = new byte[40];

            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = (byte)(i * 7 + 3);

            HexBLOB expected = new HexBLOB(bytes);
            HexBLOB actual = TestXml.SerializeDeserialize<HexBLOB>(expected);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void HexBLOB_Equals()
        {
            var first = new HexBLOB(new byte[] { 0 });
            var second = default(HexBLOB);

            Assert.True(first.Equals(first));
            Assert.True(second.Equals(second));
            Assert.False(first.Equals(second));
            Assert.False(second.Equals(first));
        }

        [Fact]
        public void Object_Equals()
        {
            var blob = new HexBLOB(new byte[] { 0 });

            Assert.True(blob.Equals((object)blob));
            Assert.False(blob.Equals((object)null));
        }

        [Fact]
        public void Object_GetHashCode()
        {
            var blob = new HexBLOB(new byte[] { 4, 5, 6 });

            Assert.Equal(blob.GetHashCode(), blob.GetHashCode());
            Assert.Equal(default(HexBLOB).GetHashCode(), default(HexBLOB).GetHashCode());
        }
    }
}
