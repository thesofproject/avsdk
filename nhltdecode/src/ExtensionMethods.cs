//
// Copyright (c) 2020-2023, Intel Corporation. All rights reserved.
//
// Authors: Piotr Maziarz <piotrx.maziarz@linux.intel.com>
//          Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace nhltdecode
{
    internal static class ExtensionMethods
    {
        internal static uint PopCount(uint i)
        {
            return (i & 0x01) + ((i >> 1) & 0x01);
        }

        internal static bool TryUInt32(this string value, out uint result)
        {
            if (value.StartsWith("0x", StringComparison.CurrentCulture))
                return uint.TryParse(value.Substring(2), NumberStyles.HexNumber,
                              CultureInfo.CurrentCulture, out result);

            return uint.TryParse(value, out result);
        }

        internal static uint ToUInt32(this string value)
        {
            TryUInt32(value, out uint result);
            return result;
        }

        internal static ushort ToUInt16(this string value)
        {
            TryUInt32(value, out uint result);
            return (ushort)result;
        }

        internal static byte ToUInt8(this string value)
        {
            TryUInt32(value, out uint result);
            return (byte)result;
        }

        internal static byte PeekByte(this BinaryReader reader)
        {
            long pos = reader.BaseStream.Position;
            byte result = reader.ReadByte();

            reader.BaseStream.Position = pos;
            return result;
        }

        internal static uint PeekUInt32(this BinaryReader reader)
        {
            long pos = reader.BaseStream.Position;
            uint result = reader.ReadUInt32();

            reader.BaseStream.Position = pos;
            return result;
        }

        internal static T Peek<T>(this BinaryReader reader)
            where T : struct
        {
            long pos = reader.BaseStream.Position;
            T result = reader.Read<T>();

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
    }
}
