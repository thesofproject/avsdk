//
// Copyright (c) 2023, Intel Corporation. All rights reserved.
//
// Authors: Cezary Rojewski <cezary.rojewski@intel.com>
//          Piotr Maziarz <piotrx.maziarz@linux.intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace nhltdecode
{
    public static class BinaryWriting
    {
        public static int WriteConfig(BinaryWriter writer, byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            writer.Write(bytes.Length);
            writer.Write(bytes);

            return Marshal.SizeOf(typeof(Native.Config)) + bytes.Length;
        }

        public static int WriteDeviceConfig(BinaryWriter writer, DeviceConfig device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            var bytes = new List<byte>();
            if (!device.VirtualSlot.HasValue)
                return WriteConfig(writer, bytes.ToArray());

            bytes.Add(device.VirtualSlot.Value);
            if (!device.ConfigType.HasValue)
                return WriteConfig(writer, bytes.ToArray());

            bytes.Add(device.ConfigType.Value);
            if (!device.ArrayTypeEx.HasValue)
                return WriteConfig(writer, bytes.ToArray());

            bytes.Add(device.ArrayTypeEx.Value);
            if (device.VendorMicsConfig == null)
                return WriteConfig(writer, bytes.ToArray());

            bytes.Add((byte)device.VendorMicsConfig.Length);
            foreach (Native.VendorMicConfig mic in device.VendorMicsConfig)
                bytes.AddRange(MarshalHelper.StructureToBytes(mic));

            if (device.MicSnrSensitivity.HasValue)
                bytes.AddRange(MarshalHelper.StructureToBytes(device.MicSnrSensitivity.Value));

            return WriteConfig(writer, bytes.ToArray());
        }

        private static int WriteSSPConfig(BinaryWriter writer, I2SConfig i2s)
        {
            var ssp = new Native.SSPConfig()
            {
                Ssc0 = i2s.Ssc0,
                Ssc1 = i2s.Ssc1,
                Sscto = i2s.Sscto,
                Sspsp = i2s.Sspsp,
                Sstsa = i2s.Sstsa,
                Ssrsa = i2s.Ssrsa,
                Ssc2 = i2s.Ssc2,
                Sspsp2 = i2s.Sspsp2,
                Ssc3 = i2s.Ssc3,
                Ssioc = i2s.Ssioc,
            };

            return writer.Write<Native.SSPConfig>(ssp);
        }

        private static int WriteMclkConfig(BinaryWriter writer, I2SConfig i2s)
        {
            var mclk = new Native.MclkConfig()
            {
                MdivCtrl = i2s.MdivCtrl,
            };

            if (i2s.MdivR?.Length > 0)
                mclk.MdivR = i2s.MdivR[0];

            return writer.Write<Native.MclkConfig>(mclk);
        }

        private static int WriteMclkConfig15(BinaryWriter writer, I2SConfig i2s)
        {
            var mclk = new Native.MclkConfig15()
            {
                MdivCtrl = i2s.MdivCtrl,
            };
            int size;

            if (i2s.MdivR?.Length > 0)
            {
                mclk.MdivRCount = (uint)i2s.MdivR.Length;
                size = writer.Write<Native.MclkConfig15>(mclk);

                int bytesCount = i2s.MdivR.Length * 4;
                byte[] bytes = new byte[bytesCount];

                Buffer.BlockCopy(i2s.MdivR, 0, bytes, 0, bytesCount);
                writer.Write(bytes);
                size += bytesCount;
            }
            else
            {
                size = writer.Write<Native.MclkConfig15>(mclk);
            }

            return size;
        }

        private static int WriteI2SConfigLegacy(BinaryWriter writer, I2SConfig i2s)
        {
            byte[] tsgroup = i2s.TdmTsGroup;

            // Count based on size of Native.I2SConfigLegacy.TdmTsGroup.
            Array.Resize(ref tsgroup, 32);

            writer.Write(i2s.GatewayAttributes);
            writer.Write(tsgroup);
            WriteSSPConfig(writer, i2s);
            WriteMclkConfig(writer, i2s);

            return Marshal.SizeOf(typeof(Native.I2SConfigLegacy));
        }

        private static int WriteI2SConfigHeader(BinaryWriter writer, I2SConfig i2s)
        {
            var hdr = new Native.I2SConfigHeader()
            {
                VersionMinor = i2s.VersionMinor,
                VersionMajor = i2s.VersionMajor,
                Signature = I2SConfig.SIGNATURE,
                SizeBytes = (uint)i2s.SizeOf(),
            };

            return writer.Write<Native.I2SConfigHeader>(hdr);
        }

        private static int WriteI2SConfig15(BinaryWriter writer, I2SConfig i2s)
        {
            int size = Marshal.SizeOf(typeof(Native.I2SConfig15));
            byte[] tsgroup = i2s.TdmTsGroup;

            // Count based on size of Native.I2SConfig15.TdmTsGroup.
            Array.Resize(ref tsgroup, 32);

            writer.Write(i2s.GatewayAttributes);
            WriteI2SConfigHeader(writer, i2s);
            writer.Write(tsgroup);
            WriteSSPConfig(writer, i2s);

            size += WriteMclkConfig15(writer, i2s);
            size -= Marshal.SizeOf(typeof(Native.MclkConfig15));

            return size;
        }

        private static int WriteI2SConfig2(BinaryWriter writer, I2SConfig i2s)
        {
            byte[] tsgroup = i2s.TdmTsGroup;

            // Count based on size of Native.I2SConfig2.TdmTsGroup.
            Array.Resize(ref tsgroup, 256);

            writer.Write(i2s.GatewayAttributes);
            WriteI2SConfigHeader(writer, i2s);
            writer.Write(tsgroup);
            WriteSSPConfig(writer, i2s);
            WriteMclkConfig(writer, i2s);

            return Marshal.SizeOf(typeof(Native.I2SConfig2));
        }

        public static int WriteI2SConfig(BinaryWriter writer, I2SConfig i2s)
        {
            if (i2s == null)
                throw new ArgumentNullException(nameof(i2s));

            switch (i2s.Version)
            {
                case I2SConfig.VERSION2_0:
                    return WriteI2SConfig2(writer, i2s);

                case I2SConfig.VERSION1_5:
                    return WriteI2SConfig15(writer, i2s);

                case 0:
                    return WriteI2SConfigLegacy(writer, i2s);

                default:
                    throw new ArgumentException($"Invalid I2SConfig version: {i2s.Version}");
            }
        }

        public static int WriteFirFilter(BinaryWriter writer, FirFilter filter)
        {
            var native = new Native.FirFilter()
            {
                FirControl = filter.FirControl,
                FirConfig = filter.FirConfig,
                DcOffsetLeft = filter.DcOffsetLeft,
                DcOffsetRight = filter.DcOffsetRight,
                OutGainLeft = filter.OutGainLeft,
                OutGainRight = filter.OutGainRight,
            };

            return writer.Write<Native.FirFilter>(native);
        }

        public static int WritePDMCtrlConfig(BinaryWriter writer, PDMCtrlConfig pdm)
        {
            Native.PDMCtrlConfig hdr = new Native.PDMCtrlConfig()
            {
                CicControl = pdm.CicControl,
                CicConfig = pdm.CicConfig,
                MicControl = pdm.MicControl,
                PdmSdwMap = pdm.PdmSdwMap,
                ReuseFirFromPdm = pdm.ReuseFirFromPdm,
            };
            int size = 0;

            size += writer.Write<Native.PDMCtrlConfig>(hdr);
            size += WriteFirFilter(writer, pdm.FirA);
            size += WriteFirFilter(writer, pdm.FirB);
            writer.Write(pdm.FirCoeffs);
            size += pdm.FirCoeffs.Length;

            return size;
        }

        public static int WritePDMCtrlsConfig(BinaryWriter writer, PDMCtrlConfig[] pdms)
        {
            if (pdms == null)
                throw new ArgumentNullException(nameof(pdms));

            int size = 0;

            foreach (PDMCtrlConfig pdm in pdms)
                size += WritePDMCtrlConfig(writer, pdm);
            return size;
        }

        public static int WriteChannelConfig(BinaryWriter writer, ChannelConfig channel)
        {
            writer.Write(channel.OutControl);
            return Marshal.SizeOf<Native.ChannelConfig>();
        }

        public static int WriteChannelsConfig(BinaryWriter writer, ChannelConfig[] channels)
        {
            if (channels == null)
                throw new ArgumentNullException(nameof(channels));

            int size = 0;

            foreach (ChannelConfig channel in channels)
                size += WriteChannelConfig(writer, channel);
            return size;
        }

        public static int WriteDMICConfig(BinaryWriter writer, DMICConfig dmic)
        {
            if (dmic == null)
                throw new ArgumentNullException(nameof(dmic));

            int size = Marshal.SizeOf<Native.DMICConfig>();
            byte[] tsgroup = dmic.TsGroup;

            // Count based on size of Native.DMICConfig.TsGroup.
            Array.Resize(ref tsgroup, 16);
            writer.Write(dmic.GatewayAttributes);
            writer.Write(tsgroup);
            writer.Write(dmic.GlobalConfig);

            writer.Write(dmic.ChannelCtrlsMask);
            size += WriteChannelsConfig(writer, dmic.ChannelsConfig);
            writer.Write(dmic.PDMCtrlsMask);
            size += WritePDMCtrlsConfig(writer, dmic.PDMCtrlsConfig);

            return size;
        }

        public static int WriteFormatConfig(BinaryWriter writer, FormatConfig format)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            var fmtext = new Native.WaveFormatExtensible()
            {
                FormatTag = FormatConfig.FORMATEXT_TAG,
                Channels = format.Channels,
                SamplesPerSec = format.SamplesPerSec,
                BitsPerSample = format.BitsPerSample,
                Size = FormatConfig.FORMATEXT_SIZE,
                ValidBitsPerSample = format.ValidBitsPerSample,
                ChannelMask = format.ChannelMask,
                Subformat = format.Subformat,
            };
            fmtext.BlockAlign = (ushort)(format.Channels * format.BitsPerSample / 8);
            fmtext.AvgBytesPerSec = fmtext.BlockAlign * format.SamplesPerSec;

            int size = writer.Write<Native.WaveFormatExtensible>(fmtext);
            // Save starting position to write the CapabilitiesSize at once it's known.
            long pos = writer.BaseStream.Position;
            int cfgSize = 0;

            writer.Write(cfgSize);
            if (format.I2SConfig != null)
                cfgSize = WriteI2SConfig(writer, format.I2SConfig);
            else if (format.DMICConfig != null)
                cfgSize = WriteDMICConfig(writer, format.DMICConfig);

            writer.OverwriteAt(pos, cfgSize);
            size += Marshal.SizeOf(typeof(Native.Config));
            size += cfgSize;

            return size;
        }

        public static int WriteFormatsConfig(BinaryWriter writer, FormatConfig[] formats)
        {
            if (formats == null)
                throw new ArgumentNullException(nameof(formats));

            int size = Marshal.SizeOf<Native.FormatsConfig>();

            writer.Write((byte)formats.Length);

            foreach (FormatConfig format in formats)
                size += WriteFormatConfig(writer, format);
            return size;
        }

        public static int WriteDeviceInfo(BinaryWriter writer, Native.DeviceInfo device)
        {
            return writer.Write<Native.DeviceInfo>(device);
        }

        public static int WriteDevicesInfo(BinaryWriter writer, Native.DeviceInfo[] devices)
        {
            if (devices == null)
                throw new ArgumentNullException(nameof(devices));

            int size = Marshal.SizeOf<Native.DevicesInfo>();

            writer.Write((byte)devices.Length);

            foreach (Native.DeviceInfo device in devices)
                size += WriteDeviceInfo(writer, device);
            return size;
        }

        public static int WriteEndpoint(BinaryWriter writer, Endpoint endpoint)
        {
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));

            // Save starting position to write the endpoints's length at once it's known.
            long pos = writer.BaseStream.Position;
            var hdr = new Native.Endpoint()
            {
                LinkType = endpoint.LinkType,
                InstanceId = endpoint.InstanceId,
                VendorId = endpoint.VendorId,
                DeviceId = endpoint.DeviceId,
                RevisionId = endpoint.RevisionId,
                SubsystemId = endpoint.SubsystemId,
                DeviceType = endpoint.DeviceType,
                Direction = endpoint.Direction,
                VirtualBusId = endpoint.VirtualBusId,
            };
            int size = 0;

            size += writer.Write<Native.Endpoint>(hdr);
            size += WriteDeviceConfig(writer, endpoint.DeviceConfig);
            size += WriteFormatsConfig(writer, endpoint.FormatsConfig);
            size += WriteDevicesInfo(writer, endpoint.DevicesInfo);
            writer.OverwriteAt(pos, size);

            return size;
        }

        public static int WriteEndpoints(BinaryWriter writer, Endpoint[] endpoints)
        {
            if (endpoints == null)
                throw new ArgumentNullException(nameof(endpoints));

            int size = sizeof(byte);

            writer.Write((byte)endpoints.Length);

            foreach (Endpoint ep in endpoints)
                size += WriteEndpoint(writer, ep);
            return size;
        }

        public static int WriteOEDConfig(BinaryWriter writer, HexBLOB? oed)
        {
            // This component is optional.
            if (!oed.HasValue)
                return 0;

            return WriteConfig(writer, oed.Value);
        }

        private static byte[] StringToFixedBytes(string value, int size)
        {
            if (value == null)
                return new byte[size];

            byte[] result = Encoding.ASCII.GetBytes(value).Take(size).ToArray();

            Array.Resize(ref result, size);
            return result;
        }

        public static int WriteNHLT(BinaryWriter writer, NHLT table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            // Save starting position to write the table's length at once it's known.
            long pos = writer.BaseStream.Position;
            var hdr = new Native.TableHeader();
            Encoding ascii = Encoding.ASCII;

            // All fixed sizes based on Native.TableHeader layout.
            hdr.Signature = Encoding.ASCII.GetBytes("NHLT");
            hdr.Revision = table.Revision;
            hdr.OemId = StringToFixedBytes(table.OemId, 6);
            hdr.OemTableId = StringToFixedBytes(table.OemTableId, 8);
            hdr.OemRevision = table.OemRevision;
            hdr.AslCompilerId = StringToFixedBytes(table.AslCompilerId, 6);
            hdr.AslCompilerRevision = table.AslCompilerRevision;

            int size = 0;

            size += writer.Write<Native.TableHeader>(hdr);
            size += WriteEndpoints(writer, table.Endpoints);
            size += WriteOEDConfig(writer, table.OEDConfig);

            long offset = (long)Marshal.OffsetOf<Native.TableHeader>("Length");

            writer.OverwriteAt(pos + offset, size);
            offset = (long)Marshal.OffsetOf<Native.TableHeader>("Checksum");
            writer.OverwriteAt(pos + offset, writer.BaseStream.CalculateChecksum());

            return size;
        }
    }
}
