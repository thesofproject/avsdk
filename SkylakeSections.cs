// SkylakeSections.cs - SKL Section helpers
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

using NUmcSerializer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace itt
{
    [UmcSection("SectionVendorTokens")]
    public class SectionSkylakeTokens : SectionVendorTokens
    {
        public const string IDENTIFIER = "skl_tokens";

        public SectionSkylakeTokens()
            : base(IDENTIFIER)
        {
            IEnumerable<SKL_TKN> values = Enum.GetValues(typeof(SKL_TKN)).Cast<SKL_TKN>();
            Tokens = values.Select(
                t => Tuple.Create(t.GetName(), (uint)t)).ToArray();
        }
    }

    [UmcSection("SectionVendorTuples")]
    public class SectionSkylakeTuples : SectionVendorTuples
    {
        public SectionSkylakeTuples(string identifier)
            : base(identifier)
        {
            Tokens = SectionSkylakeTokens.IDENTIFIER;
        }
    }

    public static class SectionHelper
    {
        public static SectionVendorTuples GetSizeDescriptor(this Section section, int size,
            SKL_BLOCK_TYPE type = SKL_BLOCK_TYPE.TUPLE)
        {
            var desc = new SectionSkylakeTuples($"{section.Identifier}_size_desc");

            var bytes = new VendorTuples<byte>("u8_block_type");
            bytes.Tuples = new[] { Tuple.Create(SKL_TKN.U8_BLOCK_TYPE.GetName(), (byte)type) };

            var shorts = new VendorTuples<ushort>("u16_size_desc");
            shorts.Tuples = new[] { Tuple.Create(SKL_TKN.U16_BLOCK_SIZE.GetName(), (ushort)size) };
            desc.Tuples = new VendorTuples[] { bytes, shorts };

            return desc;
        }

        public static SectionVendorTuples GetSizeDescriptor(this SectionVendorTuples section,
            SKL_BLOCK_TYPE type = SKL_BLOCK_TYPE.TUPLE)
        {
            return GetSizeDescriptor(section, section.Size(), type);
        }

        public static SectionVendorTuples GetNumDescriptor(this Section section, int num)
        {
            var desc = new SectionSkylakeTuples($"{section.Identifier} num_desc");

            var bytes = new VendorTuples<byte>("u8_num_blocks");
            bytes.Tuples = new[] { Tuple.Create(SKL_TKN.U8_NUM_BLOCKS.GetName(), (byte)num) };
            desc.Tuples = new[] { bytes };

            return desc;
        }

        public static SectionData GetPrivateData(this Section section)
        {
            return new SectionData() { Identifier = section.Identifier, Tuples = section.Identifier };
        }
    }
}
