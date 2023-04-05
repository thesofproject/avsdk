using System;
using avstplg;
using NUcmSerializer;
using Xunit;

namespace avstplg.tests
{
    public class ExtensionMethodsTests
    {
        [Fact]
        public void TestTryUInt32()
        {
            string str;
            bool result;
            uint val;

            str = "0x10";
            result = str.TryUInt32(out val);

            Assert.True(result);
            Assert.Equal<uint>(0x10, val);
            Assert.NotEqual<uint>(10, val);

            str = "012345678";
            result = str.TryUInt32(out val);

            Assert.True(result);
            Assert.Equal<uint>(012345678, val);
            Assert.NotEqual<uint>(0x012345678, val);

            str = "0x0x";
            result = str.TryUInt32(out val);

            Assert.False(result);
        }

        [Fact]
        public void TestToUInt32()
        {
            string str;
            uint val;

            str = "0x10";
            val = str.ToUInt32();

            Assert.Equal<uint>(0x10, val);
            Assert.NotEqual<uint>(10, val);

            str = "012345678";
            val = str.ToUInt32();

            Assert.Equal<uint>(012345678, val);
            Assert.NotEqual<uint>(0x012345678, val);

            str = "0x0x";
            val = str.ToUInt32();
            Assert.Equal<uint>(default(uint), val);
        }

        [Fact]
        public void TestToUInts32()
        {
            string str;
            uint[] vals;

            str = "16, 0x0,2,0x4, 0xa, 2456757";
            vals = str.ToUInts32();

            Assert.Equal<int>(6, vals.Length);
            Assert.Equal<uint>(16, vals[0]);
            Assert.Equal<uint>(0x0, vals[1]);
            Assert.Equal<uint>(2, vals[2]);
            Assert.Equal<uint>(0x4, vals[3]);
            Assert.Equal<uint>(0xa, vals[4]);
            Assert.Equal<uint>(2456757, vals[5]);

            str = @"333, aaaa;.[],.;,].;.,`/\/\[[]]5,0x999";
            vals = str.ToUInts32();

            Assert.Equal<int>(2, vals.Length);
            Assert.Equal<uint>(333, vals[0]);
            Assert.Equal<uint>(0x999, vals[1]);
        }

        [Fact]
        void TestToRate()
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
        void TestToFormat()
        {
            Assert.Equal(PCM_FORMAT.S16_LE, 16u.ToFormat());
            Assert.Equal(PCM_FORMAT.S24_LE, 24u.ToFormat());
            Assert.Equal(PCM_FORMAT.S32_LE, 32u.ToFormat());

            Assert.Throws<NotSupportedException>(() => 7u.ToFormat());
            Assert.Throws<NotSupportedException>(() => 21u.ToFormat());
        }

    }
}

