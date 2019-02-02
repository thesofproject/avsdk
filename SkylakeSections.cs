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
