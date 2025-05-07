using System;
using System.Linq;
using NUcmSerializer;
using Xunit;

namespace NUcmSerializerTests
{
    public class SectionTests
    {
        SectionManifest section;

        public SectionTests()
        {
            section = new SectionManifest();
        }

        [Fact]
        public void TestPropertyComment()
        {
            string comment = "something descriptive";

            section.Comment = comment;
            Assert.Equal(comment, section.Comment);
            section.Comment = null;
            Assert.Null(section.Comment);
        }
    }

    public class OpsTests
    {
        Ops section;

        public OpsTests()
        {
            section = new Ops();
        }

        [Fact]
        public void TestConstructor()
        {
            new Ops(string.Empty);
            new Ops(null);
            new Ops();
        }

        [Fact]
        public void TestPropertyGet()
        {
            uint op = 257;

            section.Get = op;
            Assert.Equal(op, section.Get);
            section.Get = null;
            Assert.Null(section.Get);
        }

        [Fact]
        public void TestPropertyPut()
        {
            uint op = 257;

            section.Put = op;
            Assert.Equal(op, section.Put);
            section.Put = null;
            Assert.Null(section.Put);
        }

        [Fact]
        public void TestPropertyInfo()
        {
            uint op = 257;

            section.Info = op;
            Assert.Equal(op, section.Info);
            section.Info = null;
            Assert.Null(section.Info);
        }
    }

    public class ChannelMapTests
    {
        ChannelMap section;

        public ChannelMapTests()
        {
            section = new ChannelMap();
        }

        [Fact]
        public void TestConstructor()
        {
            new ChannelMap(string.Empty);
            new ChannelMap(null);
            new ChannelMap();
        }

        [Fact]
        public void TestPropertyIdentifier()
        {
            string s;

            s = section.Identifier;
            Assert.Equal(s, section.Identifier);

            s = "definitely not a channel name";
            section.Identifier = s;
            Assert.NotEqual(s, section.Identifier);

            s = ChannelName.FrontLeftWide;
            section.Identifier = s;
            Assert.Equal(s, section.Identifier);
        }

        [Fact]
        public void TestPropertyReg()
        {
            int reg = 43;

            section.Reg = reg;
            Assert.Equal(reg, section.Reg);
        }

        [Fact]
        public void TestPropertyShift()
        {
            int shift = -1;

            section.Shift = shift;
            Assert.Equal(shift, section.Shift);
        }
    }

    public class DBScaleTests
    {
        DBScale section;

        public DBScaleTests()
        {
            section = new DBScale();
        }

        [Fact]
        public void TestConstructor()
        {
            new DBScale(string.Empty);
            new DBScale(null);
            new DBScale();
        }

        [Fact]
        public void TestPropertyMin()
        {
            int min = -1;

            section.Min = min;
            Assert.Equal(min, section.Min);
            section.Min = null;
            Assert.Null(section.Min);
        }

        [Fact]
        public void TestPropertyStep()
        {
            int step = 1;

            section.Step = step;
            Assert.Equal(step, section.Step);
        }

        [Fact]
        public void TestPropertyMute()
        {
            byte mute = 1;

            section.Mute = mute;
            Assert.Equal(mute, section.Mute);
        }
    }

    public class SectionDataTests
    {
        SectionData section;

        public SectionDataTests()
        {
            section = new SectionData();
        }

        [Fact]
        public void TestConstructor()
        {
            new SectionData(string.Empty);
            new SectionData(null);
            new SectionData();
        }

        [Fact]
        public void TestPropertyFile()
        {
            string file = "123.txt";

            section.File = file;
            Assert.Equal(file, section.File);
            section.File = null;
            Assert.Null(section.File);
        }

        [Fact]
        public void TestPropertyBytesString()
        {
            byte[] bytes;
            string s;

            bytes = new byte[] { 16, 32, 0, 255 };
            s = string.Join(", ", bytes.Select(e => $"0x{e.ToString("X2")}"));

            section.Bytes = bytes;
            Assert.Equal(s, section.BytesString);
            section.BytesString = s;
            Assert.Equal(s, section.BytesString);
            section.BytesString = null;
            Assert.Null(section.BytesString);

            // Expected format provided to ToString() vs any other
            s = string.Join(", ", bytes.Select(e => $"0x{e.ToString("X8")}"));
            Assert.NotEqual(s, section.BytesString);

            // ", " vs ","
            s = string.Join(",", bytes.Select(e => $"0x{e.ToString("X2")}"));
            Assert.NotEqual(s, section.BytesString);

            // 0x prefix
            s = string.Join(",", bytes.Select(e => $"{e.ToString("X2")}"));
            Assert.NotEqual(s, section.BytesString);
        }

        [Fact]
        public void TestPropertyShortsString()
        {
            ushort[] shorts;
            string s;

            shorts = new ushort[] { 999, 1337, 0, 65535 };
            s = string.Join(", ", shorts.Select(e => $"0x{e.ToString("X4")}"));

            section.Shorts = shorts;
            Assert.Equal(s, section.ShortsString);
            section.ShortsString = s;
            Assert.Equal(s, section.ShortsString);
            section.ShortsString = null;
            Assert.Null(section.ShortsString);

            // Expected format provided to ToString() vs any other
            s = string.Join(", ", shorts.Select(e => $"0x{e.ToString("X8")}"));
            Assert.NotEqual(s, section.ShortsString);

            // ", " vs ","
            s = string.Join(",", shorts.Select(e => $"0x{e.ToString("X4")}"));
            Assert.NotEqual(s, section.ShortsString);

            // 0x prefix
            s = string.Join(",", shorts.Select(e => $"{e.ToString("X4")}"));
            Assert.NotEqual(s, section.ShortsString);
        }

        [Fact]
        public void TestPropertyWordsString()
        {
            uint[] words;
            string s;

            words = new uint[] { 999, 1337, 0, 213511 };
            s = string.Join(", ", words.Select(e => $"0x{e.ToString("X8")}"));

            section.Words = words;
            Assert.Equal(s, section.WordsString);
            section.WordsString = s;
            Assert.Equal(s, section.WordsString);
            section.WordsString = null;
            Assert.Null(section.WordsString);

            // Expected format provided to ToString() vs any other
            s = string.Join(", ", words.Select(e => $"0x{e.ToString("X4")}"));
            Assert.NotEqual(s, section.WordsString);

            // ", " vs ","
            s = string.Join(",", words.Select(e => $"0x{e.ToString("X8")}"));
            Assert.NotEqual(s, section.WordsString);

            // 0x prefix
            s = string.Join(",", words.Select(e => $"{e.ToString("X8")}"));
            Assert.NotEqual(s, section.WordsString);
        }

        [Fact]
        public void TestPropertyTuples()
        {
            string[] tuples = new[] { "map", "data", "priv" };

            section.Tuples = tuples;
            Assert.Equal(tuples, section.Tuples);
            section.Tuples = null;
            Assert.Null(section.Tuples);
        }

        [Fact]
        public void TestPropertyType()
        {
            uint type = 16;

            section.Type = type;
            Assert.Equal(type, section.Type);
            section.Type = null;
            Assert.Null(section.Type);
        }
    }

    public class VendorTuplesTests
    {
        [Fact]
        public void TestGetElementSize()
        {
            // Standard tuple types
            Assert.Equal(VendorTuples.CTL_ELEM_ID_NAME_MAXLEN, VendorTuples.GetElementSize<string>());
            Assert.Equal(16 * sizeof(byte), VendorTuples.GetElementSize<Guid>());
            Assert.Equal(sizeof(uint), VendorTuples.GetElementSize<bool>());
            Assert.Equal(sizeof(uint), VendorTuples.GetElementSize<byte>());
            Assert.Equal(sizeof(uint), VendorTuples.GetElementSize<ushort>());
            Assert.Equal(sizeof(uint), VendorTuples.GetElementSize<uint>());

            // Non-standard - no matching ALSA tuple types
            Assert.Equal(sizeof(uint), VendorTuples.GetElementSize<int>());
        }
    }

    public class VendorTuplesTTests
    {
        VendorTuples<uint> section;

        public VendorTuplesTTests()
        {
            section = new VendorTuples<uint>();
        }

        [Fact]
        public void TestPropertyTupleType()
        {
            // Standard tuple types
            Assert.Equal("string", VendorTuples<string>.TupleType);
            Assert.Equal("uuid", VendorTuples<Guid>.TupleType);
            Assert.Equal("bool", VendorTuples<bool>.TupleType);
            Assert.Equal("byte", VendorTuples<byte>.TupleType);
            Assert.Equal("short", VendorTuples<ushort>.TupleType);
            Assert.Equal("word", VendorTuples<uint>.TupleType);

            // Non-standard - no matching ALSA tuple types
            Assert.Equal(typeof(int).Name, VendorTuples<int>.TupleType);
        }

        [Fact]
        public void TestConstructor()
        {
            // Standard tuple types
            new VendorTuples<string>();
            new VendorTuples<Guid>();
            new VendorTuples<bool>();
            new VendorTuples<byte>();
            new VendorTuples<ushort>();
            new VendorTuples<uint>();

            // Non-standard - no matching ALSA tuple types
            new VendorTuples<int>();

            new VendorTuples<uint>(string.Empty);
            new VendorTuples<uint>(null);
        }

        [Fact]
        public void TestPropertyIdentifier()
        {
            string s = "unique string";

            section.Identifier = s;
            // Expected identifier goes in form of: "<type name>.<custom id>"
            s = $"{VendorTuples.TupleTypes[typeof(uint)]}.{s}";
            Assert.Equal(s, section.Identifier);

            s = VendorTuples.TupleTypes[typeof(uint)];

            section.Identifier = string.Empty;
            Assert.NotEqual(string.Empty, section.Identifier);
            Assert.Equal(s, section.Identifier);

            section.Identifier = null;
            Assert.NotNull(section.Identifier);
            Assert.Equal(s, section.Identifier);
        }

        [Fact]
        public void TestSize()
        {
            int size;

            // Expected size of single word-tuple is 8 bytes.
            size = sizeof(uint) + VendorTuples.GetElementSize<uint>();
            section.Tuples = new Tuple<string, uint>[]
            {
                new Tuple<string, uint>("name", 1),
            };
            Assert.Equal(size, section.Size());

            section.Tuples = new Tuple<string, uint>[] { };
            Assert.Equal(0, section.Size());

            new VendorTuples<uint>().Size();
        }
    }

    public class SectionVendorTokensTests
    {
        SectionVendorTokens section;

        public SectionVendorTokensTests()
        {
            section = new SectionVendorTokens();
        }

        [Fact]
        public void TestConstructor()
        {
            new SectionVendorTokens(string.Empty);
            new SectionVendorTokens(null);
            new SectionVendorTokens();
        }

        [Fact]
        public void TestPropertyTokens()
        {
            Tuple<string, uint>[] tokens = new[]
            {
                new Tuple<string, uint>("hex", 999),
                new Tuple<string, uint>("data", 1337),
                new Tuple<string, uint>("priv", 0),
                new Tuple<string, uint>(string.Empty, 213511),
            };

            section.Tokens = tokens;
            Assert.Equal(tokens, section.Tokens);
            section.Tokens = Array.Empty<Tuple<string, uint>>();
            Assert.Empty(section.Tokens);
            section.Tokens = null;
            Assert.Null(section.Tokens);
        }
    }

    public class SectionVendorTuplesTests
    {
        SectionVendorTuples section;

        public SectionVendorTuplesTests()
        {
            section = new SectionVendorTuples();
        }

        [Fact]
        public void TestConstructor()
        {
            new SectionVendorTuples(string.Empty);
            new SectionVendorTuples(null);
            new SectionVendorTuples();
        }

        [Fact]
        public void TestPropertyTokens()
        {
            string s = "my tokens";

            section.Tokens = s;
            Assert.Equal(s, section.Tokens);
            section.Tokens = null;
            Assert.Null(section.Tokens);
        }

        [Fact]
        public void TestPropertyTuples()
        {
            VendorTuples[] tuples = Array.Empty<VendorTuples>();

            section.Tuples = tuples;
            Assert.Equal(tuples, section.Tuples);

            section.Tuples = null;
            Assert.Null(section.Tuples);
        }

        [Fact]
        public void TestSize()
        {
            VendorTuples[] tuples = Array.Empty<VendorTuples>();

            section.Tuples = tuples;
            Assert.Equal(0, section.Size());

            section.Tuples = null;
            Assert.Equal(0, section.Size());
        }
    }

    public class SectionControlTests
    {
        SectionControl section;

        public SectionControlTests()
        {
            section = new SectionControlMixer();
        }

        [Fact]
        public void TestPropertyIndex()
        {
            uint index = 1;

            section.Index = index;
            Assert.Equal(index, section.Index);
        }

        [Fact]
        public void TestPropertyChannel()
        {
            ChannelMap[] map = new[] { new ChannelMap(), null };

            section.Channel = map;
            Assert.Equal(map, section.Channel);
            section.Channel = null;
            Assert.Null(section.Channel);
        }

        [Fact]
        public void TestPropertyOps()
        {
            Ops ops = new Ops();

            section.Ops = ops;
            Assert.Equal(ops, section.Ops);
            section.Ops = null;
            Assert.Null(section.Ops);
        }

        [Fact]
        public void TestPropertyAccess()
        {
            CTL_ELEM_ACCESS[] access = new[] { CTL_ELEM_ACCESS.READ, CTL_ELEM_ACCESS.VOLATILE };

            section.Access = access;
            Assert.Equal(access, section.Access);
            section.Access = null;
            Assert.Null(section.Access);
        }

        [Fact]
        public void TestPropertyData()
        {
            string data = "section";

            section.Data = data;
            Assert.Equal(data, section.Data);
            section.Data = null;
            Assert.Null(section.Data);
        }
    }

    public class SectionControlMixerTests
    {
        SectionControlMixer section;

        public SectionControlMixerTests()
        {
            section = new SectionControlMixer();
        }

        [Fact]
        public void TestConstructor()
        {
            new SectionControlMixer(string.Empty);
            new SectionControlMixer(null);
            new SectionControlMixer();
        }

        [Fact]
        public void TestPropertyMax()
        {
            int max = 1;

            section.Max = max;
            Assert.Equal(max, section.Max);
            section.Max = null;
            Assert.Null(section.Max);
        }

        [Fact]
        public void TestPropertyInvert()
        {
            bool invert = true;

            section.Invert = invert;
            Assert.Equal(invert, section.Invert);
        }

        [Fact]
        public void TestPropertyTLV()
        {
            string tlv = "mixer_private";

            section.TLV = tlv;
            Assert.Equal(tlv, section.TLV);
            section.TLV = null;
            Assert.Null(section.TLV);
        }
    }

    public class SectionControlBytesTests
    {
        SectionControlBytes section;

        public SectionControlBytesTests()
        {
            section = new SectionControlBytes();
        }

        [Fact]
        public void TestConstructor()
        {
            new SectionControlBytes(string.Empty);
            new SectionControlBytes(null);
            new SectionControlBytes();
        }

        [Fact]
        public void TestPropertyOps()
        {
            Ops ops = new Ops();

            section.ExtOps = ops;
            Assert.Equal(ops, section.ExtOps);
            section.ExtOps = null;
            Assert.Null(section.ExtOps);
        }

        [Fact]
        public void TestPropertyBase()
        {
            int val = 10;

            section.Base = val;
            Assert.Equal(val, section.Base);
            section.Base = null;
            Assert.Null(section.Base);
        }

        [Fact]
        public void TestPropertyNumRegs()
        {
            int num = 88;

            section.NumRegs = num;
            Assert.Equal(num, section.NumRegs);
            section.NumRegs = null;
            Assert.Null(section.NumRegs);
        }

        [Fact]
        public void TestPropertyMask()
        {
            int mask = 0xFFFF;

            section.Mask = mask;
            Assert.Equal(mask, section.Mask);
            section.Mask = null;
            Assert.Null(section.Mask);
        }

        [Fact]
        public void TestPropertyMax()
        {
            int max = 55555;

            section.Max = max;
            Assert.Equal(max, section.Max);
            section.Max = null;
            Assert.Null(section.Max);
        }

        [Fact]
        public void TestPropertyTLV()
        {
            string tlv = "bytes_private";

            section.TLV = tlv;
            Assert.Equal(tlv, section.TLV);
            section.TLV = null;
            Assert.Null(section.TLV);
        }
    }

    public class SectionControlEnumTests
    {
        SectionControlEnum section;

        public SectionControlEnumTests()
        {
            section = new SectionControlEnum();
        }

        [Fact]
        public void TestConstructor()
        {
            new SectionControlEnum(string.Empty);
            new SectionControlEnum(null);
            new SectionControlEnum();
        }

        [Fact]
        public void TestPropertyData()
        {
            string text = "something important";

            section.Texts = text;
            Assert.Equal(text, section.Texts);
            section.Texts = null;
            Assert.Null(section.Texts);
        }
    }

    public class SectionTextTests
    {
        SectionText section;

        public SectionTextTests()
        {
            section = new SectionText();
        }

        [Fact]
        public void TestConstructor()
        {
            new SectionText(string.Empty);
            new SectionText(null);
            new SectionText();
        }

        [Fact]
        public void TestPropertyValues()
        {
            string[] values = new[] { "one", "two", "three" };

            section.Values = values;
            Assert.Equal(values, section.Values);
            section.Values = null;
            Assert.Null(section.Values);
        }
    }

    public class SectionGraphTests
    {
        SectionGraph section;

        public SectionGraphTests()
        {
            section = new SectionGraph();
        }

        [Fact]
        public void TestConstructor()
        {
            new SectionGraph(string.Empty);
            new SectionGraph(null);
            new SectionGraph();
        }

        [Fact]
        public void TestPropertyIndex()
        {
            uint index = 765;

            section.Index = index;
            Assert.Equal(index, section.Index);
        }

        [Fact]
        public void TestPropertyLines()
        {
            string[] lines = new[] { "sink, ctrl, source", "sink, null, source" };

            section.Lines = lines;
            Assert.Equal(lines, section.Lines);
            section.Lines = null;
            Assert.Null(section.Lines);
        }
    }

    public class SectionWidgetTests
    {
        SectionWidget section;

        public SectionWidgetTests()
        {
            section = new SectionWidget();
        }

        [Fact]
        public void TestConstructor()
        {
            new SectionWidget(string.Empty);
            new SectionWidget(null);
            new SectionWidget();
        }

        [Fact]
        public void TestPropertyIndex()
        {
            uint index = 17;

            section.Index = index;
            Assert.Equal(index, section.Index);
        }

        [Fact]
        public void TestPropertyType()
        {
            TPLG_DAPM type = TPLG_DAPM.EFFECT;

            section.Type = type;
            Assert.Equal(type, section.Type);
        }

        [Fact]
        public void TestPropertyStreamName()
        {
            string name = "headphones playback";

            section.StreamName = name;
            Assert.Equal(name, section.StreamName);
            section.StreamName = null;
            Assert.Null(section.StreamName);
        }

        [Fact]
        public void TestPropertyNoPm()
        {
            bool flag = true;

            section.NoPm = flag;
            Assert.Equal(flag, section.NoPm);
            section.NoPm = null;
            Assert.Null(section.NoPm);
        }

        [Fact]
        public void TestPropertyReg()
        {
            int reg = 0xFD;

            section.Reg = reg;
            Assert.Equal(reg, section.Reg);
            section.Reg = null;
            Assert.Null(section.Reg);
        }

        [Fact]
        public void TestPropertyShift()
        {
            int shift = -26;

            section.Shift = shift;
            Assert.Equal(shift, section.Shift);
            section.Shift = null;
            Assert.Null(section.Shift);
        }

        [Fact]
        public void TestPropertyInvert()
        {
            bool invert = true;

            section.Invert = invert;
            Assert.Equal(invert, section.Invert);
            section.Invert = null;
            Assert.Null(section.Invert);
        }

        [Fact]
        public void TestPropertySubseq()
        {
            uint subseq = 101;

            section.Subseq = subseq;
            Assert.Equal(subseq, section.Subseq);
            section.Subseq = null;
            Assert.Null(section.Subseq);
        }

        [Fact]
        public void TestPropertyEventType()
        {
            uint type = 12;

            section.EventType = type;
            Assert.Equal(type, section.EventType);
            section.EventType = null;
            Assert.Null(section.EventType);
        }

        [Fact]
        public void TestPropertyEventFlags()
        {
            DAPM_EVENT evt = DAPM_EVENT.PRE_POST_PMD;

            section.EventFlags = evt;
            Assert.Equal(evt, section.EventFlags);
            section.EventFlags = null;
            Assert.Null(section.EventFlags);
        }

        [Fact]
        public void TestPropertyMixer()
        {
            string[] ctls = new[] { "mixer", "control9", null };

            section.Mixer = ctls;
            Assert.Equal(ctls, section.Mixer);
            section.Mixer = null;
            Assert.Null(section.Mixer);
        }

        [Fact]
        public void TestPropertyEnum()
        {
            string[] ctls = new[] { "control86", null, "enum" };

            section.Enum = ctls;
            Assert.Equal(ctls, section.Enum);
            section.Enum = null;
            Assert.Null(section.Enum);
        }

        [Fact]
        public void TestPropertyBytes()
        {
            string[] ctls = new[] { null, "bytes", "control0" };

            section.Bytes = ctls;
            Assert.Equal(ctls, section.Bytes);
            section.Bytes = null;
            Assert.Null(section.Bytes);
        }

        [Fact]
        public void TestPropertyData()
        {
            string[] data = new[] { "data", "mixer", null };

            section.Data = data;
            Assert.Equal(data, section.Data);
            section.Data = null;
            Assert.Null(section.Data);
        }
    }

    public class SectionPCMCapabilitiesTests
    {
        SectionPCMCapabilities section;

        public SectionPCMCapabilitiesTests()
        {
            section = new SectionPCMCapabilities();
        }

        [Fact]
        public void TestConstructor()
        {
            new SectionPCMCapabilities(string.Empty);
            new SectionPCMCapabilities(null);
            new SectionPCMCapabilities();
        }

        [Fact]
        public void TestPropertyFormatsString()
        {
            string formats = "23, abc, 24, U24_BE, 0";

            section.FormatsString = formats;
            Assert.NotEqual(formats, section.FormatsString);
            // Only valid formats should be left.
            Assert.Equal("U24_BE", section.FormatsString);

            section.FormatsString = null;
            Assert.NotNull(section.FormatsString);
            Assert.Equal(string.Empty, section.FormatsString);
        }

        [Fact]
        public void TestPropertyRatesString()
        {
            string rates = "1, 0, 192001, _192000, 96000, d48000";

            section.RatesString = rates;
            Assert.NotEqual(rates, section.RatesString);
            // Only valid rates should be left.
            Assert.Equal("96000", section.RatesString);

            section.RatesString = null;
            Assert.NotNull(section.RatesString);
            Assert.Equal(string.Empty, section.RatesString);
        }

        [Fact]
        public void TestPropertyRateMin()
        {
            uint min = 8000;

            section.RateMin = min;
            Assert.Equal(min, section.RateMin);
            section.RateMin = null;
            Assert.Null(section.RateMin);
        }

        [Fact]
        public void TestPropertyRateMax()
        {
            uint max = 192000;

            section.RateMax = max;
            Assert.Equal(max, section.RateMax);
            section.RateMax = null;
            Assert.Null(section.RateMax);
        }

        [Fact]
        public void TestPropertyChannelsMin()
        {
            uint min = 2;

            section.ChannelsMin = min;
            Assert.Equal(min, section.ChannelsMin);
            section.ChannelsMin = null;
            Assert.Null(section.ChannelsMin);
        }

        [Fact]
        public void TestPropertyChannelsMax()
        {
            uint max = 8;

            section.ChannelsMax = max;
            Assert.Equal(max, section.ChannelsMax);
            section.ChannelsMax = null;
            Assert.Null(section.ChannelsMax);
        }

        [Fact]
        public void TestPropertyPeriodsMin()
        {
            uint min = 2;

            section.PeriodsMin = min;
            Assert.Equal(min, section.PeriodsMin);
            section.PeriodsMin = null;
            Assert.Null(section.PeriodsMin);
        }

        [Fact]
        public void TestPropertyPeriodsMax()
        {
            uint max = 32;

            section.PeriodsMax = max;
            Assert.Equal(max, section.PeriodsMax);
            section.PeriodsMax = null;
            Assert.Null(section.PeriodsMax);
        }

        [Fact]
        public void TestPropertyPeriodSizeMin()
        {
            uint min = 192;

            section.PeriodSizeMin = min;
            Assert.Equal(min, section.PeriodSizeMin);
            section.PeriodSizeMin = null;
            Assert.Null(section.PeriodSizeMin);
        }

        [Fact]
        public void TestPropertyPeriodSizeMax()
        {
            uint max = 1048576;

            section.PeriodSizeMax = max;
            Assert.Equal(max, section.PeriodSizeMax);
            section.PeriodSizeMax = null;
            Assert.Null(section.PeriodSizeMax);
        }

        [Fact]
        public void TestPropertyBufferSizeMin()
        {
            uint min = 384;

            section.BufferSizeMin = min;
            Assert.Equal(min, section.BufferSizeMin);
            section.BufferSizeMin = null;
            Assert.Null(section.BufferSizeMin);
        }

        [Fact]
        public void TestPropertyBufferSizeMax()
        {
            uint max = 4194304;

            section.BufferSizeMax = max;
            Assert.Equal(max, section.BufferSizeMax);
            section.BufferSizeMax = null;
            Assert.Null(section.BufferSizeMax);
        }

        [Fact]
        public void TestPropertySigBits()
        {
            uint bits = 24;

            section.SigBits = bits;
            Assert.Equal(bits, section.SigBits);
            section.SigBits = null;
            Assert.Null(section.SigBits);
        }
    }

    public class FE_DAITests
    {
        FE_DAI section;

        public FE_DAITests()
        {
            section = new FE_DAI();
        }

        [Fact]
        public void TestConstructor()
        {
            new FE_DAI(string.Empty);
            new FE_DAI(null);
            new FE_DAI();
        }

        [Fact]
        public void TestPropertyID()
        {
            uint id = 56219834;

            section.ID = id;
            Assert.Equal(id, section.ID);
        }
    }

    public class PCMStreamTests
    {
        PCMStream section;

        public PCMStreamTests()
        {
            section = new PCMStream();
        }

        [Fact]
        public void TestConstructor()
        {
            new PCMStream(string.Empty);
            new PCMStream(null);
            new PCMStream();
        }

        [Fact]
        public void TestPropertyCapabilities()
        {
            string caps = "pcm capabilities";

            section.Capabilities = caps;
            Assert.Equal(caps, section.Capabilities);
            section.Capabilities = null;
            Assert.Null(section.Capabilities);
        }
    }

    public class SectionPCMTests
    {
        SectionPCM section;

        public SectionPCMTests()
        {
            section = new SectionPCM();
        }

        [Fact]
        public void TestConstructor()
        {
            new SectionPCM(string.Empty);
            new SectionPCM(null);
            new SectionPCM();
        }

        [Fact]
        public void TestPropertyIndex()
        {
            uint index = 74892;

            section.Index = index;
            Assert.Equal(index, section.Index);
        }

        [Fact]
        public void TestPropertyID()
        {
            uint id = 3886;

            section.ID = id;
            Assert.Equal(id, section.ID);
        }

        [Fact]
        public void TestPropertyDAI()
        {
            FE_DAI dai = new FE_DAI();

            section.DAI = dai;
            Assert.Equal(dai, section.DAI);
            section.DAI = null;
            Assert.Null(section.DAI);
        }

        [Fact]
        public void TestPropertyPlayback()
        {
            PCMStream stream = new PCMStream();

            section.Playback = stream;
            Assert.Equal(stream, section.Playback);
            section.Playback = null;
            Assert.Null(section.Playback);
        }

        [Fact]
        public void TestPropertyCapture()
        {
            PCMStream stream = new PCMStream();

            section.Capture = stream;
            Assert.Equal(stream, section.Capture);
            section.Capture = null;
            Assert.Null(section.Capture);
        }

        [Fact]
        public void TestPropertyCompress()
        {
            bool flag = true;

            section.Compress = flag;
            Assert.Equal(flag, section.Compress);
            section.Compress = null;
            Assert.Null(section.Compress);
        }

        [Fact]
        public void TestPropertySymmetricRates()
        {
            bool flag = true;

            section.SymmetricRates = flag;
            Assert.Equal(flag, section.SymmetricRates);
            section.SymmetricRates = null;
            Assert.Null(section.SymmetricRates);
        }

        [Fact]
        public void TestPropertySymmetricChannels()
        {
            bool flag = true;

            section.SymmetricChannels = flag;
            Assert.Equal(flag, section.SymmetricChannels);
            section.SymmetricChannels = null;
            Assert.Null(section.SymmetricChannels);
        }

        [Fact]
        public void TestPropertySymmetricSampleBits()
        {
            bool flag = true;

            section.SymmetricSampleBits = flag;
            Assert.Equal(flag, section.SymmetricSampleBits);
            section.SymmetricSampleBits = null;
            Assert.Null(section.SymmetricSampleBits);
        }

        [Fact]
        public void TestPropertyData()
        {
            string data = "private";

            section.Data = data;
            Assert.Equal(data, section.Data);
            section.Data = null;
            Assert.Null(section.Data);
        }
    }

    public class SectionLinkTests
    {
        SectionLink section;

        public SectionLinkTests()
        {
            section = new SectionLink();
        }

        [Fact]
        public void TestConstructor()
        {
            new SectionLink(string.Empty);
            new SectionLink(null);
            new SectionLink();
        }

        [Fact]
        public void TestPropertyIndex()
        {
            uint index = 1;

            section.Index = index;
            Assert.Equal(index, section.Index);
        }

        [Fact]
        public void TestPropertyID()
        {
            uint id = 5;

            section.ID = id;
            Assert.Equal(id, section.ID);
        }

        [Fact]
        public void TestPropertyStreamName()
        {
            string name = "internal mic";

            section.StreamName = name;
            Assert.Equal(name, section.StreamName);
            section.StreamName = null;
            Assert.Null(section.StreamName);
        }

        [Fact]
        public void TestPropertyHwConfId()
        {
            string[] cfgs = new[] { null, "config0", "config1" };

            section.HwConfigs = cfgs;
            Assert.Equal(cfgs, section.HwConfigs);
            section.HwConfigs = null;
            Assert.Null(section.HwConfigs);
        }

        [Fact]
        public void TestPropertyDefaultHwConfId()
        {
            uint id = 345;

            section.DefaultHwConfId = id;
            Assert.Equal(id, section.DefaultHwConfId);
        }

        [Fact]
        public void TestPropertySymmetricRates()
        {
            bool flag = true;

            section.SymmetricRates = flag;
            Assert.Equal(flag, section.SymmetricRates);
            section.SymmetricRates = null;
            Assert.Null(section.SymmetricRates);
        }

        [Fact]
        public void TestPropertySymmetricChannels()
        {
            bool flag = true;

            section.SymmetricChannels = flag;
            Assert.Equal(flag, section.SymmetricChannels);
            section.SymmetricChannels = null;
            Assert.Null(section.SymmetricChannels);
        }

        [Fact]
        public void TestPropertySymmetricSampleBits()
        {
            bool flag = true;

            section.SymmetricSampleBits = flag;
            Assert.Equal(flag, section.SymmetricSampleBits);
            section.SymmetricSampleBits = null;
            Assert.Null(section.SymmetricSampleBits);
        }

        [Fact]
        public void TestPropertyData()
        {
            string data = "private";

            section.Data = data;
            Assert.Equal(data, section.Data);
            section.Data = null;
            Assert.Null(section.Data);
        }
    }

    public class SectionCCTests
    {
        SectionCC section;

        public SectionCCTests()
        {
            section = new SectionCC();
        }

        [Fact]
        public void TestConstructor()
        {
            new SectionCC(string.Empty);
            new SectionCC(null);
            new SectionCC();
        }

        [Fact]
        public void TestPropertyID()
        {
            uint id = 5;

            section.ID = id;
            Assert.Equal(id, section.ID);
        }
    }

    public class SectionHWConfigTests
    {
        SectionHWConfig section;

        public SectionHWConfigTests()
        {
            section = new SectionHWConfig();
        }

        [Fact]
        public void TestConstructor()
        {
            new SectionHWConfig(string.Empty);
            new SectionHWConfig(null);
            new SectionHWConfig();
        }

        [Fact]
        public void TestPropertyID()
        {
            uint id = 5;

            section.ID = id;
            Assert.Equal(id, section.ID);
        }

        [Fact]
        public void TestPropertyFormat()
        {
            DAI_FORMAT format = DAI_FORMAT.AC97;

            section.Format = format;
            Assert.Equal(format, section.Format);
            section.Format = null;
            Assert.Null(section.Format);
        }

        [Fact]
        public void TestPropertyBclk()
        {
            TPLG_BCLK bclk = TPLG_BCLK.CS;

            section.Bclk = bclk;
            Assert.Equal(bclk, section.Bclk);
            section.Bclk = null;
            Assert.Null(section.Bclk);
        }

        [Fact]
        public void TestPropertyInvertBclk()
        {
            bool invert = true;

            section.InvertBclk = invert;
            Assert.Equal(invert, section.InvertBclk);
            section.InvertBclk = null;
            Assert.Null(section.InvertBclk);
        }

        [Fact]
        public void TestPropertyBclkRate()
        {
            uint rate = 24000000;

            section.BclkRate = rate;
            Assert.Equal(rate, section.BclkRate);
            section.BclkRate = null;
            Assert.Null(section.BclkRate);
        }

        [Fact]
        public void TestPropertyFsync()
        {
            TPLG_FSYNC fsync = TPLG_FSYNC.CS;

            section.Fsync = fsync;
            Assert.Equal(fsync, section.Fsync);
            section.Fsync = null;
            Assert.Null(section.Fsync);
        }

        [Fact]
        public void TestPropertyInvertFsync()
        {
            bool invert = true;

            section.InvertFsync = invert;
            Assert.Equal(invert, section.InvertFsync);
            section.InvertFsync = null;
            Assert.Null(section.InvertFsync);
        }

        [Fact]
        public void TestPropertyFsyncRate()
        {
            uint rate = 19200000;

            section.FsyncRate = rate;
            Assert.Equal(rate, section.FsyncRate);
            section.FsyncRate = null;
            Assert.Null(section.FsyncRate);
        }

        [Fact]
        public void TestPropertyMclk()
        {
            TPLG_MCLK mclk = TPLG_MCLK.CI;

            section.Mclk = mclk;
            Assert.Equal(mclk, section.Mclk);
            section.Mclk = null;
            Assert.Null(section.Mclk);
        }

        [Fact]
        public void TestPropertyMclkRate()
        {
            uint rate = 48000;

            section.MclkRate = rate;
            Assert.Equal(rate, section.MclkRate);
            section.MclkRate = null;
            Assert.Null(section.MclkRate);
        }

        [Fact]
        public void TestPropertyClockGated()
        {
            bool gated = true;

            section.ClockGated = gated;
            Assert.Equal(gated, section.ClockGated);
            section.ClockGated = null;
            Assert.Null(section.ClockGated);
        }

        [Fact]
        public void TestPropertyTdmSlots()
        {
            uint slots = 0x10;

            section.TdmSlots = slots;
            Assert.Equal(slots, section.TdmSlots);
            section.TdmSlots = null;
            Assert.Null(section.TdmSlots);
        }

        [Fact]
        public void TestPropertyTdmSlotWidth()
        {
            uint width = 8;

            section.TdmSlotWidth = width;
            Assert.Equal(width, section.TdmSlotWidth);
            section.TdmSlotWidth = null;
            Assert.Null(section.TdmSlotWidth);
        }

        [Fact]
        public void TestPropertyTxSlots()
        {
            uint slots = 0x01;

            section.TxSlots = slots;
            Assert.Equal(slots, section.TxSlots);
            section.TxSlots = null;
            Assert.Null(section.TxSlots);
        }

        [Fact]
        public void TestPropertyRxSlots()
        {
            uint slots = 0x10;

            section.RxSlots = slots;
            Assert.Equal(slots, section.RxSlots);
            section.RxSlots = null;
            Assert.Null(section.RxSlots);
        }

        [Fact]
        public void TestPropertyTxChannels()
        {
            uint channels = 0x01;

            section.TxChannels = channels;
            Assert.Equal(channels, section.TxChannels);
            section.TxChannels = null;
            Assert.Null(section.TxChannels);
        }

        [Fact]
        public void TestPropertyRxChannels()
        {
            uint channels = 0x10;

            section.RxChannels = channels;
            Assert.Equal(channels, section.RxChannels);
            section.RxChannels = null;
            Assert.Null(section.RxChannels);
        }
    }

    public class SectionDAITests
    {
        SectionDAI section;

        public SectionDAITests()
        {
            section = new SectionDAI();
        }

        [Fact]
        public void TestConstructor()
        {
            new SectionDAI(string.Empty);
            new SectionDAI(null);
            new SectionDAI();
        }

        [Fact]
        public void TestPropertyIndex()
        {
            uint index = 1;

            section.Index = index;
            Assert.Equal(index, section.Index);
        }

        [Fact]
        public void TestPropertyID()
        {
            uint id = 5;

            section.ID = id;
            Assert.Equal(id, section.ID);
        }

        [Fact]
        public void TestPropertySupportsPlayback()
        {
            uint pb = 1;

            section.SupportsPlayback = pb;
            Assert.Equal(pb, section.SupportsPlayback);
            section.SupportsPlayback = null;
            Assert.Null(section.SupportsPlayback);
        }

        [Fact]
        public void TestPropertySupportsCapture()
        {
            uint cp = 1;

            section.SupportsCapture = cp;
            Assert.Equal(cp, section.SupportsCapture);
            section.SupportsCapture = null;
            Assert.Null(section.SupportsCapture);
        }

        [Fact]
        public void TestPropertyPlayback()
        {
            PCMStream stream = new PCMStream();

            section.Playback = stream;
            Assert.Equal(stream, section.Playback);
            section.Playback = null;
            Assert.Null(section.Playback);
        }

        [Fact]
        public void TestPropertyCapture()
        {
            PCMStream stream = new PCMStream();

            section.Capture = stream;
            Assert.Equal(stream, section.Capture);
            section.Capture = null;
            Assert.Null(section.Capture);
        }

        [Fact]
        public void TestPropertySymmetricRates()
        {
            bool flag = true;

            section.SymmetricRates = flag;
            Assert.Equal(flag, section.SymmetricRates);
            section.SymmetricRates = null;
            Assert.Null(section.SymmetricRates);
        }

        [Fact]
        public void TestPropertySymmetricChannels()
        {
            bool flag = true;

            section.SymmetricChannels = flag;
            Assert.Equal(flag, section.SymmetricChannels);
            section.SymmetricChannels = null;
            Assert.Null(section.SymmetricChannels);
        }

        [Fact]
        public void TestPropertySymmetricSampleBits()
        {
            bool flag = true;

            section.SymmetricSampleBits = flag;
            Assert.Equal(flag, section.SymmetricSampleBits);
            section.SymmetricSampleBits = null;
            Assert.Null(section.SymmetricSampleBits);
        }

        [Fact]
        public void TestPropertyData()
        {
            string data = "private";

            section.Data = data;
            Assert.Equal(data, section.Data);
            section.Data = null;
            Assert.Null(section.Data);
        }
    }

    public class SectionManifestTests
    {
        SectionManifest section;

        public SectionManifestTests()
        {
            section = new SectionManifest();
        }

        [Fact]
        public void TestConstructor()
        {
            new SectionManifest(string.Empty);
            new SectionManifest(null);
            new SectionManifest();
        }

        [Fact]
        public void TestPropertyData()
        {
            string[] data = new[] { "block", null, "section_dai" };

            section.Data = data;
            Assert.Equal(data, section.Data);
            section.Data = null;
            Assert.Null(section.Data);
        }
    }
}
