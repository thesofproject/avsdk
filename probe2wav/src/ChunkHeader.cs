//
// Copyright (c) 2020-2022, Intel Corporation. All rights reserved.
//
// Author: Piotr Maziarz <piotrx.maziarz@linux.intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System.IO;
using System.Runtime.InteropServices;

namespace probe2wav
{
    internal static class Constants
    {
        internal static readonly uint SyncPattern = 0xBABEBEBA;
        internal static readonly uint BaseFWProbeId = 0x01000000;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ChunkHeader
    {
        public uint ProbeId;
        public uint ProbeFormat;
        public uint TimestampHigh;
        public uint TimestampLow;
        public uint DataSize;

        public string ProbeIdStr => "0x" + ProbeId.ToString("X8").TrimStart('0');

        // While checksum theoretically is 8 bytes only least significant 4 bytes contains valid data.
        // Most significant 4 bytes should be equal to 0.
        public ulong ExpectedChecksum
        {
            get
            {
                return (ProbeId + ProbeFormat + TimestampHigh + TimestampLow + DataSize +
                    Constants.SyncPattern) & 0xFFFFFFFF;
            }
        }

        public bool Wav => ProbeId != Constants.BaseFWProbeId;

        public static ChunkHeader Create(BinaryReader binaryReader)
        {
            ChunkHeader header;
            header.ProbeId = binaryReader.ReadUInt32();
            header.ProbeFormat = binaryReader.ReadUInt32();
            header.TimestampHigh = binaryReader.ReadUInt32();
            header.TimestampLow = binaryReader.ReadUInt32();
            header.DataSize = binaryReader.ReadUInt32();

            return header;
        }
    }
}
