//
// Copyright (c) 2026, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.IO;
using nhltdecode.Native;
using Xunit;

namespace nhltdecode.tests
{
    public class BinaryWritingTests
    {
        static void AssertWrite<T>(Func<BinaryWriter, T, int> write, T obj, byte[] expected)
        {
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                int size = write(writer, obj);

                Assert.Equal(expected.Length, size);
                Assert.Equal(expected, stream.ToArray());
            }
        }

        [Fact]
        public void WriteConfig()
        {
            AssertWrite<byte[]>(BinaryWriting.WriteConfig, Data.Config, Data.ConfigBytes);
            Assert.Throws<ArgumentNullException>(() => BinaryWriting.WriteConfig(null, null));
        }

        [Fact]
        public void WriteDeviceConfig()
        {
            AssertWrite<DeviceConfig>(BinaryWriting.WriteDeviceConfig, Data.DeviceCfg, Data.DeviceCfgBytes);
            AssertWrite<DeviceConfig>(BinaryWriting.WriteDeviceConfig, Data.DeviceCfg1b, Data.DeviceCfg1bBytes);
            AssertWrite<DeviceConfig>(BinaryWriting.WriteDeviceConfig, Data.DeviceCfg2b, Data.DeviceCfg2bBytes);
            AssertWrite<DeviceConfig>(BinaryWriting.WriteDeviceConfig, Data.DeviceCfg3b, Data.DeviceCfg3bBytes);
            Assert.Throws<ArgumentNullException>(() => BinaryWriting.WriteDeviceConfig(null, null));
        }

        [Fact]
        public void WriteI2SConfig()
        {
            I2SConfig invalid = new I2SConfig() { Version = 0xFF };

            AssertWrite<I2SConfig>(BinaryWriting.WriteI2SConfig, Data.I2SLegacy, Data.I2SLegacyBytes);
            AssertWrite<I2SConfig>(BinaryWriting.WriteI2SConfig, Data.I2S15, Data.I2S15Bytes);
            AssertWrite<I2SConfig>(BinaryWriting.WriteI2SConfig, Data.I2S2, Data.I2S2Bytes);
            AssertWrite<I2SConfig>(BinaryWriting.WriteI2SConfig, Data.I2S3, Data.I2S3Bytes);
            AssertWrite<I2SConfig>(BinaryWriting.WriteI2SConfig, Data.I2SMclk, Data.I2SMclkBytes);
            AssertWrite<I2SConfig>(BinaryWriting.WriteI2SConfig, Data.I2SMclk15, Data.I2SMclk15Bytes);
            AssertWrite<I2SConfig>(BinaryWriting.WriteI2SConfig, Data.I2SDMA, Data.I2SDMABytes);
            Assert.Throws<ArgumentNullException>(() => BinaryWriting.WriteI2SConfig(null, null));
            Assert.Throws<ArgumentException>(() => BinaryWriting.WriteI2SConfig(null, invalid));
        }

        [Fact]
        public void WriteFirFilter()
        {
            AssertWrite<FirFilter>(BinaryWriting.WriteFirFilter, Data.Filter, Data.FilterBytes);
        }

        [Fact]
        public void WritePDMCtrlConfig()
        {
            AssertWrite<PDMCtrlConfig>(BinaryWriting.WritePDMCtrlConfig, Data.PDM, Data.PDMBytes);
        }

        [Fact]
        public void WritePDMCtrlsConfig()
        {
            PDMCtrlConfig[] pdms = new PDMCtrlConfig[] { Data.PDM };

            AssertWrite<PDMCtrlConfig[]>(BinaryWriting.WritePDMCtrlsConfig, pdms, Data.PDMBytes);
            Assert.Throws<ArgumentNullException>(() => BinaryWriting.WritePDMCtrlsConfig(null, null));
        }

        [Fact]
        public void WriteChannelConfig()
        {
            AssertWrite<ChannelConfig>(BinaryWriting.WriteChannelConfig, Data.Channel, Data.ChannelBytes);
        }

        [Fact]
        public void WriteChannelsConfig()
        {
            ChannelConfig[] channels = new ChannelConfig[] { Data.Channel };

            AssertWrite<ChannelConfig[]>(BinaryWriting.WriteChannelsConfig, channels, Data.ChannelBytes);
            Assert.Throws<ArgumentNullException>(() => BinaryWriting.WriteChannelsConfig(null, null));
        }

        [Fact]
        public void WriteDMICConfig()
        {
            AssertWrite<DMICConfig>(BinaryWriting.WriteDMICConfig, Data.DMIC, Data.DMICBytes);
            Assert.Throws<ArgumentNullException>(() => BinaryWriting.WriteDMICConfig(null, null));
        }

        [Fact]
        public void WriteFormatConfig()
        {
            AssertWrite<FormatConfig>(BinaryWriting.WriteFormatConfig, Data.Format, Data.FormatBytes);
            Assert.Throws<ArgumentNullException>(() => BinaryWriting.WriteFormatConfig(null, null));
            AssertWrite<FormatConfig>(BinaryWriting.WriteFormatConfig, Data.I2SFormat, Data.I2SFormatBytes);
            AssertWrite<FormatConfig>(BinaryWriting.WriteFormatConfig, Data.DMICFormat, Data.DMICFormatBytes);
        }

        [Fact]
        public void WriteFormatsConfig()
        {
            AssertWrite<FormatConfig[]>(BinaryWriting.WriteFormatsConfig, Data.Formats, Data.FormatsBytes);
            Assert.Throws<ArgumentNullException>(() => BinaryWriting.WriteFormatsConfig(null, null));
        }

        [Fact]
        public void WriteDevicesInfo()
        {
            AssertWrite<DeviceInfo[]>(BinaryWriting.WriteDevicesInfo, Data.Devices, Data.DevicesBytes);
            AssertWrite<DeviceInfo[]>(BinaryWriting.WriteDevicesInfo, null, Array.Empty<byte>());
        }

        [Fact]
        public void WriteEndpoint()
        {
            AssertWrite<Endpoint>(BinaryWriting.WriteEndpoint, Data.Endpoint, Data.EndpointBytes);
            Assert.Throws<ArgumentNullException>(() => BinaryWriting.WriteEndpoint(null, null));
        }

        [Fact]
        public void WriteEndpoints()
        {
            AssertWrite<Endpoint[]>(BinaryWriting.WriteEndpoints, Data.Endpoints, Data.EndpointsBytes);
            Assert.Throws<ArgumentNullException>(() => BinaryWriting.WriteEndpoints(null, null));
        }

        [Fact]
        public void WriteOEDConfig()
        {
            AssertWrite<HexBLOB?>(BinaryWriting.WriteOEDConfig, Data.OED, Data.OEDBytes);
            AssertWrite<HexBLOB?>(BinaryWriting.WriteOEDConfig, null, Array.Empty<byte>());
        }

        [Fact]
        public void WriteNHLT()
        {
            AssertWrite<NHLT>(BinaryWriting.WriteNHLT, Data.NHLT, Data.NHLTBytes);
            Assert.Throws<ArgumentNullException>(() => BinaryWriting.WriteNHLT(null, null));
        }
    }
}
