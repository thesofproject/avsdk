//
// Copyright (c) 2026, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System.Runtime.InteropServices;
using Xunit;

namespace nhltdecode.tests
{
    public class MarshalHelperTests
    {
        [Fact]
        public void StructureToBytes()
        {
            var cfg = new Native.MclkConfig() { MdivCtrl = 0xAABBCCDD, MdivR = 0x11223344 };
            byte[] expected = new byte[]
            {
                0xDD, 0xCC, 0xBB, 0xAA, // MdivCtrl
                0x44, 0x33, 0x22, 0x11  // MdivR
            };

            byte[] actual1 = MarshalHelper.StructureToBytes(cfg);
            byte[] actual2 = MarshalHelper.StructureToBytes(cfg, Marshal.SizeOf<Native.MclkConfig>());

            Assert.Equal(expected, actual1);
            Assert.Equal(expected, actual2);
        }

        [Fact]
        public void BytesToStructure()
        {
            var expected = new Native.MclkConfig() { MdivCtrl = 0xAABBCCDD, MdivR = 0x11223344 };
            byte[] bytes = new byte[]
            {
                0xDD, 0xCC, 0xBB, 0xAA, // MdivCtrl
                0x44, 0x33, 0x22, 0x11  // MdivR
            };

            var actual = MarshalHelper.BytesToStructure<Native.MclkConfig>(bytes);

            Assert.Equal(expected.MdivCtrl, actual.MdivCtrl);
            Assert.Equal(expected.MdivR, actual.MdivR);
        }
    }
}
