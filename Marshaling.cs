using System;
using System.Runtime.InteropServices;

namespace itt
{
    [StructLayout(LayoutKind.Explicit)]
    public struct DmactrlCtrl
    {
        [FieldOffset(0)] public uint Type;
        [FieldOffset(4)] public uint Size;
        [FieldOffset(8)] public SclkfsCfg Sclkfs;
        [FieldOffset(8)] public MclkCfg Mclk;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SclkfsCfg
    {
        public uint SamplingFrequency;
        public uint BitDepth;
        public uint ChannelMap;
        public uint ChannelConfig;
        public uint InterleavingStyle;
        public byte NumberOfChannels;
        public byte ValidBitDepth;
        public byte SampleType;
        public byte Reserved;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct MclkCfg
    {
        public ushort ClkWarmUp;
        public ushort MclkWarmUpOver;
        public ushort ClkStopDelay;
        public ushort KeepRunningClkStopOver;

        public byte Mclk
        {
            get
            {
                return (byte)(MclkWarmUpOver & 0x1);
            }
            set
            {
                MclkWarmUpOver |= (ushort)(value & 0x1);
            }
        }
        public byte WarmUpOver
        {
            get
            {
                return (byte)(MclkWarmUpOver & 0x2);
            }
            set
            {
                MclkWarmUpOver |= (ushort)(value & 0x2);
            }
        }
        public byte KeepRunning
        {
            get
            {
                return (byte)(KeepRunningClkStopOver & 0x1);
            }
            set
            {
                KeepRunningClkStopOver |= (ushort)(value & 0x1);
            }
        }
        public byte ClkStopOver
        {
            get
            {
                return (byte)(KeepRunningClkStopOver & 0x2);
            }
            set
            {
                KeepRunningClkStopOver |= (ushort)(value & 0x2);
            }
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct DfwAlgoData
    {
        [FieldOffset(0)] public uint Config;
        [FieldOffset(4)] public uint ParamId;
        [FieldOffset(8)] public uint Size;
        [FieldOffset(12)] public IntPtr Data;

        public SetParams SetParams
        {
            get
            {
                return (SetParams)(Config & 0x3);
            }
            set
            {
                Config |= ((uint)value & 0x3);
            }
        }
        public bool RuntimeApplicable
        {
            get
            {
                return Convert.ToBoolean((Config >> 2) & 0x1);
            }
            set
            {
                Config |= ((Convert.ToUInt32(value) & 0x1) << 2);
            }
        }
        public AccessType AccessType
        {
            get
            {
                return (AccessType)((Config >> 3) & 0x3);
            }
            set
            {
                Config |= (((uint)value & 0x3) << 3);
            }
        }
        public bool ValueCacheable
        {
            get
            {
                return Convert.ToBoolean((Config >> 5) & 0x1);
            }
            set
            {
                Config |= ((Convert.ToUInt32(value) & 0x1) << 5);
            }
        }
        public bool NotificationCtrl
        {
            get
            {
                return Convert.ToBoolean((Config >> 6) & 0x1);
            }
            set
            {
                Config |= ((Convert.ToUInt32(value) & 0x1) << 6);
            }
        }
    }
}
