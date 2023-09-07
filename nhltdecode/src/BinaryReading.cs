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
    public static class BinaryReading
    {
        static string NormalizeId(byte[] id)
        {
            var validBytes = new List<byte>();
            string result = string.Empty;

            // Ignore non-printable ASCII characters.
            foreach (byte b in id)
                if (b >= ' ' && b <= '~')
                    validBytes.Add(b);

            return Encoding.ASCII.GetString(validBytes.ToArray());
        }

        public static DeviceConfig Read1bDeviceConfig(BinaryReader reader)
        {
            return new DeviceConfig()
            {
                VirtualSlot = reader.ReadByte(),
            };
        }

        public static DeviceConfig Read2bDeviceConfig(BinaryReader reader)
        {
            return new DeviceConfig()
            {
                VirtualSlot = reader.ReadByte(),
                ConfigType = reader.ReadByte(),
            };
        }

        public static DeviceConfig ReadMicDeviceConfig(BinaryReader reader)
        {
            return new DeviceConfig()
            {
                VirtualSlot = reader.ReadByte(),
                ConfigType = reader.ReadByte(),
                ArrayTypeEx = reader.ReadByte(),
            };
        }

        public static Native.VendorMicConfig[] ReadVendorMicsConfig(BinaryReader reader)
        {
            byte count = reader.ReadByte();
            var result = new Native.VendorMicConfig[count];

            for (byte i = 0; i < count; i++)
                result[i] = reader.Read<Native.VendorMicConfig>();
            return result;
        }

        public static Native.MicSnrSensitivity ReadMicSnrSensitivity(BinaryReader reader)
        {
            return reader.Read<Native.MicSnrSensitivity>();
        }

        public static DeviceConfig ReadDeviceConfig(BinaryReader reader)
        {
            Native.Config cfg = reader.Read<Native.Config>();
            DeviceConfig result;

            switch (cfg.CapabilitiesSize)
            {
                case 0:
                    result = new DeviceConfig();
                    break;

                case 1:
                    result = Read1bDeviceConfig(reader);
                    break;

                case 2:
                    result = Read2bDeviceConfig(reader);
                    break;

                case 3:
                default:
                    result = ReadMicDeviceConfig(reader);
                    if (result.ArrayType == DeviceConfig.MICARRAY_TYPE_VENDOR)
                        result.VendorMicsConfig = ReadVendorMicsConfig(reader);
                    if (result.ArrayExtension == DeviceConfig.MICARRAY_EXT_SNR_SENSITIVITY)
                        result.MicSnrSensitivity = ReadMicSnrSensitivity(reader);
                    break;
            }

            return result;
        }

        private static void InitSSPConfig(BinaryReader reader, ref I2SConfig i2s)
        {
            Native.SSPConfig ssp = reader.Read<Native.SSPConfig>();

            i2s.Ssc0 = ssp.Ssc0;
            i2s.Ssc1 = ssp.Ssc1;
            i2s.Sscto = ssp.Sscto;
            i2s.Sspsp = ssp.Sspsp;
            i2s.Sstsa = ssp.Sstsa;
            i2s.Ssrsa = ssp.Ssrsa;
            i2s.Ssc2 = ssp.Ssc2;
            i2s.Sspsp2 = ssp.Sspsp2;
            i2s.Ssc3 = ssp.Ssc3;
            i2s.Ssioc = ssp.Ssioc;
        }

        private static void InitMclkConfig(BinaryReader reader, ref I2SConfig i2s)
        {
            i2s.MdivCtrl = reader.ReadUInt32();
            i2s.MdivR = new HexUInt32[1];
            i2s.MdivR[0] = reader.ReadUInt32();
        }

        private static void InitMclkConfig15(BinaryReader reader, ref I2SConfig i2s)
        {
            i2s.MdivCtrl = reader.ReadUInt32();
            i2s.MdivR = new HexUInt32[reader.ReadUInt32()];
            for (uint i = 0; i < i2s.MdivR.Length; i++)
                i2s.MdivR[i] = reader.ReadUInt32();
        }

        private static void InitI2SConfigLegacy(BinaryReader reader, ref I2SConfig i2s)
        {
            // Count based on size of Native.I2SConfigLegacy.TdmTsGroup.
            i2s.TdmTsGroup = new HexBLOB(reader.ReadBytes(32));
            InitSSPConfig(reader, ref i2s);
            InitMclkConfig(reader, ref i2s);
        }

        private static void InitI2SConfig15(BinaryReader reader, ref I2SConfig i2s)
        {
            // Count based on size of Native.I2SConfig15.TdmTsGroup.
            i2s.TdmTsGroup = new HexBLOB(reader.ReadBytes(32));
            InitSSPConfig(reader, ref i2s);
            InitMclkConfig15(reader, ref i2s);
        }

        public static I2SConfig ReadI2SConfig(BinaryReader reader)
        {
            Native.Config cfg = reader.Read<Native.Config>();
            long pos = reader.BaseStream.Position;
            var result = new I2SConfig();

            result.GatewayAttributes = reader.ReadUInt32();
            Native.I2SConfigHeader hdr = reader.Peek<Native.I2SConfigHeader>();

            if (hdr.Signature == I2SConfig.SIGNATURE)
            {
                // We have the header already, so just advance the position.
                reader.Read<Native.I2SConfigHeader>();
                result.Version = (ushort)((hdr.VersionMajor << 8) | hdr.VersionMinor);
            }

            switch (result.Version)
            {
                case I2SConfig.VERSION1_5:
                    InitI2SConfig15(reader, ref result);
                    break;

                case 0:
                default:
                    InitI2SConfigLegacy(reader, ref result);
                    break;
            }

            int sizeLeft = (int)(cfg.CapabilitiesSize - (reader.BaseStream.Position - pos));
            if (sizeLeft > 0)
                result.DmaControls = new HexBLOB(reader.ReadBytes(sizeLeft));

            return result;
        }

        public static FormatConfig ReadI2SFormatConfig(BinaryReader reader)
        {
            Native.WaveFormatExtensible fmt = reader.Read<Native.WaveFormatExtensible>();
            var result = new FormatConfig();

            result.Channels = fmt.Channels;
            result.SamplesPerSec = fmt.SamplesPerSec;
            result.BitsPerSample = fmt.BitsPerSample;
            result.ValidBitsPerSample = fmt.ValidBitsPerSample;
            result.ChannelMask = fmt.ChannelMask;
            result.Subformat = fmt.Subformat;
            result.I2SConfig = ReadI2SConfig(reader);

            return result;
        }

        public static FormatConfig[] ReadI2SFormatsConfig(BinaryReader reader)
        {
            byte count = reader.ReadByte();
            var result = new FormatConfig[count];

            for (byte i = 0; i < count; i++)
                result[i] = ReadI2SFormatConfig(reader);
            return result;
        }

        public static FirFilter ReadFirFilter(BinaryReader reader)
        {
            Native.FirFilter filter = reader.Read<Native.FirFilter>();

            return new FirFilter()
            {
                FirControl = filter.FirControl,
                FirConfig = filter.FirConfig,
                DcOffsetLeft = filter.DcOffsetLeft,
                DcOffsetRight = filter.DcOffsetRight,
                OutGainLeft = filter.OutGainLeft,
                OutGainRight = filter.OutGainRight,
            };
        }

        public static PDMCtrlConfig ReadPDMCtrlConfig(BinaryReader reader)
        {
            Native.PDMCtrlConfig cfg = reader.Read<Native.PDMCtrlConfig>();
            var result = new PDMCtrlConfig();

            result.CicControl = cfg.CicControl;
            result.CicConfig = cfg.CicConfig;
            result.MicControl = cfg.MicControl;
            result.PdmSdwMap = cfg.PdmSdwMap;
            result.ReuseFirFromPdm = cfg.ReuseFirFromPdm;
            result.FirA = ReadFirFilter(reader);
            result.FirB = ReadFirFilter(reader);

            uint count = result.FirA.ActiveTapsCount + result.FirB.ActiveTapsCount;

            if (reader.PeekUInt32() == PDMCtrlConfig.FIR_COEFFS_PACKED_TO_24_BITS)
                count = count * 3 + sizeof(uint);
            else
                count *= sizeof(uint);
            result.FirCoeffs = new HexBLOB(reader.ReadBytes((int)count));

            return result;
        }

        public static PDMCtrlConfig[] ReadPDMCtrlsConfig(BinaryReader reader)
        {
            uint mask = reader.ReadUInt32();
            uint count = ExtensionMethods.PopCount(mask);
            var result = new PDMCtrlConfig[count];

            for (int i = 0, j = 0; i < 32 && j < count; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    result[j] = ReadPDMCtrlConfig(reader);
                    result[j].Id = (uint)i;
                    j++;
                }
            }

            return result;
        }

        public static ChannelConfig ReadChannelConfig(BinaryReader reader)
        {
            var result = new ChannelConfig();

            result.OutControl = reader.ReadUInt32();
            return result;
        }

        public static ChannelConfig[] ReadChannelsConfig(BinaryReader reader)
        {
            uint mask = reader.ReadUInt32();
            uint count = ExtensionMethods.PopCount(mask);
            var result = new ChannelConfig[count];

            for (int i = 0, j = 0; i < 32 && j < count; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    result[j] = ReadChannelConfig(reader);
                    result[j].Id = (uint)i;
                    j++;
                }
            }

            return result;
        }

        public static DMICConfig ReadDMICConfig(BinaryReader reader)
        {
            var result = new DMICConfig();

            // Advance over Config.CapabilitiesSize first.
            reader.ReadUInt32();
            result.GatewayAttributes = reader.ReadUInt32();
            // Count based on size of Native.DMICConfig.TsGroup.
            result.TsGroup = reader.ReadBytes(16);
            result.GlobalConfig = reader.ReadUInt32();
            result.ChannelsConfig = ReadChannelsConfig(reader);
            result.PDMCtrlsConfig = ReadPDMCtrlsConfig(reader);

            return result;
        }

        public static FormatConfig ReadPDMFormatConfig(BinaryReader reader)
        {
            Native.WaveFormatExtensible fmt = reader.Read<Native.WaveFormatExtensible>();
            var result = new FormatConfig();

            result.Channels = fmt.Channels;
            result.SamplesPerSec = fmt.SamplesPerSec;
            result.BitsPerSample = fmt.BitsPerSample;
            result.ValidBitsPerSample = fmt.ValidBitsPerSample;
            result.ChannelMask = fmt.ChannelMask;
            result.Subformat = fmt.Subformat;
            result.DMICConfig = ReadDMICConfig(reader);

            return result;
        }

        public static FormatConfig[] ReadPDMFormatsConfig(BinaryReader reader)
        {
            byte count = reader.ReadByte();
            var result = new FormatConfig[count];

            for (byte i = 0; i < count; i++)
                result[i] = ReadPDMFormatConfig(reader);
            return result;
        }

        public static Native.DeviceInfo ReadDeviceInfo(BinaryReader reader)
        {
            var result = new Native.DeviceInfo();

            result.Id = new Guid(reader.ReadBytes(16));
            result.InstanceId = reader.ReadByte();
            result.PortId = reader.ReadByte();

            return result;
        }

        public static Native.DeviceInfo[] ReadDevicesInfo(BinaryReader reader)
        {
            byte count = reader.ReadByte();
            var result = new Native.DeviceInfo[count];

            for (byte i = 0; i < count; i++)
                result[i] = ReadDeviceInfo(reader);
            return result;
        }

        public static Endpoint ReadEndpoint(BinaryReader reader)
        {
            long pos = reader.BaseStream.Position;
            Native.Endpoint ep = reader.Read<Native.Endpoint>();

            var result = new Endpoint()
            {
                LinkType = ep.LinkType,
                InstanceId = ep.InstanceId,
                VendorId = ep.VendorId,
                DeviceId = ep.DeviceId,
                RevisionId = ep.RevisionId,
                SubsystemId = ep.SubsystemId,
                DeviceType = ep.DeviceType,
                Direction = ep.Direction,
                VirtualBusId = ep.VirtualBusId,
            };

            result.DeviceConfig = ReadDeviceConfig(reader);
            switch (result.LinkType)
            {
                case Native.LINKTYPE.SSP:
                    result.FormatsConfig = ReadI2SFormatsConfig(reader);
                    break;

                case Native.LINKTYPE.PDM:
                    result.FormatsConfig = ReadPDMFormatsConfig(reader);
                    break;

                default:
                    break;
            }

            result.DevicesInfo = ReadDevicesInfo(reader);
            // Skip over any remaining artifacts at the end.
            reader.BaseStream.Position = pos + ep.Length;

            return result;
        }

        public static Endpoint[] ReadEndpoints(BinaryReader reader)
        {
            byte count = reader.ReadByte();
            var result = new Endpoint[count];

            for (byte i = 0; i < count; i++)
                result[i] = ReadEndpoint(reader);
            return result;
        }

        public static HexBLOB? ReadOEDConfig(BinaryReader reader)
        {
            // The component is optional.
            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                int count = (int)reader.ReadUInt32();

                return new HexBLOB(reader.ReadBytes(count));
            }

            return null;
        }

        public static NHLT ReadNHLT(BinaryReader reader)
        {
            Native.TableHeader hdr = reader.Read<Native.TableHeader>();
            var result = new NHLT();

            result.Revision = hdr.Revision;
            result.OemId = NormalizeId(hdr.OemId);
            result.OemTableId = NormalizeId(hdr.OemTableId);
            result.OemRevision = hdr.OemRevision;
            result.AslCompilerId = NormalizeId(hdr.AslCompilerId);
            result.AslCompilerRevision = hdr.AslCompilerRevision;
            result.Endpoints = ReadEndpoints(reader);
            result.OEDConfig = ReadOEDConfig(reader);

            return result;
        }
    }
}
