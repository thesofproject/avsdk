//
// Copyright (c) 2026, Intel Corporation. All rights reserved.
//
// Authors: Cezary Rojewski <cezary.rojewski@intel.com>
//          Piotr Maziarz <piotrx.maziarz@linux.intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System.Runtime.InteropServices;

namespace probe2wav
{
    internal static class MarshalHelper
    {
        internal static byte[] StructureToBytes<T>(T str)
            where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
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
