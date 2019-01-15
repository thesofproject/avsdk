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
        {
            Identifier = IDENTIFIER;

            IEnumerable<SKL_TKN> values = Enum.GetValues(typeof(SKL_TKN)).Cast<SKL_TKN>();
            Tokens = values.Select(
                t => Tuple.Create(t.GetName(), (uint)t)).ToArray();
        }
    }

    [UmcSection("SectionVendorTuples")]
    public class SectionSkylakeTuples : SectionVendorTuples
    {
        public SectionSkylakeTuples(string identifier)
        {
            Identifier = identifier;
            Tokens = SectionSkylakeTokens.IDENTIFIER;
        }
    }
}
