﻿//
// Copyright (c) 2020-2022, Intel Corporation. All rights reserved.
//
// Authors: Piotr Maziarz <piotrx.maziarz@linux.intel.com>
//          Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.Globalization;
using System.IO;

namespace nhltdecode
{
    internal static class ExtensionMethods
    {
        internal static byte CalculateChecksum(this NHLT table)
        {
            var writer = new BinaryWriter(new MemoryStream());
            int checksum = 0;

            table.WriteToBinary(writer);
            writer.BaseStream.Seek(0, SeekOrigin.Begin);

            while (true)
            {
                int oneByte = writer.BaseStream.ReadByte();
                if (oneByte < 0)
                    break;
                checksum += oneByte;
            }

            writer.Close();
            return (byte)(256 - checksum + table.Header.Checksum);
        }

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
    }
}
