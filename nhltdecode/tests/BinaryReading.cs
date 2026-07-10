//
// Copyright (c) 2026, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.IO;
using Xunit;

namespace nhltdecode.tests
{
    public class BinaryReadingTests
    {
        static T Read<T>(Func<BinaryReader, T> read, byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
                return read(new BinaryReader(stream));
        }

        [Fact]
        public static void ReadDeviceConfig()
        {
            DeviceConfig empty = Read(BinaryReading.ReadDeviceConfig, Data.DeviceCfg0bBytes);
            DeviceConfig cfg1b = Read(BinaryReading.ReadDeviceConfig, Data.DeviceCfg1bBytes);
            DeviceConfig cfg2b = Read(BinaryReading.ReadDeviceConfig, Data.DeviceCfg2bBytes);
            DeviceConfig cfg = Read(BinaryReading.ReadDeviceConfig, Data.DeviceCfgBytes);

            Assert.Null(empty.VirtualSlot);

            Assert.Equal(Data.DeviceCfg1b.VirtualSlot, cfg1b.VirtualSlot);
            Assert.Equal(Data.DeviceCfg1b.ConfigType, cfg1b.ConfigType);
            Assert.Equal(Data.DeviceCfg1b.ArrayTypeEx, cfg1b.ArrayTypeEx);
            Assert.Equal(Data.DeviceCfg1b.VendorMicsConfig, cfg1b.VendorMicsConfig);

            Assert.Equal(Data.DeviceCfg2b.VirtualSlot, cfg2b.VirtualSlot);
            Assert.Equal(Data.DeviceCfg2b.ConfigType, cfg2b.ConfigType);
            Assert.Equal(Data.DeviceCfg2b.ArrayTypeEx, cfg2b.ArrayTypeEx);
            Assert.Equal(Data.DeviceCfg2b.VendorMicsConfig, cfg2b.VendorMicsConfig);

            Assert.Equal(Data.DeviceCfg.VirtualSlot, cfg.VirtualSlot);
            Assert.Equal(Data.DeviceCfg.ConfigType, cfg.ConfigType);
            Assert.Equal(Data.DeviceCfg.ArrayTypeEx, cfg.ArrayTypeEx);
            Assert.Equal(Data.DeviceCfg.VendorMicsConfig, cfg.VendorMicsConfig);
        }

        [Fact]
        public static void ReadI2SConfig()
        {
            I2SConfig legacy = Read(BinaryReading.ReadI2SConfig, Data.I2SLegacyConfigBytes);
            I2SConfig i2s15 = Read(BinaryReading.ReadI2SConfig, Data.I2S15ConfigBytes);
            I2SConfig i2s2 = Read(BinaryReading.ReadI2SConfig, Data.I2S2ConfigBytes);
            I2SConfig i2s3 = Read(BinaryReading.ReadI2SConfig, Data.I2S3ConfigBytes);
            I2SConfig dma = Read(BinaryReading.ReadI2SConfig, Data.I2SDMAConfigBytes);

            Assert.Equal(Data.I2SLegacy.Version, legacy.Version);
            Assert.Equal(Data.I2SLegacy.GatewayAttributes, legacy.GatewayAttributes);
            Assert.Equal(Data.I2SLegacy.Ssc0, legacy.Ssc0);
            Assert.Equal(Data.I2SLegacy.MdivCtrl, legacy.MdivCtrl);
            Assert.Equal(Data.I2SLegacy.MdivR, legacy.MdivR);

            Assert.Equal(Data.I2S15.Version, i2s15.Version);
            Assert.Equal(Data.I2S15.MdivR, i2s15.MdivR);
            Assert.Equal(Data.I2S2.Version, i2s2.Version);
            Assert.Equal(Data.I2S2.MdivR, i2s2.MdivR);
            Assert.Equal(Data.I2S3.Version, i2s3.Version);
            Assert.Equal(Data.I2S3.MdivR, i2s3.MdivR);
            Assert.Equal(Data.I2SDMA.DmaControls, dma.DmaControls);
        }

        [Fact]
        public static void ReadI2SFormatConfig()
        {
            FormatConfig actual = Read(BinaryReading.ReadI2SFormatConfig, Data.I2SFormatBytes);

            Assert.Equal(Data.I2SFormat.Channels, actual.Channels);
            Assert.NotNull(actual.I2SConfig);
        }

        [Fact]
        public static void ReadI2SFormatsConfig()
        {
            FormatConfig[] actual = Read(BinaryReading.ReadI2SFormatsConfig, Data.I2SFormatsBytes);

            Assert.Single(actual);
        }

        [Fact]
        public static void ReadFirFilter()
        {
            FirFilter actual = Read(BinaryReading.ReadFirFilter, Data.FilterBytes);

            Assert.Equal(Data.Filter.FirControl, actual.FirControl);
            Assert.Equal(Data.Filter.OutGainRight, actual.OutGainRight);
        }

        [Fact]
        public static void ReadPDMCtrlConfig()
        {
            PDMCtrlConfig pdm = Read(BinaryReading.ReadPDMCtrlConfig, Data.PDMBytes);
            PDMCtrlConfig packed = Read(BinaryReading.ReadPDMCtrlConfig, Data.PDMPackedBytes);

            Assert.Equal(Data.PDM.CicControl, pdm.CicControl);
            Assert.Equal(Data.PDM.FirCoeffs, pdm.FirCoeffs);
            Assert.Equal(10, packed.FirCoeffs.Length);
        }

        [Fact]
        public static void ReadPDMCtrlsConfig()
        {
            PDMCtrlConfig[] actual = Read(BinaryReading.ReadPDMCtrlsConfig, Data.PDMMaskedBytes);

            Assert.Single(actual);
            Assert.Equal(Data.PDM.Id, actual[0].Id);
        }

        [Fact]
        public static void ReadChannelConfig()
        {
            ChannelConfig actual = Read(BinaryReading.ReadChannelConfig, Data.ChannelBytes);

            Assert.Equal(Data.Channel.OutControl, actual.OutControl);
        }

        [Fact]
        public static void ReadChannelsConfig()
        {
            ChannelConfig[] actual = Read(BinaryReading.ReadChannelsConfig, Data.ChannelMaskedBytes);

            Assert.Single(actual);
            Assert.Equal(Data.Channel.Id, actual[0].Id);
        }

        [Fact]
        public static void ReadDMICConfig()
        {
            DMICConfig actual = Read(BinaryReading.ReadDMICConfig, Data.DMICConfigBytes);

            Assert.Equal(Data.DMIC.GatewayAttributes, actual.GatewayAttributes);
            Assert.Equal(Data.DMIC.ChannelsConfig, actual.ChannelsConfig);
            Assert.Equal(Data.DMIC.PDMCtrlsConfig, actual.PDMCtrlsConfig);
        }

        [Fact]
        public static void ReadPDMFormatConfig()
        {
            FormatConfig actual = Read(BinaryReading.ReadPDMFormatConfig, Data.DMICFormatBytes);

            Assert.Equal(Data.DMICFormat.Channels, actual.Channels);
            Assert.NotNull(actual.DMICConfig);
        }

        [Fact]
        public static void ReadPDMFormatsConfig()
        {
            FormatConfig[] actual = Read(BinaryReading.ReadPDMFormatsConfig, Data.DMICFormatsBytes);

            Assert.Single(actual);
        }

        [Fact]
        public static void ReadDevicesInfo()
        {
            Native.DeviceInfo[] actual = Read(BinaryReading.ReadDevicesInfo, Data.DevicesBytes);

            Assert.Single(actual);
        }

        [Fact]
        public static void ReadEndpoint()
        {
            Endpoint i2s = Read(BinaryReading.ReadEndpoint, Data.EndpointBytes);
            Endpoint pdm = Read(BinaryReading.ReadEndpoint, Data.EndpointPDMBytes);
            Endpoint hda = Read(BinaryReading.ReadEndpoint, Data.EndpointHDABytes);
            Endpoint dev = Read(BinaryReading.ReadEndpoint, Data.EndpointDevicesBytes);

            Assert.Equal(Data.Endpoint.LinkType, i2s.LinkType);
            Assert.Equal(Data.Endpoint.FormatsConfig.Length, i2s.FormatsConfig.Length);
            Assert.Null(i2s.DevicesInfo);
            Assert.Equal(Native.LINKTYPE.PDM, pdm.LinkType);
            Assert.Null(pdm.DevicesInfo);

            Assert.Equal(Native.LINKTYPE.HDA, hda.LinkType);
            Assert.Null(hda.DevicesInfo);
            Assert.NotNull(hda.FormatsConfig);
            Assert.Equal(Data.EndpointDevices.DevicesInfo, dev.DevicesInfo);
        }

        [Fact]
        public static void ReadEndpoints()
        {
            Endpoint[] actual = Read(BinaryReading.ReadEndpoints, Data.EndpointsBytes);

            Assert.Single(actual);
        }

        [Fact]
        public static void ReadOEDConfig()
        {
            HexBLOB? oed = Read(BinaryReading.ReadOEDConfig, Data.OEDBytes);
            HexBLOB? empty = Read(BinaryReading.ReadOEDConfig, Array.Empty<byte>());

            Assert.Equal(Data.OED, oed.Value);
            Assert.False(empty.HasValue);
        }

        [Fact]
        public static void ReadNHLT()
        {
            NHLT actual = Read(BinaryReading.ReadNHLT, Data.NHLTBytes);

            Assert.Equal(Data.NHLT.Revision, actual.Revision);
            Assert.Equal(Data.NHLT.OemId, actual.OemId);
            Assert.Equal(Data.NHLT.OemTableId, actual.OemTableId);
            Assert.Empty(actual.Endpoints);
            Assert.False(actual.OEDConfig.HasValue);
        }
    }
}
