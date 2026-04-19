//
// Copyright (c) 2026, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;
using probe2wav.Native;

namespace probe2wav.tests
{
    public class ExtensionMethodsTests
    {
        [Fact]
        public void IsWavType()
        {
            Assert.True(0u.IsWavType());
            Assert.True(0xFFFFFFFFu.IsWavType());
            Assert.False(Constants.BASEFW_PROBE_ID.IsWavType());
        }

        [Fact]
        public void Remaining()
        {
            byte[] bytes = new byte[100];
            using var stream = new MemoryStream(bytes);
            using var reader = new BinaryReader(stream);

            Assert.Equal(100, reader.Remaining());
            reader.ReadBytes(30);
            Assert.Equal(70, reader.Remaining());
            reader.ReadBytes(70);
            Assert.Equal(0, reader.Remaining());
        }

        [Fact]
        public void PeekUInt32()
        {
            byte[] bytes = BitConverter.GetBytes(0xDEADBEEFu);
            using var stream = new MemoryStream(bytes);
            using var reader = new BinaryReader(stream);

            long before = reader.BaseStream.Position;
            uint actual = reader.PeekUInt32();
            long after = reader.BaseStream.Position;

            Assert.Equal(before, after);
            Assert.Equal(0xDEADBEEFu, actual);
            // Calling peek twice in a row shall have the same result.
            Assert.Equal(0xDEADBEEFu, reader.PeekUInt32());
        }

        [Fact]
        public void ReadT()
        {
            DataFormat expected = new DataFormat(0xCAFEBABE);
            byte[] bytes = new byte[] { 0xBE, 0xBA, 0xFE, 0xCA };

            using var stream = new MemoryStream(bytes);
            using var reader = new BinaryReader(stream);
            var actual = reader.Read<DataFormat>();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WriteT()
        {
            DataFormat format = new DataFormat(0x12345678);
            byte[] expected = new byte[] { 0x78, 0x56, 0x34, 0x12 };
            byte[] actual = new byte[4];

            using var stream = new MemoryStream(actual);
            using var writer = new BinaryWriter(stream);
            int written = writer.Write(format);

            Assert.Equal(Marshal.SizeOf(format), written);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WriteReadT()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            var expected = new ProbePacketHeader
            {
                SyncWord = Constants.SYNC_WORD,
                ProbeId = 0x00010002,
                DataFormat = new DataFormat(0x103000),
                TimestampHigh = 1,
                TimestampLow = 2,
                DataSize = 64
            };

            writer.Write(expected);
            stream.Position = 0;

            using var reader = new BinaryReader(stream);
            var actual = reader.Read<ProbePacketHeader>();

            Assert.Equal(expected, actual);
        }
    }
}
