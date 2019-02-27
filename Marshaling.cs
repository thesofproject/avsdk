// Marshaling.cs - Marshal helpers
//
// Copyright (c) 2018-2019 Intel Corporation
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// The source code contained or described herein and all documents
// related to the source code ("Material") are owned by Intel Corporation
// or its suppliers or licensors. Title to the Material remains with
// Intel Corporation or its suppliers and licensors. The Material contains
// trade secrets and proprietary and confidential information of Intel or
// its suppliers and licensors. The Material is protected by worldwide
// copyright and trade secret laws and treaty provisions. No part of the
// Material may be used, copied, reproduced, modified, published, uploaded,
// posted, transmitted, distributed, or disclosed in any way without Intel's
// prior express written permission.
//
// No license under any patent, copyright, trade secret or other intellectual
// property right is granted to or conferred upon you by disclosure or
// delivery of the Materials, either expressly, by implication, inducement,
// estoppel or otherwise. Any license under such intellectual property
// rights must be express and approved by Intel in writing.
//

using System;
using System.Runtime.InteropServices;

namespace itt
{
    internal class MarshalHelper
    {
        internal static byte[] StructureToBytes<T>(T str)
            where T : struct
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];
            GCHandle h = default(GCHandle);

            try
            {
                h = GCHandle.Alloc(arr, GCHandleType.Pinned);
                Marshal.StructureToPtr(str, h.AddrOfPinnedObject(), false);
            }
            finally
            {
                if (h.IsAllocated)
                {
                    h.Free();
                }
            }

            return arr;
        }
    }

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
