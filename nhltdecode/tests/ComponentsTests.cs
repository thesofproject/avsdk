//
// Copyright (c) 2026, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System.Runtime.InteropServices;
using Xunit;

namespace nhltdecode.tests
{
    public class I2SConfigComponentTests
    {
        [Fact]
        public void Version()
        {
            var i2s = new I2SConfig { Version = 0x0305 };

            Assert.Equal(0x05, i2s.VersionMinor);
            Assert.Equal(0x03, i2s.VersionMajor);
        }

        [Fact]
        public void SizeOf()
        {
            var invalid = new I2SConfig { Version = 0x1234 };
            var i2s0 = new I2SConfig { Version = 0 };
            var i2s15 = new I2SConfig { Version = I2SConfig.VERSION1_5 };
            var i2s2 = new I2SConfig { Version = I2SConfig.VERSION2_0 };
            var i2s3 = new I2SConfig { Version = I2SConfig.VERSION3_0 };

            Assert.Equal(0, invalid.SizeOf());
            Assert.Equal(Marshal.SizeOf(typeof(Native.I2SConfigLegacy)), i2s0.SizeOf());
            Assert.Equal(Marshal.SizeOf(typeof(Native.I2SConfig15)), i2s15.SizeOf());
            Assert.Equal(Marshal.SizeOf(typeof(Native.I2SConfig2)), i2s2.SizeOf());
            Assert.Equal(Marshal.SizeOf(typeof(Native.I2SConfig3)), i2s3.SizeOf());

            i2s15.MdivR = new HexUInt32[] { 1, 2, 3 };
            i2s3.MdivR = new HexUInt32[] { 1, 2, 3 };

            Assert.Equal(Marshal.SizeOf(typeof(Native.I2SConfig15)) + 3 * sizeof(uint), i2s15.SizeOf());
            Assert.Equal(Marshal.SizeOf(typeof(Native.I2SConfig3)) + 3 * sizeof(uint), i2s3.SizeOf());
        }

        [Fact]
        public void SizeOfBlob()
        {
            var invalid = new I2SConfig { Version = 0x1234 };
            var i2s0 = new I2SConfig { Version = 0 };
            var i2s15 = new I2SConfig { Version = I2SConfig.VERSION1_5 };
            var i2s2 = new I2SConfig { Version = I2SConfig.VERSION2_0 };
            var i2s3 = new I2SConfig { Version = I2SConfig.VERSION3_0 };

            Assert.Equal(0u, invalid.SizeOfBlob());
            Assert.Equal((uint)i2s0.SizeOf() - 4, i2s0.SizeOfBlob());
            Assert.Equal((uint)i2s15.SizeOf() - 4, i2s15.SizeOfBlob());
            Assert.Equal((uint)i2s2.SizeOf() - 4, i2s2.SizeOfBlob());
            Assert.Equal((uint)i2s3.SizeOf() - 4, i2s3.SizeOfBlob());
        }

        [Fact]
        public void ShouldSerializeVersion()
        {
            var i2s = new I2SConfig { Version = I2SConfig.VERSION1_5 };

            Assert.True(i2s.ShouldSerializeVersion());
            Assert.False(new I2SConfig().ShouldSerializeVersion());
        }

        [Fact]
        public void ShouldSerializeDmaControls()
        {
            var i2s = new I2SConfig { DmaControls = new HexBLOB(new byte[1]) };

            Assert.True(i2s.ShouldSerializeDmaControls());
            Assert.False(new I2SConfig().ShouldSerializeDmaControls());
        }
    }

    public class FirFilterComponentTests
    {
        [Fact]
        public void ActiveTapsCount()
        {
            Assert.Equal(1u, new FirFilter { FirConfig = 0x00 }.ActiveTapsCount);
            Assert.Equal(4u, new FirFilter { FirConfig = 0xFF03 }.ActiveTapsCount);
        }
    }

    public class DMICConfigComponentTests
    {
        [Fact]
        public void ChannelCtrlsMask()
        {
            var dmic = new DMICConfig()
            {
                ChannelsConfig = new[]
                {
                    new ChannelConfig { Id = 0 },
                    new ChannelConfig { Id = 3 },
                },
            };

            Assert.Equal(0x03u, dmic.ChannelCtrlsMask);
            Assert.Equal(0u, new DMICConfig().ChannelCtrlsMask);
        }

        [Fact]
        public void PDMCtrlsMask()
        {
            var dmic = new DMICConfig()
            {
                PDMCtrlsConfig = new[]
                {
                    new PDMCtrlConfig { Id = 1 },
                    new PDMCtrlConfig { Id = 2 },
                },
            };

            Assert.Equal(0x03u, dmic.PDMCtrlsMask);
            Assert.Equal(0u, new DMICConfig().PDMCtrlsMask);
        }
    }

    public class DeviceConfigComponentTests
    {
        [Fact]
        public void ArrayTypeEx()
        {
            var dev = new DeviceConfig { ArrayTypeEx = 0xAF };

            Assert.Equal(0xF, dev.ArrayType);
            Assert.Equal(0xA, dev.ArrayExtension);
            Assert.Equal(0, new DeviceConfig().ArrayType);
            Assert.Equal(0, new DeviceConfig().ArrayExtension);
        }

        [Fact]
        public void ShouldSerializeVirtualSlot()
        {
            var dev = new DeviceConfig { VirtualSlot = 1 };

            Assert.True(dev.ShouldSerializeVirtualSlot());
            Assert.False(new DeviceConfig().ShouldSerializeVirtualSlot());
        }

        [Fact]
        public void ShouldSerializeConfigType()
        {
            var dev = new DeviceConfig { ConfigType = 1 };

            Assert.True(dev.ShouldSerializeConfigType());
            Assert.False(new DeviceConfig().ShouldSerializeConfigType());
        }

        [Fact]
        public void ShouldSerializeArrayTypeEx()
        {
            var dev = new DeviceConfig { ArrayTypeEx = 1 };

            Assert.True(dev.ShouldSerializeArrayTypeEx());
            Assert.False(new DeviceConfig().ShouldSerializeArrayTypeEx());
        }
    }

    public class NHLTComponentTests
    {
        [Fact]
        public void ShouldSerializeOEDConfig()
        {
            var nhlt = new NHLT { OEDConfig = new HexBLOB(new byte[1]) };

            Assert.True(nhlt.ShouldSerializeOEDConfig());
            Assert.False(new NHLT().ShouldSerializeOEDConfig());
        }
    }
}
