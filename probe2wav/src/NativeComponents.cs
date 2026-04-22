//
// Copyright (c) 2020-2026, Intel Corporation. All rights reserved.
//
// Authors: Cezary Rojewski <cezary.rojewski@intel.com>
//          Piotr Maziarz <piotrx.maziarz@linux.intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System.Runtime.InteropServices;

namespace probe2wav.Native
{
    public static class Constants
    {
        // Order does matter and shall match the firmware equivalent.
        public static uint[] SAMPLE_RATES =
        {
            8000, 11025, 12000, 16000, 22050, 24000,
            32000, 44100, 48000, 64000, 88200, 96000,
            128000, 176400, 192000
        };

        public const uint SYNC_WORD         = 0xBABEBEBA;
        public const uint BASEFW_PROBE_ID   = 0x01000000;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DataFormat
    {
        public uint Raw;

        public DataFormat(uint r)
        {
            Raw = r;
        }

#pragma warning disable format
        public uint FormatType      => Raw & 0x1;
        public uint StandardType    => (Raw >> 1) & 0xF;
        public uint AudioFormat     => (Raw >> 5) & 0xF;
        public uint SampleRate0     => (Raw >> 9) & 0xF;
        public uint NumChannels0    => (Raw >> 13) & 0x1F;
        public uint SampleSize0     => (Raw >> 18) & 0x3;
        public uint ContainerSize0  => (Raw >> 20) & 0x3;
        public uint SampleFormat    => (Raw >> 22) & 0x1;
        public uint SampleEnd       => (Raw >> 23) & 0x1;
        public uint Interleaving    => (Raw >> 24) & 0x1;

        public uint NumChannels     => NumChannels0 + 1;
        public uint ContainerBits   => (ContainerSize0 + 1) * 8;
#pragma warning restore format
        public uint SampleRate
        {
            get
            {
                int id = (int)SampleRate0;

                if (id < Constants.SAMPLE_RATES.Length)
                    return Constants.SAMPLE_RATES[id];
                return uint.MaxValue;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ProbePacketHeader
    {
        public uint SyncWord;
        public uint ProbeId;
        public DataFormat DataFormat;
        public uint TimestampHigh;
        public uint TimestampLow;
        public uint DataSize;

        public ulong Checksum()
        {
            return (ulong)SyncWord + ProbeId + DataFormat.Raw +
                   TimestampHigh + TimestampLow + DataSize;
        }
    }

    // For documentation purposes only.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ProbePacket
    {
        public ProbePacketHeader Header;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0)]
        public byte[] Data;     // size assigned dynamically based on DataSize
        public uint Checksum;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct WavHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] ChunkID;
        public uint ChunkSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] Format;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] Subchunk1ID;
        public uint Subchunk1Size;
        public ushort AudioFormat;
        public ushort NumChannels;
        public uint SampleRate;
        public uint ByteRate;
        public ushort BlockAlign;
        public ushort BitsPerSample;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] Subchunk2ID;
        public uint Subchunk2Size;
    }
}
