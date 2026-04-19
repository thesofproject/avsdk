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
    public class DataWriterTests : IDisposable
    {
        readonly string tempDir;
        readonly DataFormat defaultFormat;

        public DataWriterTests()
        {
            tempDir = Path.Combine(Path.GetTempPath(), "probe2wav_tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            defaultFormat = new DataFormat(0x103000); // 2ch, 16bits, 48kHz
        }

        public void Dispose()
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }

        byte[] WriteReadData(string filename, uint id, byte[] data)
        {
            string path = Path.Combine(tempDir, filename);

            using (var writer = new DataWriter(path, id, defaultFormat))
            {
                if (data != null)
                    writer.Write(data, 0, data.Length);
            }

            return File.ReadAllBytes(path);
        }

        [Fact]
        public void WriteBin()
        {
            byte[] expected = new byte[4] { 0xB, 0xA, 0xB, 0xE };
            byte[] actual = WriteReadData("test.bin", Constants.BASEFW_PROBE_ID, expected);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WriteWav()
        {
            byte[] actual = WriteReadData("test.wav", 0x00010002, null);
            var hdr = MarshalHelper.BytesToStructure<WavHeader>(actual);

            Assert.Equal(Marshal.SizeOf(hdr), actual.Length);
            Assert.Equal(2, hdr.NumChannels);
            Assert.Equal(48000u, hdr.SampleRate);
            Assert.Equal(16, hdr.BitsPerSample);
            Assert.Equal(36u, hdr.ChunkSize); // see WriteWavHeader()
            Assert.Equal(0u, hdr.Subchunk2Size);
        }

        [Fact]
        public void WriteAccumulatesData()
        {
            byte[] expected = new byte[4] { 0xB, 0xA, 0xB, 0xE };
            byte[] actual = WriteReadData("test.wav", 0x00010002, expected);
            var hdr = MarshalHelper.BytesToStructure<WavHeader>(actual);

            Assert.Equal(Marshal.SizeOf(hdr) + expected.Length, actual.Length);
            Assert.Equal(36u + expected.Length, hdr.ChunkSize); // see WriteWavHeader()
            Assert.Equal((uint)expected.Length, hdr.Subchunk2Size);
        }

        [Fact]
        public void WriterDispose()
        {
            string path = Path.Combine(tempDir, "test.wav");
            var writer = new DataWriter(path, 0, new DataFormat());

            // The second dispose shall not throw and return early.
            writer.Dispose();
            writer.Dispose();
        }
    }
}
