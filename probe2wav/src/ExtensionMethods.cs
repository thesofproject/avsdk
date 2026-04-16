//
// Copyright (c) 2026, Intel Corporation. All rights reserved.
//
// Authors: Cezary Rojewski <cezary.rojewski@intel.com>
//          Piotr Maziarz <piotrx.maziarz@linux.intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System.IO;
using System.Runtime.InteropServices;

namespace probe2wav
{
    internal static class ExtensionMethods
    {
        internal static bool IsWavType(this uint probeId)
        {
            return probeId != Native.Constants.BASEFW_PROBE_ID;
        }

        internal static long Remaining(this BinaryReader reader)
        {
            return reader.BaseStream.Length - reader.BaseStream.Position;
        }

        internal static uint PeekUInt32(this BinaryReader reader)
        {
            long pos = reader.BaseStream.Position;
            uint result = reader.ReadUInt32();

            reader.BaseStream.Position = pos;
            return result;
        }

        internal static T Read<T>(this BinaryReader reader)
            where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] bytes = reader.ReadBytes(size);

            return MarshalHelper.BytesToStructure<T>(bytes);
        }

        internal static int Write<T>(this BinaryWriter writer, T value)
            where T : struct
        {
            byte[] bytes = MarshalHelper.StructureToBytes<T>(value);

            writer.Write(bytes);
            return bytes.Length;
        }
    }
}
