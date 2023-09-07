//
// Copyright (c) 2020-2023, Intel Corporation. All rights reserved.
//
// Authors: Piotr Maziarz <piotrx.maziarz@linux.intel.com>
//          Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System.IO;
using System.Runtime.InteropServices;

namespace nhltdecode
{
    internal static class MarshalHelper
    {
        internal static byte[] StructureToBytes<T>(T str, int size)
            where T : struct
        {
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
                    h.Free();
            }

            return arr;
        }

        internal static byte[] StructureToBytes<T>(T str)
            where T : struct
        {
            return StructureToBytes<T>(str, Marshal.SizeOf(typeof(T)));
        }

        internal static T BytesToStructure<T>(byte[] bytes)
        {
            GCHandle h = default(GCHandle);
            T result;

            try
            {
                h = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                result = (T)Marshal.PtrToStructure(h.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                if (h.IsAllocated)
                    h.Free();
            }

            return result;
        }
    }
}
