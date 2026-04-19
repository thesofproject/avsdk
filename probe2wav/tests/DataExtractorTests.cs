//
// Copyright (c) 2026, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;
using probe2wav.Native;

namespace probe2wav.tests
{
    public class DataExtractorTests : IDisposable
    {
        readonly string tempDir;
        readonly string defaultPath;
        readonly DataFormat defaultFormat;
        readonly byte[] defaultData;

        public DataExtractorTests()
        {
            tempDir = Path.Combine(Path.GetTempPath(), "probe2wav_tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            defaultPath = Path.Combine(tempDir, "input.bin");
            defaultFormat = new DataFormat(0x103000); // 2ch, 16bits, 48kHz
            defaultData = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD };
        }

        public void Dispose()
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }

        byte[] BuildProbePacket(uint id, DataFormat format, byte[] data,
                                bool gapped, bool invalidChecksum, bool shiftChecksum)
        {
            byte[] preGap = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            byte[] postGap = new byte[] { 0xEE };
            var hdr = new ProbePacketHeader
            {
                SyncWord = Constants.SYNC_WORD,
                ProbeId = id,
                DataFormat = format,
                TimestampHigh = 0,
                TimestampLow = 0,
                DataSize = (uint)data.Length
            };

            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            ulong checksum = hdr.Checksum();

            if (invalidChecksum)
                checksum = 0x12345678;
            if (shiftChecksum)
                checksum <<= 32;

            if (gapped)
                writer.Write(preGap);
            writer.Write(MarshalHelper.StructureToBytes(hdr));
            writer.Write(data);
            writer.Write(checksum);
            if (gapped)
                writer.Write(postGap);

            return stream.ToArray();
        }

        void WriteProbePacket(uint id, DataFormat format, byte[] data)
        {
            using FileStream stream = File.Open(defaultPath, FileMode.Append, FileAccess.Write);
            byte[] packet = BuildProbePacket(id, format, data, false, false, false);

            stream.Write(packet, 0, packet.Length);
        }

        void WriteProbePacket(uint id, bool gapped, bool invalidChecksum, bool shiftChecksum)
        {
            using FileStream stream = File.Open(defaultPath, FileMode.Append, FileAccess.Write);
            byte[] packet = BuildProbePacket(id, defaultFormat, defaultData,
                                             gapped, invalidChecksum, shiftChecksum);

            stream.Write(packet, 0, packet.Length);
        }

        [Fact]
        public void ProcessFile_CreatesWav()
        {
            string expectedPath = Path.Combine(tempDir, "input_0x00010002.wav");
            byte[] actual, actualData;

            WriteProbePacket(0x00010002, defaultFormat, defaultData);
            DataExtractor.Extract(defaultPath, false, false);

            actual = File.ReadAllBytes(expectedPath);
            Assert.Equal(44 + defaultData.Length, actual.Length);
            actualData = new ArraySegment<byte>(actual, 44, defaultData.Length).ToArray();
            Assert.Equal(defaultData, actualData);
        }

        [Fact]
        public void ProcessFile_CreatesBin()
        {
            string expectedPath = Path.Combine(tempDir, "input_0x01000000.bin");

            WriteProbePacket(Constants.BASEFW_PROBE_ID, defaultFormat, defaultData);
            DataExtractor.Extract(defaultPath, false, false);

            Assert.Equal(defaultData, File.ReadAllBytes(expectedPath));
        }

        [Fact]
        public void ProcessFile_CreatesMixed()
        {
            byte[] data = new byte[64];
            new Random(42).NextBytes(data);

            string expectedPath1 = Path.Combine(tempDir, "input_0x00010002.wav");
            string expectedPath2 = Path.Combine(tempDir, "input_0x01000000.bin");
            byte[] expectedData1 = defaultData.Concat(defaultData).ToArray();
            byte[] expectedData2 = defaultData.Concat(data).ToArray();
            byte[] actual, actualData;

            WriteProbePacket(0x00010002, defaultFormat, defaultData);
            WriteProbePacket(Constants.BASEFW_PROBE_ID, defaultFormat, defaultData);
            WriteProbePacket(Constants.BASEFW_PROBE_ID, defaultFormat, data);
            WriteProbePacket(0x00010002, defaultFormat, defaultData);
            DataExtractor.Extract(defaultPath, false, false);

            actual = File.ReadAllBytes(expectedPath1);
            Assert.Equal(44 + expectedData1.Length, actual.Length);
            actualData = new ArraySegment<byte>(actual, 44, expectedData1.Length).ToArray();
            Assert.Equal(expectedData1, actualData);
            actual = File.ReadAllBytes(expectedPath2);
            Assert.Equal(expectedData2, actual);
        }

        [Fact]
        public void ProcessFile_EmptyInput()
        {
            string[] expectedFiles = new string[] { defaultPath };
            using (FileStream stream = File.OpenWrite(defaultPath))
            {
            }

            DataExtractor.Extract(defaultPath, false, false);

            // Just the input file should be present.
            Assert.Equal(expectedFiles, Directory.GetFiles(tempDir));
        }

        [Fact]
        public void ProcessFile_EmptyData()
        {
            string expectedPath = Path.Combine(tempDir, "input_0x00010002.wav");
            string actualRIFF;
            byte[] actual;

            WriteProbePacket(0x00010002, new DataFormat(uint.MaxValue), new byte[0]);
            DataExtractor.Extract(defaultPath, false, false);

            actual = File.ReadAllBytes(expectedPath);
            Assert.Equal(44, actual.Length);
            actualRIFF = Encoding.ASCII.GetString(actual.Take(4).ToArray());
            Assert.Equal("RIFF", actualRIFF);
        }

        [Fact]
        public void ProcessFile_SkipsGapsNoVerbose()
        {
            string expectedPath = Path.Combine(tempDir, "input_0x01000000.bin");

            WriteProbePacket(Constants.BASEFW_PROBE_ID, gapped: true, false, false);
            DataExtractor.Extract(defaultPath, verbose: false, false);

            Assert.Equal(defaultData, File.ReadAllBytes(expectedPath));
        }

        [Fact]
        public void ProcessFile_ShiftedChecksum()
        {
            string expectedPath = Path.Combine(tempDir, "input_0x01000000.bin");

            WriteProbePacket(Constants.BASEFW_PROBE_ID, false, false, shiftChecksum: true);
            DataExtractor.Extract(defaultPath, true, false);

            Assert.Equal(defaultData, File.ReadAllBytes(expectedPath));
        }

        [Fact]
        public void ProcessFile_InvalidChecksum()
        {
            string expectedPath = Path.Combine(tempDir, "input_0x01000000.bin");

            WriteProbePacket(Constants.BASEFW_PROBE_ID, false, invalidChecksum: true, false);
            DataExtractor.Extract(defaultPath, true, false);

            Assert.False(File.Exists(expectedPath));
        }

        [Fact]
        public void ProcessFile_InvalidChecksumIgnore()
        {
            string expectedPath = Path.Combine(tempDir, "input_0x01000000.bin");

            WriteProbePacket(Constants.BASEFW_PROBE_ID, false, invalidChecksum: true, false);
            DataExtractor.Extract(defaultPath, true, ignoreChecksum: true);

            Assert.True(File.Exists(expectedPath));
        }

        [Fact]
        public void ProcessFile_TruncatedPacket()
        {
            string expectedPath = Path.Combine(tempDir, "input_0x01000000.bin");
            byte[] packet;

            using (FileStream stream = File.OpenWrite(defaultPath))
            {
                packet = BuildProbePacket(Constants.BASEFW_PROBE_ID, defaultFormat, defaultData, false, false, false);
                Array.Resize(ref packet, packet.Length - 1);
                stream.Write(packet, 0, packet.Length);
            }

            DataExtractor.Extract(defaultPath, false, false);

            Assert.False(File.Exists(expectedPath));
        }
    }
}
