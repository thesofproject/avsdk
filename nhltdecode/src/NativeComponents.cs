//
// Copyright (c) 2023, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace nhltdecode.Native
{
    // The base configuration space structure.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Config
    {
        // Does not include the size of itself.
        public uint CapabilitiesSize;
        //
        // Followed by:
        // byte[] Capabilities;
        //
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
    public struct DeviceInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string Id;
        public byte InstanceId;
        public byte PortId;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DevicesInfo
    {
        public byte DevicesCount;
        //
        // Followed by:
        // DeviceInfo[] Devices;
        //
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MclkConfig
    {
         public uint MdivCtrl;
         public uint MdivR;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SSPConfig
    {
        public uint Ssc0;
        public uint Ssc1;
        public uint Sscto;
        public uint Sspsp;
        public uint Sstsa;
        public uint Ssrsa;
        public uint Ssc2;
        public uint Sspsp2;
        public uint Ssc3;
        public uint Ssioc;
    }

    // For ACE 3.0+ onward
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SSPConfig3
    {
        public uint Ssc0;
        public uint Ssc1;
        public uint Sscto;
        public uint Sspsp;
        public uint Ssc2;
        public uint Sspsp2;
        public uint Ssc3;
        public uint Ssioc;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public ulong[] Ssmidytsa;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public ulong[] Ssmodytsa;
    }

    //
    // Depending on configuration of choice, different struct is used:
    //
    // - I2SConfigLegacy for 1 MCLK, up to 8 channels configuration
    // - I2SConfig15 for 2+ MCLK, up to 8 channels configuration
    // - I2SConfig2 for 1 MCLK, up to 16 channels configuration
    // - I2SConfig3 for 2+ MCLK, up to 16 channels configuration (TdmTsGroup ignored at 8+)
    //

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct I2SConfigLegacy
    {
        public uint GatewayAttributes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public uint[] TdmTsGroup;
        public SSPConfig SSPConfig;
        public MclkConfig MclkConfig;
        //
        // Optionally followed by:
        // byte[] DmaControls;
        //
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MclkConfig15
    {
        public uint MdivCtrl;
        public uint MdivRCount;
        //
        // Followed by:
        // public uint[] MdivR;
        //
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct I2SConfigHeader
    {
        public byte VersionMinor;
        public byte VersionMajor;
        public byte Reserved;
        public byte Signature;
        public uint SizeBytes;
    }

    // Header.VersionMajor = 1.5
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct I2SConfig15
    {
        public uint GatewayAttributes;
        public I2SConfigHeader Header;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public uint[] TdmTsGroup;
        public SSPConfig SSPConfig;
        public MclkConfig15 MclkConfig;
        //
        // Optionally followed by:
        // byte[] DmaControls;
        //
    };

    // Header.VersionMajor = 2.0
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct I2SConfig2
    {
        public uint GatewayAttributes;
        public I2SConfigHeader Header;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public uint[] TdmTsGroup;
        public SSPConfig SSPConfig;
        public MclkConfig MclkConfig;
        //
        // Optionally followed by:
        // byte[] DmaControls;
        //
    };

    // Header.VersionMajor/Minor = 3.0
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct I2SConfig3
    {
        public uint GatewayAttributes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public uint[] TdmTsGroup;
        public SSPConfig3 SSPConfig;
        public MclkConfig15 MclkConfig;
        //
        // Optionally followed by:
        // byte[] DmaControls;
        //
    };
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FirFilter
    {
        public uint FirControl;
        public uint FirConfig;
        public uint DcOffsetLeft;
        public uint DcOffsetRight;
        public uint OutGainLeft;
        public uint OutGainRight;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[] Rsvd;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PDMCtrlConfig
    {
        public uint CicControl;
        public uint CicConfig;
        public uint Rsvd0;
        public uint MicControl;
        public uint PdmSdwMap;
        public uint ReuseFirFromPdm;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[] Rsvd1;
        //
        // Followed by:
        // FirFilter  FirA;
        // FirFilter  FirB;
        // byte[]     FirCoeffs;
        //
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ChannelConfig
    {
        public uint OutControl;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DMICConfig
    {
        public uint GatewayAttributes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] TsGroup;
        public uint GlobalConfig;
        public uint ChannelCtrlMask;
        //
        // Followed by:
        // ChannelConfig[] ChannelCtrls;
        //
        public uint PdmCtrlMask;
        //
        // Followed by:
        // PdmCtrlCfg[] PdmCtrls;
        //
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WaveFormatExtensible
    {
        public ushort FormatTag;
        public ushort Channels;
        public uint SamplesPerSec;
        public uint AvgBytesPerSec;
        public ushort BlockAlign;
        public ushort BitsPerSample;
        public ushort Size;
        public ushort ValidBitsPerSample;
        public uint ChannelMask;
        public Guid Subformat;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FormatConfig
    {
        public WaveFormatExtensible Format;
        public Config Config;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FormatsConfig
    {
        public byte FormatsCount;
        //
        // Followed by:
        // FormatConfig[] Formats;
        //
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VendorMicConfig
    {
        public byte Type;
        public byte Panel;
        public ushort SpeakerPositionDistance;
        public ushort HorizontalOffset;
        public ushort VerticalOffset;
        public byte FrequencyLowBand;
        public byte FrequencyHighBand;
        public short DirectionAngle;
        public short ElevationAngle;
        public short WorkVerticalAngleBegin;
        public short WorkVerticalAngleEnd;
        public short WorkHorizontalAngleBegin;
        public short WorkHorizontalAngleEnd;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VendorMicDeviceConfig
    {
        public uint CapabilitiesSize;
        public byte VirtualSlot;
        public byte ConfigType;
        public byte ArrayTypeEx;
        public byte MicsCount;
        //
        // Followed by:
        // VendorMicConfig[] Mics;
        //
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MicDeviceConfig
    {
        public uint CapabilitiesSize;
        public byte VirtualSlot;
        public byte ConfigType;
        public byte ArrayTypeEx;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DeviceConfig
    {
        public uint CapabilitiesSize;
        public byte VirtualSlot;
        public byte ConfigType;
    }

    public static class DIRECTION
    {
        public const ushort RENDER = 0;
        public const ushort CAPTURE = 1;
    }

    public static class DEVICETYPE
    {
        // Unique to an endpoint of LINKTYPE=PDM.
        public const byte PDM = 0;
        public const byte PDM_SKL = 1;

        // Unique to an endpoint of LINKTYPE=SSP.
        public const byte BT = 0;
        public const byte FM = 1;
        public const byte MODEM = 2;
        public const byte CODEC = 4;
    }

    public static class DEVICEID
    {
        public const ushort DMIC  = 0xAE20;
        public const ushort BT    = 0xAE30;
        public const ushort I2S   = 0xAE34;
    }

    public static class LINKTYPE
    {
        public const byte HDA = 0;
        public const byte DSP = 1;
        public const byte PDM = 2;
        public const byte SSP = 3;
        public const byte SLIMBUS = 4;
        public const byte SDW = 5;
        public const byte UAOL = 6;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Endpoint
    {
        public uint Length;
        public byte LinkType;
        public byte InstanceId;
        public ushort VendorId;
        public ushort DeviceId;
        public ushort RevisionId;
        public uint SubsystemId;
        public byte DeviceType;
        public byte Direction;
        public byte VirtualBusId;
        //
        // Followed by:
        // Config         DeviceConfig;
        // FormatsConfig  FormatsConfig;
        //
        // Optionally followed by:
        // DevicesInfo    DevicesInfo;
        //
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TableHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Signature;
        public uint Length;
        public byte Revision;
        public byte Checksum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] OemId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] OemTableId;
        public uint OemRevision;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] AslCompilerId;
        public uint AslCompilerRevision;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NHLT
    {
        public TableHeader Header;
        public byte EndpointsCount;
        //
        // Followed by:
        // Endpoint[]  Endpoints;
        //
        // Optionally followed by:
        // Config      OEDConfig;
        //
    }
}
