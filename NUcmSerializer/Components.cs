using System;
using System.Collections.Generic;
using System.Linq;

namespace NUmcSerializer
{
    public abstract class Section
    {
        public virtual string Identifier { get; set; }
        public string Comment { get; set; }
    }

    public class Ops : Section
    {
        public uint? Get { get; set; }
        public uint? Put { get; set; }
        public uint? Info { get; set; }
    }

    public class ChannelMap : Section
    {
        public string Reg { get; set; }
        public string Shift { get; set; }
    }

    public class DBScale : Section
    {
        public int? Min { get; set; }
        public int? Max { get; set; }
        public int Step { get; set; }
        public byte Mute { get; set; }
    }

    public class SectionData : Section
    {
        private byte[] bytes;
        private ushort[] shorts;
        private uint[] words;

        public string File { get; set; }

        public string Bytes
        {
            get
            {
                return (bytes != null) ? string.Join(",", bytes) : null;
            }

            set
            {
                try
                {
                    bytes = value.Split(',').Select(v => byte.Parse(v)).ToArray();
                }
                catch { }
            }
        }

        public string Shorts
        {
            get
            {
                return (shorts != null) ? string.Join(",", shorts) : null;
            }

            set
            {
                try
                {
                    shorts = value.Split(',').Select(v => ushort.Parse(v)).ToArray();
                }
                catch { }
            }
        }

        public string Words
        {
            get
            {
                return (words != null) ? string.Join(",", words) : null;
            }

            set
            {
                try
                {
                    words = value.Split(',').Select(v => uint.Parse(v)).ToArray();
                }
                catch { }
            }
        }

        public string Tuples { get; set; }
    }

    public abstract class VendorTuples : Section
    {
    }

    public class VendorTuples<T> : VendorTuples
    {
        static readonly Dictionary<Type, string> typeToTupleType =
            new Dictionary<Type, string>()
            {
                { typeof(string), "string" },
                { typeof(Guid), "uuid" },
                { typeof(bool), "bool" },
                { typeof(byte), "byte" },
                { typeof(ushort), "short" },
                { typeof(uint), "word" },
            };

        public static string TupleType { get; }

        public Tuple<string, T>[] Tuples { get; set; }

        public override string Identifier
        {
            get
            {
                return base.Identifier;
            }

            set
            {
                base.Identifier = TupleType + $".{value}";
            }
        }

        static VendorTuples()
        {
            Type type = typeof(T);

            if (typeToTupleType.ContainsKey(type))
                TupleType = typeToTupleType[type];
            else
                TupleType = type.Name;
        }
    }

    public class SectionVendorTokens : Section
    {
        public Tuple<string, uint>[] Tokens { get; set; }
    }

    public class SectionVendorTuples : Section
    {
        public string Tokens { get; set; }

        public VendorTuples[] Tuples { get; set; }
    }

    public class SectionControlMixer : Section
    {
        public uint Index { get; set; }

        public string Texts { get; set; }

        public ChannelMap Channel { get; set; }
        public Ops Ops { get; set; }

        public int? Max { get; set; }
        public int? Min { get; set; }
        public byte Invert { get; set; }

        public string TLV { get; set; }

        public string Data { get; set; }
    }

    public class SectionControlBytes : Section
    {
        public uint Index { get; set; }

        public ChannelMap Channel { get; set; }
        public Ops Ops { get; set; }

        public int? Base { get; set; }
        public int? NumRegs { get; set; }
        public int? Mask { get; set; }
        public int? Min { get; set; }
        public int? Max { get; set; }

        public string TLV { get; set; }

        public string Data { get; set; }
    }

    public class SectionText : Section
    {
        public string[] Values { get; set; }
    }

    public class SectionGraph : Section
    {
        public uint Index { get; set; }
        public string[] Lines { get; set; }
    }

    public class SectionWidget : Section
    {
        public uint Index { get; set; }

        public string Type { get; set; }
        public string StreamName { get; set; }

        public bool? NoPm { get; set; }
        public string Reg { get; set; }
        public string Shift { get; set; }
        public byte Invert { get; set; }
        public int Subseq { get; set; }

        public int? EventType { get; set; }
        public int? EventFlags { get; set; }

        public string Mixer { get; set; }
        public string Enum { get; set; }

        public string Data { get; set; }
    }

    public class SectionPCMCapabilities : Section
    {
        public string Formats { get; set; }
        public uint RateMin { get; set; }
        public uint RateMax { get; set; }
        public byte ChannelMin { get; set; }
        public byte ChannelMax { get; set; }
    }

    public class PCMConfig : Section
    {
        public string Formats { get; set; }
        public uint Rate { get; set; }
        public byte Channels { get; set; }
        public byte TDMSlot { get; set; }
    }

    public class SectionPCMConfig : Section
    {
        public PCMConfig Playback { get; set; }
        public PCMConfig Capture { get; set; }
    }

    public class DAI : Section
    {
        public uint ID { get; set; }
    }

    public class DAILink : Section
    {
        public string Capabilities { get; set; }
        public string[] Configs { get; set; }
    }

    public class SectionPCM : Section
    {
        public uint Index { get; set; }
        public uint ID { get; set; }

        public DAI DAI { get; set; }
        public DAILink Playback { get; set; }
        public DAILink Capture { get; set; }

        public bool? SymmetricRates { get; set; }
        public bool? SymmetricChannels { get; set; }
        public bool? SymmetricSampleBits { get; set; }

        public string Data { get; set; }
    }

    public class SectionLink : Section
    {
        public uint Index { get; set; }
        public uint ID { get; set; }

        public string StreamName { get; set; }
        public string[] Configs { get; set; }
        public uint DefaultHwConfId { get; set; }

        public bool? SymmetricRates { get; set; }
        public bool? SymmetricChannels { get; set; }
        public bool? SymmetricSampleBits { get; set; }

        public string Data { get; set; }
    }

    public class SectionHWConfig : Section
    {
        public uint ID { get; set; }
        public string Format { get; set; }
        public string Bclk { get; set; }
        public string Fsync { get; set; }
    }

    public class SectionDAI : Section
    {
        public uint Index { get; set; }
        public uint ID { get; set; }

        public DAILink Playback { get; set; }
        public DAILink Capture { get; set; }

        public bool? SymmetricRates { get; set; }
        public bool? SymmetricChannels { get; set; }
        public bool? SymmetricSampleBits { get; set; }

        public string Data { get; set; }
    }

    public class SectionManifest : Section
    {
        public string[] Data { get; set; }
    }
}
