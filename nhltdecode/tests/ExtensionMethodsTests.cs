//
// Copyright (c) 2026, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System.IO;
using Xunit;

namespace nhltdecode.tests
{
    public class ExtensionMethodsTests
    {
        [Theory]
        [InlineData(0u, 0u)]
        [InlineData(1u, 1u)]
        [InlineData(1u, 2u)]
        [InlineData(2u, 3u)]
        public void PopCount2(uint expected, uint input)
        {
            Assert.Equal(expected, ExtensionMethods.PopCount2(input));
        }

        [Fact]
        public void TryUInt64()
        {
            ulong actual;

            Assert.True("0xDEADBEEF12".TryUInt64(out actual));
            Assert.Equal(0xDEADBEEF12ul, actual);
            Assert.True("12345".TryUInt64(out actual));
            Assert.Equal(12345ul, actual);

            Assert.False("0xZZ".TryUInt64(out actual));
            Assert.False("notanumber".TryUInt64(out actual));
        }

        [Fact]
        public void ToUInt64()
        {
            Assert.Equal(0xDEADBEEF12ul, "0xDEADBEEF12".ToUInt64());
            Assert.Equal(12345ul, "12345".ToUInt64());
            Assert.Equal(default, "0xZZ".ToUInt64());
        }

        [Fact]
        public void TryUInt32()
        {
            uint actual;

            Assert.True("0x10".TryUInt32(out actual));
            Assert.Equal(0x10u, actual);
            Assert.True("12345".TryUInt32(out actual));
            Assert.Equal(12345u, actual);

            Assert.False("0xZZ".TryUInt32(out actual));
            Assert.False("notanumber".TryUInt32(out actual));
        }

        [Fact]
        public void ToUInt32()
        {
            Assert.Equal(0x10u, "0x10".ToUInt32());
            Assert.Equal(12345u, "12345".ToUInt32());
            Assert.Equal(default, "0xZZ".ToUInt32());
        }

        [Fact]
        public void ToUInt16()
        {
            Assert.Equal<ushort>(0xABCD, "0xABCD".ToUInt16());
        }

        [Fact]
        public void ToUInt8()
        {
            Assert.Equal<byte>(0xAB, "0xAB".ToUInt8());
        }

        [Fact]
        public void PeekByte()
        {
            byte[] bytes = new byte[] { 0x42, 0x43 };

            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                Assert.Equal(0x42, reader.PeekByte());
                Assert.Equal(0, reader.BaseStream.Position);
                Assert.Equal(0x42, reader.ReadByte());
            }
        }

        [Fact]
        public void PeekUInt32()
        {
            byte[] bytes = new byte[] { 0x78, 0x56, 0x34, 0x12 };

            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                Assert.Equal(0x12345678u, reader.PeekUInt32());
                Assert.Equal(0, reader.BaseStream.Position);
                Assert.Equal(0x12345678u, reader.PeekUInt32());
            }
        }

        [Fact]
        public void PeekT()
        {
            byte[] bytes = new byte[] { 0x04, 0x00, 0x00, 0x00 };

            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                Native.Config cfg = reader.Peek<Native.Config>();

                Assert.Equal(4u, reader.Peek<Native.Config>().CapabilitiesSize);
                Assert.Equal(0, reader.BaseStream.Position);
                cfg = reader.Peek<Native.Config>();
                Assert.Equal(4u, reader.Peek<Native.Config>().CapabilitiesSize);
            }
        }

        [Fact]
        public void Read()
        {
            var expected = new Native.Config() { CapabilitiesSize = 0xCAFE };
            byte[] bytes = new byte[4] { 0xFE, 0xCA, 0x00, 0x00 };

            using (var stream = new MemoryStream(bytes))
            {
                var reader = new BinaryReader(stream);
                Native.Config actual = reader.Read<Native.Config>();

                Assert.Equal(expected.CapabilitiesSize, actual.CapabilitiesSize);
            }
        }

        [Fact]
        public void OverwriteAtByte()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);

                writer.Write(new byte[] { 0x11, 0x22 });
                writer.OverwriteAt(0, (byte)0x99);

                Assert.Equal(2, stream.Position);
                Assert.Equal(new byte[] { 0x99, 0x22 }, stream.ToArray());
            }
        }

        [Fact]
        public void OverwriteAtInt()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);

                writer.Write(new byte[] { 0x11, 0x11, 0x11, 0x11, 0x22, 0x22, 0x22, 0x22 });
                writer.OverwriteAt(0, 0x33333333);

                Assert.Equal(8, stream.Position);
                Assert.Equal(new byte[] { 0x33, 0x33, 0x33, 0x33, 0x22, 0x22, 0x22, 0x22 }, stream.ToArray());
            }
        }

        [Fact]
        public void Write()
        {
            var cfg = new Native.Config() { CapabilitiesSize = 0xCAFE };
            byte[] expected = new byte[4] { 0xFE, 0xCA, 0x00, 0x00 };

            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                int size = writer.Write(cfg);

                Assert.Equal(4, size);
                Assert.Equal(expected, stream.ToArray());
            }
        }

        [Fact]
        public void CalculateChecksum()
        {
            byte[] bytes = new byte[] { 0x01, 0x02, 0x03 };

            using (var stream = new MemoryStream(bytes))
            {
                stream.Position = 2;
                Assert.Equal((byte)(256 - 6), stream.CalculateChecksum());
                Assert.Equal(2, stream.Position);
            }
        }

        [Fact]
        public void CalculateChecksum_LargeBuffer()
        {
            byte[] bytes = new byte[2048]; // For testing the do-while loop.

            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = 0x01;

            using (var stream = new MemoryStream(bytes))
                // 2048 bytes of 0x01 sum to a byte value of 0, so 256 - 0 wraps to 0.
                Assert.Equal((byte)0, stream.CalculateChecksum());
        }
    }
}
