//
// Copyright (c) 2018, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0 OR MIT
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace NUcmSerializer
{
    public abstract class Section
    {
        [UcmIdentifier]
        public virtual string Identifier { get; set; }
        [UcmElement("comment")]
        public string Comment { get; set; }

        public Section(string identifier)
        {
            Identifier = identifier;
        }

        public Section()
            : this(null)
        {
        }
    }

    public static class TPLG_CTL
    {
        // individual kcontrol info types
        public const uint VOLSW = 1;
        public const uint VOLSW_SX = 2;
        public const uint VOLSW_XR_SX = 3;
        public const uint ENUM = 4;
        public const uint BYTES = 5;
        public const uint ENUM_VALUE = 6;
        public const uint RANGE = 7;
        public const uint STROBE = 8;

        // individual widget kcontrol info types
        public const uint DAPM_VOLSW = 64;
        public const uint DAPM_ENUM_DOUBLE = 65;
        public const uint DAPM_ENUM_VIRT = 66;
        public const uint DAPM_ENUM_VALUE = 67;
        public const uint DAPM_PIN = 68;
    }

    [UcmSection("ops")]
    public class Ops : Section
    {
        [UcmElement("get")]
        public uint? Get { get; set; }
        [UcmElement("put")]
        public uint? Put { get; set; }
        [UcmElement("info")]
        public uint? Info { get; set; }

        public Ops(string identifier)
            : base(identifier)
        {
        }

        public Ops()
            : this(null)
        {
        }
    }

    public static class ChannelName
    {
        public static readonly string Mono = "mono";
        public static readonly string FrontLeft = "fl";
        public static readonly string FrontRight = "fr";
        public static readonly string RearLeft = "rl";
        public static readonly string RearRight = "rr";
        public static readonly string FrontCenter = "fc";
        public static readonly string LFE = "lfe";
        public static readonly string SideLeft = "sl";
        public static readonly string SideRight = "sr";
        public static readonly string RearCenter = "rc";
        public static readonly string FrontLeftCenter = "flc";
        public static readonly string FrontRightCenter = "frc";
        public static readonly string RearLeftCenter = "rlc";
        public static readonly string RearRightCenter = "rrc";
        public static readonly string FrontLeftWide = "flw";
        public static readonly string FrontRightWide = "frw";
        public static readonly string FrontLeftHigh = "flh";
        public static readonly string FrontCenterHigh = "fch";
        public static readonly string FrontRightHigh = "frh";
        public static readonly string TopCenter = "tc";
        public static readonly string TopFrontLeft = "tfl";
        public static readonly string TopFrontRight = "tfr";
        public static readonly string TopFrontCenter = "tfc";
        public static readonly string TopRearLeft = "trl";
        public static readonly string TopRearRight = "trr";
        public static readonly string TopRearCenter = "trc";
        public static readonly string TopFrontLeftCenter = "tflc";
        public static readonly string TopFrontRightCenter = "tfrc";
        public static readonly string TopSideLeft = "tsl";
        public static readonly string TopSideRight = "tsr";
        public static readonly string LeftLFE = "llfe";
        public static readonly string RightLFE = "rlfe";
        public static readonly string BottomCenter = "bc";
        public static readonly string BottomLeftCenter = "blc";
        public static readonly string BottomRightCenter = "brc";
    }

    [UcmSection("channel")]
    public class ChannelMap : Section
    {
        [UcmElement("reg")]
        public int Reg { get; set; }
        [UcmElement("shift")]
        public int Shift { get; set; }

        [UcmIdentifier]
        public override string Identifier
        {
            get
            {
                return base.Identifier;
            }

            set
            {
                if (typeof(ChannelName).GetFields()
                        .Any(f => f.GetValue(null).Equals(value)))
                    base.Identifier = value;
            }
        }

        public ChannelMap(string identifier)
            : base(identifier)
        {
        }

        public ChannelMap()
            : this(ChannelName.Mono)
        {
        }
    }

    [UcmSection("scale")]
    public class DBScale : Section
    {
        [UcmElement("min")]
        public int? Min { get; set; }
        [UcmElement("step")]
        public int Step { get; set; }
        [UcmElement("mute")]
        public byte Mute { get; set; }

        public DBScale(string identifier)
            : base(identifier)
        {
        }

        public DBScale()
            : this(ChannelName.Mono)
        {
        }
    }

    public class SectionData : Section
    {
        public byte[] Bytes;
        public ushort[] Shorts;
        public uint[] Words;

        [UcmElement("file"), UcmExclusive("value")]
        public string File { get; set; }

        [UcmElement("bytes"), UcmExclusive("value")]
        public string BytesString
        {
            get
            {
                return (Bytes == null) ? null :
                    string.Join(", ", Bytes.Select(e => $"0x{e.ToString("X2")}"));
            }

            set
            {
                Bytes = value?.ToBytes();
            }
        }

        [UcmElement("shorts"), UcmExclusive("value")]
        public string ShortsString
        {
            get
            {
                return (Shorts == null) ? null :
                    string.Join(", ", Shorts.Select(e => $"0x{e.ToString("X4")}"));
            }

            set
            {
                Shorts = value?.ToUInts16();
            }
        }

        [UcmElement("words"), UcmExclusive("value")]
        public string WordsString
        {
            get
            {
                return (Words == null) ? null :
                    string.Join(", ", Words.Select(e => $"0x{e.ToString("X8")}"));
            }

            set
            {
                Words = value?.ToUInts32();
            }
        }

        [UcmElement("tuples"), UcmExclusive("value")]
        public string[] Tuples { get; set; }
        [UcmElement("type")]
        public uint? Type { get; set; }

        public SectionData(string identifier)
            : base(identifier)
        {
        }

        public SectionData()
            : this(null)
        {
        }
    }

    public abstract class VendorTuples : Section
    {
        public const int CTL_ELEM_ID_NAME_MAXLEN = 44;

        public static readonly Dictionary<Type, string> TupleTypes =
            new Dictionary<Type, string>()
            {
                { typeof(string), "string" },
                { typeof(Guid), "uuid" },
                { typeof(bool), "bool" },
                { typeof(byte), "byte" },
                { typeof(ushort), "short" },
                { typeof(uint), "word" },
            };

        public static int GetElementSize<T>()
        {
            Type type = typeof(T);

            if (type == typeof(string))
                return CTL_ELEM_ID_NAME_MAXLEN;
            else if (type == typeof(Guid))
                return 16 * sizeof(byte);
            return sizeof(uint);
        }

        protected VendorTuples(string identifier)
            : base(identifier)
        {
        }

        protected VendorTuples()
            : this(null)
        {
        }

        public abstract int Size();
    }

    [UcmSection("tuples")]
    public class VendorTuples<T> : VendorTuples
    {
        [UcmIgnore]
        public static string TupleType { get; }

        [UcmArray(Inline = true)]
        public Tuple<string, T>[] Tuples { get; set; }

        [UcmIdentifier]
        public override string Identifier
        {
            get
            {
                return base.Identifier;
            }

            set
            {
                base.Identifier = string.IsNullOrEmpty(value) ? TupleType : $"{TupleType}.{value}";
            }
        }

        static VendorTuples()
        {
            Type type = typeof(T);

            if (TupleTypes.ContainsKey(type))
                TupleType = TupleTypes[type];
            else
                TupleType = type.Name;
        }

        public VendorTuples(string identifier)
            : base(identifier)
        {
        }

        public VendorTuples()
            : this(null)
        {
        }

        public override int Size()
        {
            return (Tuples == null) ?
                0 : (sizeof(uint) + GetElementSize<T>()) * Tuples.Length;
        }
    }

    public class SectionVendorTokens : Section
    {
        [UcmArray(Inline = true)]
        public Tuple<string, uint>[] Tokens { get; set; }

        public SectionVendorTokens(string identifier)
            : base(identifier)
        {
        }

        public SectionVendorTokens()
            : this(null)
        {
        }
    }

    public class SectionVendorTuples : Section
    {
        [UcmElement("tokens")]
        public string Tokens { get; set; }

        [UcmArray("tuples", Inline = true)]
        public VendorTuples[] Tuples { get; set; }

        public SectionVendorTuples(string identifier)
            : base(identifier)
        {
        }

        public SectionVendorTuples()
            : this(null)
        {
        }

        public int Size()
        {
            return (Tuples == null) ? 0 : Tuples.Sum(t => t.Size());
        }
    }

    [Flags]
    public enum CTL_ELEM_ACCESS
    {
        [UcmEnum(Name = "read")]
        READ          = (1 << 0),
        [UcmEnum(Name = "write")]
        WRITE         = (1 << 1),
        [UcmEnum(Name = "read_write")]
        READWRITE     = (READ | WRITE),
        [UcmEnum(Name = "volatile")]
        VOLATILE      = (1 << 2),   // control value may be changed without a notification
        [UcmEnum(Name = "timestamp")]
        TIMESTAMP     = (1 << 3),   // when was control changed
        [UcmEnum(Name = "tlv_read")]
        TLV_READ      = (1 << 4),   // TLV read is possible
        [UcmEnum(Name = "tlv_write")]
        TLV_WRITE     = (1 << 5),   // TLV write is possible
        [UcmEnum(Name = "tlv_read_write")]
        TLV_READWRITE = (TLV_READ | TLV_WRITE),
        [UcmEnum(Name = "tlv_command")]
        TLV_COMMAND   = (1 << 6),   // TLV command is possible
        [UcmEnum(Name = "inactive")]
        INACTIVE      = (1 << 8),   // control does actually nothing, but may be updated
        [UcmEnum(Name = "lock")]
        LOCK          = (1 << 9),   // write lock
        [UcmEnum(Name = "owner")]
        OWNER         = (1 << 10),  // write lock owner
        [UcmEnum(Name = "tlv_callback")]
        TLV_CALLBACK  = (1 << 28),  // kernel use a TLV callback
        [UcmEnum(Name = "user")]
        USER          = (1 << 29)   // user space element
    }

    public abstract class SectionControl : Section
    {
        [UcmElement("index")]
        public uint Index { get; set; }

        [UcmArray("channel", Inline = true)]
        public ChannelMap[] Channel { get; set; }
        [UcmSection("ops")]
        public Ops Ops { get; set; }

        [UcmArray("access")]
        public CTL_ELEM_ACCESS[] Access { get; set; }

        [UcmElement("data")]
        public string Data { get; set; }

        public SectionControl(string identifier)
            : base(identifier)
        {
        }

        public SectionControl()
            : this(null)
        {
        }
    }

    public class SectionControlMixer : SectionControl
    {
        [UcmElement("max")]
        public int? Max { get; set; }
        [UcmElement("invert")]
        public bool Invert { get; set; }

        [UcmElement("tlv")]
        public string TLV { get; set; }

        public SectionControlMixer(string identifier)
            : base(identifier)
        {
        }

        public SectionControlMixer()
            : this(null)
        {
        }
    }

    public class SectionControlBytes : SectionControl
    {
        [UcmSection("extops")]
        public Ops ExtOps { get; set; }

        [UcmElement("base")]
        public int? Base { get; set; }
        [UcmElement("num_regs")]
        public int? NumRegs { get; set; }
        [UcmElement("mask")]
        public int? Mask { get; set; }
        [UcmElement("max")]
        public int? Max { get; set; }

        [UcmElement("tlv")]
        public string TLV { get; set; }

        public SectionControlBytes(string identifier)
            : base(identifier)
        {
        }

        public SectionControlBytes()
            : this(null)
        {
        }
    }

    public class SectionControlEnum : SectionControl
    {
        [UcmElement("texts")]
        public string Texts { get; set; }

        public SectionControlEnum(string identifier)
            : base(identifier)
        {
        }

        public SectionControlEnum()
            : this(null)
        {
        }
    }

    public class SectionText : Section
    {
        [UcmArray("values")]
        public string[] Values { get; set; }

        public SectionText(string identifier)
            : base(identifier)
        {
        }

        public SectionText()
            : this(null)
        {
        }
    }

    public class SectionGraph : Section
    {
        [UcmElement("index")]
        public uint Index { get; set; }
        [UcmArray("lines")]
        public string[] Lines { get; set; }

        public SectionGraph(string identifier)
            : base(identifier)
        {
        }

        public SectionGraph()
            : this(null)
        {
        }
    }

    public enum TPLG_DAPM
    {
        // DAPM widget types - add new items to the end
        [UcmEnum(Name = "input")] INPUT,
        [UcmEnum(Name = "output")] OUTPUT,
        [UcmEnum(Name = "mux")] MUX,
        [UcmEnum(Name = "mixer")] MIXER,
        [UcmEnum(Name = "pga")] PGA,
        [UcmEnum(Name = "out_drv")] OUT_DRV,
        [UcmEnum(Name = "adc")] ADC,
        [UcmEnum(Name = "dac")] DAC,
        [UcmEnum(Name = "switch")] SWITCH,
        [UcmEnum(Name = "pre")] PRE,
        [UcmEnum(Name = "post")] POST,
        [UcmEnum(Name = "aif_in")] AIF_IN,
        [UcmEnum(Name = "aif_out")] AIF_OUT,
        [UcmEnum(Name = "dai_in")] DAI_IN,
        [UcmEnum(Name = "dai_out")] DAI_OUT,
        [UcmEnum(Name = "dai_link")] DAI_LINK,
        [UcmEnum(Name = "buffer")] BUFFER,
        [UcmEnum(Name = "scheduler")] SCHEDULER,
        [UcmEnum(Name = "effect")] EFFECT,
        [UcmEnum(Name = "siggen")] SIGGEN,
        [UcmEnum(Name = "src")] SRC,
        [UcmEnum(Name = "asrc")] ASRC,
        [UcmEnum(Name = "encoder")] ENCODER,
        [UcmEnum(Name = "decoder")] DECODER
    }

    [Flags]
    public enum DAPM_EVENT
    {
        // dapm event types
        PRE_PMU   = 0x1,     // before widget power up
        POST_PMU  = 0x2,     // after widget power up
        PRE_PMD   = 0x4,     // before widget power down
        POST_PMD  = 0x8,     // after widget power down
        PRE_REG   = 0x10,    // before audio path setup
        POST_REG  = 0x20,    // after audio path setup
        WILL_PMU  = 0x40,    // called at start of sequence
        WILL_PMD  = 0x80,    // called at start of sequence
        PRE_POST_PMD = (PRE_PMD | POST_PMD),
    }

    public class SectionWidget : Section
    {
        [UcmElement("index")]
        public uint Index { get; set; }

        [UcmElement("type")]
        public TPLG_DAPM Type { get; set; }
        [UcmElement("stream_name")]
        public string StreamName { get; set; }

        [UcmElement("no_pm")]
        public bool? NoPm { get; set; }
        [UcmElement("reg")]
        public int? Reg { get; set; }
        [UcmElement("shift")]
        public int? Shift { get; set; }
        [UcmElement("invert")]
        public bool? Invert { get; set; }
        [UcmElement("subseq")]
        public uint? Subseq { get; set; }

        [UcmElement("event_type")]
        public uint? EventType { get; set; }
        [UcmElement("event_flags")]
        public DAPM_EVENT? EventFlags { get; set; }

        [UcmElement("mixer"), UcmExclusive("control")]
        public string[] Mixer { get; set; }
        [UcmElement("enum"), UcmExclusive("control")]
        public string[] Enum { get; set; }
        [UcmElement("bytes"), UcmExclusive("control")]
        public string[] Bytes { get; set; }

        [UcmElement("data")]
        public string[] Data { get; set; }

        public SectionWidget(string identifier)
            : base(identifier)
        {
        }

        public SectionWidget()
            : this(null)
        {
        }
    }

    public enum PCM_FORMAT
    {
        S8 = 0,      // Signed 8 bit
        U8,          // Unsigned 8 bit
        S16_LE,      // Signed 16 bit Little Endian
        S16_BE,      // Signed 16 bit Big Endian
        U16_LE,      // Unsigned 16 bit Little Endian
        U16_BE,      // Unsigned 16 bit Big Endian
        S24_LE,      // Signed 24 bit Little Endian using low three bytes in 32-bit word
        S24_BE,      // Signed 24 bit Big Endian using low three bytes in 32-bit word
        U24_LE,      // Unsigned 24 bit Little Endian using low three bytes in 32-bit word
        U24_BE,      // Unsigned 24 bit Big Endian using low three bytes in 32-bit word
        S32_LE,      // Signed 32 bit Little Endian
        S32_BE,      // Signed 32 bit Big Endian
        U32_LE,      // Unsigned 32 bit Little Endian
        U32_BE,      // Unsigned 32 bit Big Endian
        FLOAT_LE,    // Float 32 bit Little Endian, Range -1.0 to 1.0
        FLOAT_BE,    // Float 32 bit Big Endian, Range -1.0 to 1.0
        FLOAT64_LE,  // Float 64 bit Little Endian, Range -1.0 to 1.0
        FLOAT64_BE,  // Float 64 bit Big Endian, Range -1.0 to 1.0
        IEC958_SUBFRAME_LE,  // IEC-958 Little Endian
        IEC958_SUBFRAME_BE,  // IEC-958 Big Endian
        MU_LAW,      // Mu-Law
        A_LAW,       // A-Law
        IMA_ADPCM,   // Ima-ADPCM
        MPEG,        // MPEG
        GSM,         // GSM
        S20_LE,      // Signed 20bit Little Endian in 4bytes format, LSB justified
        S20_BE,      // Signed 20bit Big Endian in 4bytes format, LSB justified
        U20_LE,      // Unsigned 20bit Little Endian in 4bytes format, LSB justified
        U20_BE,      // Unsigned 20bit Big Endian in 4bytes format, LSB justified
        SPECIAL = 31,  // Special
        S24_3LE = 32,  // Signed 24bit Little Endian in 3bytes format
        S24_3BE,     // Signed 24bit Big Endian in 3bytes format
        U24_3LE,     // Unsigned 24bit Little Endian in 3bytes format
        U24_3BE,     // Unsigned 24bit Big Endian in 3bytes format
        S20_3LE,     // Signed 20bit Little Endian in 3bytes format
        S20_3BE,     // Signed 20bit Big Endian in 3bytes format
        U20_3LE,     // Unsigned 20bit Little Endian in 3bytes format
        U20_3BE,     // Unsigned 20bit Big Endian in 3bytes format
        S18_3LE,     // Signed 18bit Little Endian in 3bytes format
        S18_3BE,     // Signed 18bit Big Endian in 3bytes format
        U18_3LE,     // Unsigned 18bit Little Endian in 3bytes format
        U18_3BE,     // Unsigned 18bit Big Endian in 3bytes format
        G723_24,     // G.723 (ADPCM) 24 kbit/s, 8 samples in 3 bytes
        G723_24_1B,  // G.723 (ADPCM) 24 kbit/s, 1 sample in 1 byte
        G723_40,     // G.723 (ADPCM) 40 kbit/s, 8 samples in 3 bytes
        G723_40_1B,  // G.723 (ADPCM) 40 kbit/s, 1 sample in 1 byte
        DSD_U8,      // Direct Stream Digital (DSD) in 1-byte samples (x8)
        DSD_U16_LE,  // Direct Stream Digital (DSD) in 2-byte samples (x16)
        DSD_U32_LE,  // Direct Stream Digital (DSD) in 4-byte samples (x32)
        DSD_U16_BE,  // Direct Stream Digital (DSD) in 2-byte samples (x16)
        DSD_U32_BE   // Direct Stream Digital (DSD) in 4-byte samples (x32)
    }

    public enum PCM_RATE
    {
        _5512 = 0,  // 5512Hz
        _8000,      // 8000Hz
        _11025,     // 11025Hz
        _16000,     // 16000Hz
        _22050,     // 22050Hz
        _32000,     // 32000Hz
        _44100,     // 44100Hz
        _48000,     // 48000Hz
        _64000,     // 64000Hz
        _88200,     // 88200Hz
        _96000,     // 96000Hz
        _176400,    // 176400Hz
        _192000,    // 192000Hz
        CONTINUOUS = 30,  // continuous range
        KNOT = 31   // supports more non-continuos rates
    }

    public class SectionPCMCapabilities : Section
    {
        public readonly HashSet<PCM_FORMAT> Formats;
        public readonly HashSet<PCM_RATE> Rates;

        [UcmElement("formats")]
        public string FormatsString
        {
            get
            {
                return string.Join(", ", Formats);
            }

            set
            {
                string[] substrs = value.Split(new[] { ',' });

                Formats.Clear();
                foreach (var s in substrs)
                    Formats.Add((PCM_FORMAT)Enum.Parse(typeof(PCM_FORMAT), s));
            }
        }

        [UcmElement("rates")]
        public string RatesString
        {
            get
            {
                var rates = Rates.Select(r => r.ToString().TrimStart('_'));
                return string.Join(", ", rates);
            }

            set
            {
                string[] substrs = value.Split(new[] { ',' });

                Rates.Clear();
                foreach (var s in substrs)
                {
                    if (char.IsDigit(s[0]))
                        s.Insert(0, "_");
                    Rates.Add((PCM_RATE)Enum.Parse(typeof(PCM_RATE), s));
                }
            }
        }

        [UcmElement("rate_min")]
        public uint? RateMin { get; set; }
        [UcmElement("rate_max")]
        public uint? RateMax { get; set; }
        [UcmElement("channels_min")]
        public uint? ChannelsMin { get; set; }
        [UcmElement("channels_max")]
        public uint? ChannelsMax { get; set; }
        [UcmElement("periods_min")]
        public uint? PeriodsMin { get; set; }
        [UcmElement("periods_max")]
        public uint? PeriodsMax { get; set; }
        [UcmElement("period_size_min")]
        public uint? PeriodSizeMin { get; set; }
        [UcmElement("period_size_max")]
        public uint? PeriodSizeMax { get; set; }
        [UcmElement("buffer_size_min")]
        public uint? BufferSizeMin { get; set; }
        [UcmElement("buffer_size_max")]
        public uint? BufferSizeMax { get; set; }
        [UcmElement("sig_bits")]
        public uint? SigBits { get; set; }

        public SectionPCMCapabilities(string identifier)
            : base(identifier)
        {
            Formats = new HashSet<PCM_FORMAT>();
            Rates = new HashSet<PCM_RATE>();
        }

        public SectionPCMCapabilities()
            : this(null)
        {
        }
    }

    public class FE_DAI : Section
    {
        [UcmElement("id")]
        public uint ID { get; set; }

        public FE_DAI(string identifier)
            : base(identifier)
        {
        }

        public FE_DAI()
            : this(null)
        {
        }
    }

    public class PCMStream : Section
    {
        [UcmElement("capabilities")]
        public string Capabilities { get; set; }

        public PCMStream(string identifier)
            : base(identifier)
        {
        }

        public PCMStream()
            : this(null)
        {
        }
    }

    public class SectionPCM : Section
    {
        [UcmElement("index")]
        public uint Index { get; set; }
        [UcmElement("id")]
        public uint ID { get; set; }

        [UcmSection("dai")]
        public FE_DAI DAI { get; set; }
        [UcmSection("pcm", "playback")]
        public PCMStream Playback { get; set; }
        [UcmSection("pcm", "capture")]
        public PCMStream Capture { get; set; }
        [UcmElement("compress")]
        public bool? Compress { get; set; }

        [UcmElement("symmetric_rates")]
        public bool? SymmetricRates { get; set; }
        [UcmElement("symmetric_channels")]
        public bool? SymmetricChannels { get; set; }
        [UcmElement("symmetric_sample_bits")]
        public bool? SymmetricSampleBits { get; set; }

        [UcmElement("data")]
        public string Data { get; set; }

        public SectionPCM(string identifier)
            : base(identifier)
        {
        }

        public SectionPCM()
            : this(null)
        {
        }
    }

    public class SectionLink : Section
    {
        [UcmElement("index")]
        public uint Index { get; set; }
        [UcmElement("id")]
        public uint ID { get; set; }

        [UcmElement("stream_name")]
        public string StreamName { get; set; }
        [UcmArray("hw_configs")]
        public string[] HwConfigs { get; set; }
        [UcmElement("default_hw_conf_id")]
        public uint DefaultHwConfId { get; set; }

        [UcmElement("symmetric_rates")]
        public bool? SymmetricRates { get; set; }
        [UcmElement("symmetric_channels")]
        public bool? SymmetricChannels { get; set; }
        [UcmElement("symmetric_sample_bits")]
        public bool? SymmetricSampleBits { get; set; }

        [UcmElement("data")]
        public string Data { get; set; }

        public SectionLink(string identifier)
            : base(identifier)
        {
        }

        public SectionLink()
            : this(null)
        {
        }
    }

    public class SectionCC : Section
    {
        [UcmElement("id")]
        public uint ID { get; set; }

        public SectionCC(string identifier)
            : base(identifier)
        {
        }

        public SectionCC()
            : this(null)
        {
        }
    }

    /// <summary>
    /// DAI physical PCM data formats.
    /// </summary>
    public enum DAI_FORMAT
    {
        [UcmEnum(Name = "I2S")] I2S          = 1, // I2S mode
        [UcmEnum(Name = "RIGHT_J")] RIGHT_J  = 2, // Right Justified mode
        [UcmEnum(Name = "LEFT_J")] LEFT_J    = 3, // Left Justified mode
        [UcmEnum(Name = "DSP_A")] DSP_A      = 4, // L data MSB after FRM LRC
        [UcmEnum(Name = "DSP_B")] DSP_B      = 5, // L data MSB during FRM LRC
        [UcmEnum(Name = "AC97")] AC97        = 6, // AC97
        [UcmEnum(Name = "PDM")] PDM          = 7  // Pulse density modulation
    }

    /// <summary>
    /// DAI topology BCLK parameter.
    /// For the backwards capability, by default codec is bclk master.
    /// </summary>
    public enum TPLG_BCLK
    {
        [UcmEnum("codec_master")] CM  = 0, // codec is bclk master
        [UcmEnum("codec_slave")] CS   = 1  // codec is bclk slave
    }

    /// <summary>
    /// DAI topology FSYNC parameter.
    /// For the backwards capability, by default codec is fsync master.
    /// </summary>
    public enum TPLG_FSYNC
    {
        [UcmEnum("codec_master")] CM  = 0, // codec is fsync master
        [UcmEnum("codec_slave")] CS   = 1  // codec is fsync slave
    }

    /// <summary>
    /// DAI mclk_direction.
    /// </summary>
    public enum TPLG_MCLK
    {
        [UcmEnum("codec_mclk_out")] CO  = 0, // for codec, mclk is output
        [UcmEnum("codec_mclk_in")] CI   = 1  // for codec, mclk is input
    }

    public class SectionHWConfig : Section
    {
        [UcmElement("id")]
        public uint ID { get; set; }
        [UcmElement("format")]
        public DAI_FORMAT? Format { get; set; }

        [UcmElement("bclk")]
        public TPLG_BCLK? Bclk { get; set; }
        [UcmElement("invert_bclk")]
        public bool? InvertBclk { get; set; }
        [UcmElement("bclk_rate")]
        public uint? BclkRate { get; set; }

        [UcmElement("fsync")]
        public TPLG_FSYNC? Fsync { get; set; }
        [UcmElement("invert_fsync")]
        public bool? InvertFsync { get; set; }
        [UcmElement("fsync_rate")]
        public uint? FsyncRate { get; set; }

        [UcmElement("mclk")]
        public TPLG_MCLK? Mclk { get; set; }
        [UcmElement("mclk_rate")]
        public uint? MclkRate { get; set; }

        [UcmElement("clock_gated")]
        public bool? ClockGated { get; set; }
        [UcmElement("tdm_slots")]
        public uint? TdmSlots { get; set; }
        [UcmElement("tdm_slot_width")]
        public uint? TdmSlotWidth { get; set; }
        [UcmElement("tx_slots")]
        public uint? TxSlots { get; set; }
        [UcmElement("rx_slots")]
        public uint? RxSlots { get; set; }
        [UcmElement("tx_channels")]
        public uint? TxChannels { get; set; }
        [UcmElement("rx_channels")]
        public uint? RxChannels { get; set; }

        public SectionHWConfig(string identifier)
            : base(identifier)
        {
        }

        public SectionHWConfig()
            : this(null)
        {
        }
    }

    public class SectionDAI : Section
    {
        [UcmElement("index")]
        public uint Index { get; set; }
        [UcmElement("id")]
        public uint ID { get; set; }

        [UcmElement("playback")]
        public uint? SupportsPlayback { get; set; }
        [UcmElement("capture")]
        public uint? SupportsCapture { get; set; }
        [UcmSection("pcm", "playback")]
        public PCMStream Playback { get; set; }
        [UcmSection("pcm", "capture")]
        public PCMStream Capture { get; set; }

        [UcmElement("symmetric_rates")]
        public bool? SymmetricRates { get; set; }
        [UcmElement("symmetric_channels")]
        public bool? SymmetricChannels { get; set; }
        [UcmElement("symmetric_sample_bits")]
        public bool? SymmetricSampleBits { get; set; }

        [UcmElement("data")]
        public string Data { get; set; }

        public SectionDAI(string identifier)
            : base(identifier)
        {
        }

        public SectionDAI()
            : this(null)
        {
        }
    }

    public class SectionManifest : Section
    {
        [UcmArray("data")]
        public string[] Data { get; set; }

        public SectionManifest(string identifier)
            : base(identifier)
        {
        }

        public SectionManifest()
            : this(null)
        {
        }
    }
}
