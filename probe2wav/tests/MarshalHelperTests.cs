//
// Copyright (c) 2026, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System.Runtime.InteropServices;
using Xunit;
using probe2wav.Native;

namespace probe2wav.tests
{
    public class MarshalHelperTests
    {
        [Fact]
        public void StructureToBytes()
        {
            DataFormat format = new DataFormat(0xDEADBEEF);
            byte[] expected = new byte[] { 0xEF, 0xBE, 0xAD, 0xDE };
            byte[] actual = MarshalHelper.StructureToBytes(format);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BytesToStructure()
        {
            byte[] bytes = new byte[] { 0xEF, 0xBE, 0xAD, 0xDE };
            DataFormat expected = new DataFormat(0xDEADBEEF);
            DataFormat actual = MarshalHelper.BytesToStructure<DataFormat>(bytes);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ConversionRoundtrip()
        {
            var expected = new ProbePacketHeader
            {
                SyncWord = Constants.SYNC_WORD,
                ProbeId = 0x00010002,
                DataFormat = new DataFormat(0x103000),
                TimestampHigh = 0x11223344,
                TimestampLow = 0x55667788,
                DataSize = 256
            };

            byte[] bytes = MarshalHelper.StructureToBytes(expected);
            var actual = MarshalHelper.BytesToStructure<ProbePacketHeader>(bytes);

            Assert.Equal(expected, actual);
        }
    }
}
