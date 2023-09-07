//
// Copyright (c) 2023, Intel Corporation. All rights reserved.
//
// Authors: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.InteropServices;

namespace nhltdecode
{
    public class I2SConfig
    {
        public const ushort VERSION1_5 = 0x0105;
        public const byte SIGNATURE = 0xEE;

        public HexUInt32 GatewayAttributes;
        public HexUInt16 Version;
        public HexBLOB TdmTsGroup;
        public HexUInt32 Ssc0;
        public HexUInt32 Ssc1;
        public HexUInt32 Sscto;
        public HexUInt32 Sspsp;
        public HexUInt32 Sstsa;
        public HexUInt32 Ssrsa;
        public HexUInt32 Ssc2;
        public HexUInt32 Sspsp2;
        public HexUInt32 Ssc3;
        public HexUInt32 Ssioc;
        public HexUInt32 MdivCtrl;
        [XmlElement]
        public HexUInt32[] MdivR;
        public HexBLOB? DmaControls;

        public byte VersionMinor
        {
            get => (byte)(Version & 0xFF);
        }

        public byte VersionMajor
        {
            get => (byte)((Version >> 8) & 0xFF);
        }

        public int SizeOf()
        {
            int size = DmaControls?.Length ?? 0;

            switch (Version)
            {
                case VERSION1_5:
                    size += Marshal.SizeOf(typeof(Native.I2SConfig15));
                    if (MdivR != null)
                        size += MdivR.Length * sizeof(uint);
                    break;

                case 0:
                    size += Marshal.SizeOf(typeof(Native.I2SConfigLegacy));
                    break;

                default:
                    break;
            }
            return size;
        }

        public bool ShouldSerializeVersion()
        {
            return Version != 0;
        }

        public bool ShouldSerializeDmaControls()
        {
            return DmaControls.HasValue;
        }
    }

    public struct FirFilter
    {
        public HexUInt32 FirControl;
        public HexUInt32 FirConfig;
        public HexUInt32 DcOffsetLeft;
        public HexUInt32 DcOffsetRight;
        public HexUInt32 OutGainLeft;
        public HexUInt32 OutGainRight;

        // Bits 7:0 (FIR_LENGTH) express taps count minus 1.
        public uint ActiveTapsCount
        {
            get => (FirConfig & 0xFF) + 1;
        }
    }

    public struct PDMCtrlConfig
    {
        public const uint FIR_COEFFS_PACKED_TO_24_BITS = uint.MaxValue;

        [XmlAttribute]
        public uint Id;
        public HexUInt32 CicControl;
        public HexUInt32 CicConfig;
        public HexUInt32 MicControl;
        public HexUInt32 PdmSdwMap;
        public HexUInt32 ReuseFirFromPdm;
        public FirFilter FirA;
        public FirFilter FirB;
        public HexBLOB FirCoeffs;
    }

    public struct ChannelConfig
    {
        [XmlAttribute]
        public uint Id;
        public HexUInt32 OutControl;
    }

    public class DMICConfig
    {
        public HexUInt32 GatewayAttributes;
        public HexBLOB TsGroup;
        public HexUInt32 GlobalConfig;
        public ChannelConfig[] ChannelsConfig;
        public PDMCtrlConfig[] PDMCtrlsConfig;

        public uint ChannelCtrlsMask
        {
            get
            {
                uint mask = 0;

                if (ChannelsConfig != null)
                    foreach (ChannelConfig chn in ChannelsConfig)
                        mask |= (1u << (int)chn.Id);
                return mask;
            }
        }

        public uint PDMCtrlsMask
        {
            get
            {
                uint mask = 0;

                if (PDMCtrlsConfig != null)
                    foreach (PDMCtrlConfig pdm in PDMCtrlsConfig)
                        mask |= (1u << (int)pdm.Id);
                return mask;
            }
        }
    }

    public class FormatConfig
    {
        public const ushort FORMATEXT_TAG = 0xFFFE;
        public const ushort FORMATEXT_SIZE = 22;

        public ushort Channels;
        public uint SamplesPerSec;
        public ushort BitsPerSample;
        public ushort ValidBitsPerSample;
        public HexUInt32 ChannelMask;
        public Guid Subformat;

        public I2SConfig I2SConfig;
        public DMICConfig DMICConfig;
    }

    public class DeviceConfig
    {
        public const byte MICARRAY_TYPE_VENDOR = 0xF;
        public const byte MICARRAY_EXT_SNR_SENSITIVITY = 1;

        public byte? VirtualSlot;
        public byte? ConfigType;
        public HexUInt8? ArrayTypeEx;
        [XmlArrayItem("VendorMicConfig")]
        public Native.VendorMicConfig[] VendorMicsConfig;
        public Native.MicSnrSensitivity? MicSnrSensitivity;

        // Bits 3:0 of ArrayTypeEx express array type.
        public int ArrayType
        {
            get => ArrayTypeEx.HasValue ? ArrayTypeEx.Value & 0xF : 0;
        }

        // Bits 7:4 of ArrayTypeEx express array extension.
        public int ArrayExtension
        {
            get => ArrayTypeEx.HasValue ? (ArrayTypeEx.Value >> 4) & 0xF : 0;
        }

        public bool ShouldSerializeVirtualSlot()
        {
            return VirtualSlot.HasValue;
        }

        public bool ShouldSerializeConfigType()
        {
            return ConfigType.HasValue;
        }

        public bool ShouldSerializeArrayTypeEx()
        {
            return ArrayTypeEx.HasValue;
        }

        public bool ShouldSerializeMicSnrSensitivity()
        {
            return MicSnrSensitivity.HasValue;
        }
    }

    public class Endpoint
    {
        public byte LinkType;
        public byte InstanceId;
        public HexUInt16 VendorId;
        public HexUInt16 DeviceId;
        public ushort RevisionId;
        public uint SubsystemId;
        public byte DeviceType;
        public byte Direction;
        public byte VirtualBusId;

        public DeviceConfig DeviceConfig;
        public FormatConfig[] FormatsConfig;
        public Native.DeviceInfo[] DevicesInfo;
    }

    public class NHLT
    {
        public byte Revision;
        public string OemId;
        public string OemTableId;
        public uint OemRevision;
        public string AslCompilerId;
        public uint AslCompilerRevision;
        public Endpoint[] Endpoints;
        public HexBLOB? OEDConfig;

        public bool ShouldSerializeOEDConfig()
        {
            return OEDConfig.HasValue;
        }
    }
}
