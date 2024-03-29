﻿//
// Copyright (c) 2020-2022, Intel Corporation. All rights reserved.
//
// Authors: Piotr Maziarz <piotrx.maziarz@linux.intel.com>
//          Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace nhltdecode
{
    public enum CONF_TYPE : byte
    {
        GENERIC = 0,
        MIC_ARRAY = 1,
        RENDER_WITH_LOOPBACK = 2, // not supported in Windows
        RENDER_FEEDBACK = 3, // in case of endpoint capture direction means feedback for render
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DeviceSpecificConfig
    {
        public byte VirtualSlot;
        public CONF_TYPE ConfigType;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MicArrayDeviceSpecificConfig
    {
        public DeviceSpecificConfig DeviceConfig;
        public byte ArrayTypeEx;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MicSnrSensitivityExtension
    {
        public int SNR;
        public int Sensitivity;
    }

    [StructLayout(LayoutKind.Sequential)]
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
    public struct RenderFeedbackConfig
    {
        public byte FeedbackVirtualSlot;
        public ushort FeedbackChannels;
        public ushort FeedbackValidBitsPerSample;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct I2sConfigurationBlobLegacy
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public uint[] TdmTsGroup;
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
        public uint Mdivc;
        public uint Mdivr;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct I2sConfigurationBlob : IBinaryConvertible<I2sConfigurationBlob>
    {
        public byte Sig;
        public byte Rsvd0;
        public byte VerMajor;
        public byte VerMinor;
        public uint SizeBytes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public uint[] TdmTsGroup;
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
        public uint Mdivc;
        public uint MdivrCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)] // fake size
        public uint[] Mdivr;

        public static I2sConfigurationBlob ReadFromBinary(BinaryReader reader)
        {
            I2sConfigurationBlob blob, sizer = new I2sConfigurationBlob();

            blob = MarshalHelper.FromBinaryReader<I2sConfigurationBlob>(reader, sizer.SizeOf());

            blob.Mdivr = new uint[blob.MdivrCount];
            for (int i = 0; i < blob.MdivrCount; i++)
                blob.Mdivr[i] = reader.ReadUInt32();

            return blob;
        }

        public int SizeOf()
        {
            int uintSize = Marshal.SizeOf(typeof(uint));
            int size = Marshal.SizeOf(this);

            size -= uintSize; // fake size
            if (Mdivr != null)
                size += uintSize * Mdivr.Length;
            return size;
        }

        public void WriteToBinary(BinaryWriter writer)
        {
            byte[] bytes = MarshalHelper.StructureToBytes(this, SizeOf());
            writer.Write(bytes);
            foreach (var div in Mdivr)
                writer.Write(div);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FirCfg
    {
        public uint FirControl;
        public uint FirConfig;
        public uint DcOffsetLeft;
        public uint DcOffsetRight;
        public uint OutGainLeft;
        public uint OutGainRight;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[] Rsvd;

        internal int GetChannelCount()
        {
            return (int)(FirConfig & 0xFF) + 1;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PdmCtrlCfg : IBinaryConvertible<PdmCtrlCfg>
    {
        public uint CicControl;
        public uint CicConfig;
        public uint Rsvd0;
        public uint MicControl;
        public uint Pdmsm;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public uint[] Rsvd1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public FirCfg[] FirConfig;
        [XmlElement(DataType = "hexBinary")]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)] // fake size
        public byte[] FirCoeffs;

        public static PdmCtrlCfg ReadFromBinary(BinaryReader reader)
        {
            PdmCtrlCfg cfg, sizer = new PdmCtrlCfg();

            cfg = MarshalHelper.FromBinaryReader<PdmCtrlCfg>(reader, sizer.SizeOf());

            long pos = reader.BaseStream.Position;
            bool packed = (reader.ReadUInt32() == 0xFFFFFFFF);
            reader.BaseStream.Position = pos; // mimics 'peek'

            int count = 0;
            foreach (var fir in cfg.FirConfig)
                count += fir.GetChannelCount();

            if (!packed)
                count *= Marshal.SizeOf(typeof(uint));
            else
                count = count * 3 + Marshal.SizeOf(typeof(uint)); // the initial packed dword
            cfg.FirCoeffs = reader.ReadBytes(count);

            return cfg;
        }

        public int SizeOf()
        {
            int byteSize = Marshal.SizeOf(typeof(byte));
            int size = Marshal.SizeOf(this);

            size -= byteSize; // fake size
            if (FirCoeffs != null)
                size += byteSize * FirCoeffs.Length;
            return size;
        }

        public void WriteToBinary(BinaryWriter writer)
        {
            writer.Write(CicControl);
            writer.Write(CicConfig);
            writer.Write(Rsvd0);
            writer.Write(MicControl);
            writer.Write(Pdmsm);

            foreach (var rsvd in Rsvd1)
                writer.Write(rsvd);
            foreach (var fir in FirConfig)
                writer.Write(MarshalHelper.StructureToBytes(fir));

            if (FirCoeffs != null)
                writer.Write(FirCoeffs);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DmicConfigurationBlob : IBinaryConvertible<DmicConfigurationBlob>
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] TsGroup;
        public uint GlobalCfgClockOnDelay;
        public uint ChannelCtrlMask;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)] // fake size
        public uint[] ChannelCfg;
        public uint PdmCtrlMask;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)] // fake size
        public PdmCtrlCfg[] PdmCtrls;

        public static DmicConfigurationBlob ReadFromBinary(BinaryReader reader)
        {
            var blob = new DmicConfigurationBlob();

            blob.TsGroup = new uint[4];
            for (int i = 0; i < 4; i++)
                blob.TsGroup[i] = reader.ReadUInt32();
            blob.GlobalCfgClockOnDelay = reader.ReadUInt32();

            blob.ChannelCtrlMask = reader.ReadUInt32();
            uint count = ExtensionMethods.PopCount(blob.ChannelCtrlMask);
            blob.ChannelCfg = new uint[count];
            for (int i = 0; i < count; i++)
                blob.ChannelCfg[i] = reader.ReadUInt32();

            blob.PdmCtrlMask = reader.ReadUInt32();
            count = ExtensionMethods.PopCount(blob.PdmCtrlMask);
            blob.PdmCtrls = new PdmCtrlCfg[count];
            for (int i = 0; i < count; i++)
                blob.PdmCtrls[i] = PdmCtrlCfg.ReadFromBinary(reader);

            return blob;
        }

        public int SizeOf()
        {
            int uintSize = Marshal.SizeOf(typeof(uint));
            int size = Marshal.SizeOf(this);

            size -= uintSize; // fake size
            if (ChannelCfg != null)
                size += uintSize * ChannelCfg.Length;

            size -= Marshal.SizeOf(typeof(PdmCtrlCfg)); // fake size
            if (PdmCtrls != null)
                foreach (var ctrl in PdmCtrls)
                    size += ctrl.SizeOf();

            return size;
        }

        public void WriteToBinary(BinaryWriter writer)
        {
            foreach (var grp in TsGroup)
                writer.Write(grp);
            writer.Write(GlobalCfgClockOnDelay);

            writer.Write(ChannelCtrlMask);
            foreach (var cfg in ChannelCfg)
                writer.Write(cfg);

            writer.Write(PdmCtrlMask);
            foreach (var ctrl in PdmCtrls)
                ctrl.WriteToBinary(writer);
        }
    }
}
