//
// Copyright (c) 2026, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System.Runtime.InteropServices;
using probe2wav.Native;
using Xunit;

namespace probe2wav.tests
{
    public class DataFormatTests
    {
        [Fact]
        public void Raw()
        {
            Assert.Equal(0x700u, new DataFormat(0x700).Raw);
        }

        [Fact]
        public void FormatType()
        {
            Assert.Equal(1u, new DataFormat(0x1).FormatType);
        }

        [Fact]
        public void StandardType()
        {
            Assert.Equal(0xFu, new DataFormat(0xF << 1).StandardType);
        }

        [Fact]
        public void AudioFormat()
        {
            Assert.Equal(0xAu, new DataFormat(0xA << 5).AudioFormat);
        }

        [Fact]
        public void SampleRate0()
        {
            Assert.Equal(8u, new DataFormat(8 << 9).SampleRate0);
        }

        [Fact]
        public void SampleRate()
        {
            Assert.Equal(192000u, new DataFormat(14 << 9).SampleRate);
            Assert.Equal(uint.MaxValue, new DataFormat(15 << 9).SampleRate);
        }

        [Fact]
        public void NumChannels0()
        {
            Assert.Equal(3u, new DataFormat(3 << 13).NumChannels0);
        }

        [Fact]
        public void NumChannels()
        {
            Assert.Equal(4u, new DataFormat(3 << 13).NumChannels);
        }

        [Fact]
        public void SampleSize0()
        {
            Assert.Equal(3u, new DataFormat(3 << 18).SampleSize0);
        }

        [Fact]
        public void ContainerSize0()
        {
            Assert.Equal(1u, new DataFormat(1 << 20).ContainerSize0);
        }

        [Fact]
        public void ContainerBits()
        {
            Assert.Equal(16u, new DataFormat(1 << 20).ContainerBits);
        }

        [Fact]
        public void SampleFormat()
        {
            Assert.Equal(1u, new DataFormat(1 << 22).SampleFormat);
        }

        [Fact]
        public void SampleEnd()
        {
            Assert.Equal(1u, new DataFormat(1 << 23).SampleEnd);
        }

        [Fact]
        public void Interleaving()
        {
            Assert.Equal(1u, new DataFormat(1 << 24).Interleaving);
        }
    }

    public class ProbePacketHeaderTests
    {
        [Fact]
        public void Checksum()
        {
            ulong expected = 256ul + Constants.SYNC_WORD + 0xDEAD + 0xBEEF + 0xAA + 0xBB;
            var hdr = new ProbePacketHeader
            {
                SyncWord = Constants.SYNC_WORD,
                ProbeId = 0xDEAD,
                DataFormat = new DataFormat(0xBEEF),
                TimestampHigh = 0xAA,
                TimestampLow = 0xBB,
                DataSize = 256
            };

            Assert.Equal(expected, hdr.Checksum());
            Assert.Equal(0ul, new ProbePacketHeader().Checksum());
        }
    }
}
