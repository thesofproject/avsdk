//
// Copyright (c) 2026, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using NUcmSerializer;
using Xunit;

namespace avstplg.tests
{
    public class ExtensionMethodsTests
    {
        [Fact]
        public void TryInt32()
        {
            bool result;
            int actual;

            result = "0x10".TryInt32(out actual);
            Assert.True(result);
            Assert.Equal(0x10, actual);

            result = "12345".TryInt32(out actual);
            Assert.True(result);
            Assert.Equal(12345, actual);

            result = "-42".TryInt32(out actual);
            Assert.True(result);
            Assert.Equal(-42, actual);

            Assert.False("0x0x".TryInt32(out actual));
            Assert.False("notanumber".TryInt32(out actual));
        }

        [Fact]
        public void ToInt32()
        {
            Assert.Equal(0x10, "0x10".ToInt32());
            Assert.Equal(12345, "12345".ToInt32());
            Assert.Equal(-42, "-42".ToInt32());
            Assert.Equal(default(int), "0x0x".ToInt32());
        }

        [Fact]
        public void TryUInt32()
        {
            bool result;
            uint actual;

            result = "0x10".TryUInt32(out actual);
            Assert.True(result);
            Assert.Equal<uint>(0x10, actual);

            result = "012345678".TryUInt32(out actual);
            Assert.True(result);
            Assert.Equal<uint>(012345678, actual);

            result = "0x0x".TryUInt32(out actual);
            Assert.False(result);
        }

        [Fact]
        public void ToUInt32()
        {
            Assert.Equal(0x10u, "0x10".ToUInt32());
            Assert.Equal(012345678u, "012345678".ToUInt32());
            Assert.Equal(default(uint), "0x0x".ToUInt32());
        }

        [Fact]
        public void ToUInts32()
        {
            string str;
            uint[] actuals;

            str = "16, 0x0,2,0x4, 0xa, 2456757";
            actuals = str.ToUInts32();

            Assert.Equal(6, actuals.Length);
            Assert.Equal(16u, actuals[0]);
            Assert.Equal(0x0u, actuals[1]);
            Assert.Equal(2u, actuals[2]);
            Assert.Equal(0x4u, actuals[3]);
            Assert.Equal(0xAu, actuals[4]);
            Assert.Equal(2456757u, actuals[5]);

            str = @"333, aaaa;.[],.;,].;.,`/\/\[[]]5,0x999";
            actuals = str.ToUInts32();

            Assert.Equal(2, actuals.Length);
            Assert.Equal(333u, actuals[0]);
            Assert.Equal(0x999u, actuals[1]);
        }

        [Fact]
        public void ToUInt16()
        {
            Assert.Equal<ushort>(0x10, "0x10".ToUInt16());
            Assert.Equal<ushort>(12345, "12345".ToUInt16());
            Assert.Equal<ushort>(0xFFFF, "0xFFFF".ToUInt16());
            Assert.Equal<ushort>(default(ushort), "0x0x".ToUInt16());
        }

        [Fact]
        public void ToUInt8()
        {
            Assert.Equal<byte>(0x10, "0x10".ToUInt8());
            Assert.Equal<byte>(255, "255".ToUInt8());
            Assert.Equal<byte>(0xFF, "0xFF".ToUInt8());
            Assert.Equal<byte>(default(byte), "0x0x".ToUInt8());
        }

        [Fact]
        public void ToRate()
        {
            Assert.Equal(PCM_RATE._5512, 5512u.ToRate());
            Assert.Equal(PCM_RATE._8000, 8000u.ToRate());
            Assert.Equal(PCM_RATE._11025, 11025u.ToRate());
            Assert.Equal(PCM_RATE._12000, 12000u.ToRate());
            Assert.Equal(PCM_RATE._16000, 16000u.ToRate());
            Assert.Equal(PCM_RATE._22050, 22050u.ToRate());
            Assert.Equal(PCM_RATE._24000, 24000u.ToRate());
            Assert.Equal(PCM_RATE._32000, 32000u.ToRate());
            Assert.Equal(PCM_RATE._44100, 44100u.ToRate());
            Assert.Equal(PCM_RATE._48000, 48000u.ToRate());
            Assert.Equal(PCM_RATE._64000, 64000u.ToRate());
            Assert.Equal(PCM_RATE._88200, 88200u.ToRate());
            Assert.Equal(PCM_RATE._96000, 96000u.ToRate());
            Assert.Equal(PCM_RATE._176400, 176400u.ToRate());
            Assert.Equal(PCM_RATE._192000, 192000u.ToRate());
            Assert.Equal(PCM_RATE.KNOT, 0u.ToRate());
            Assert.Equal(PCM_RATE.KNOT, 37800u.ToRate());

            Assert.Throws<NotSupportedException>(() => 0xFu.ToRate());
        }

        [Fact]
        public void ToFormat()
        {
            Assert.Equal(PCM_FORMAT.S8, 8u.ToFormat());
            Assert.Equal(PCM_FORMAT.S16_LE, 16u.ToFormat());
            Assert.Equal(PCM_FORMAT.S24_LE, 24u.ToFormat());
            Assert.Equal(PCM_FORMAT.S32_LE, 32u.ToFormat());

            Assert.Throws<NotSupportedException>(() => 7u.ToFormat());
            Assert.Throws<NotSupportedException>(() => 21u.ToFormat());
        }
    }
}

