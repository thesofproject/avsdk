//
// Copyright (c) 2018, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// This program is free software; you can redistribute it and/or modify it
// under the terms and conditions of the MIT License.
//
// This program is distributed in the hope it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE.
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace NUmcSerializer
{
    public abstract class Section
    {
        [UmcIdentifier]
        public virtual string Identifier { get; set; }
        [UmcElement("comment")]
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

    [UmcSection("ops")]
    public class Ops : Section
    {
        [UmcElement("get")]
        public uint? Get { get; set; }
        [UmcElement("put")]
        public uint? Put { get; set; }
        [UmcElement("info")]
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

    [UmcSection("channel")]
    public class ChannelMap : Section
    {
        [UmcElement("reg")]
        public int Reg { get; set; }
        [UmcElement("shift")]
        public int Shift { get; set; }

        [UmcIdentifier]
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

    [UmcSection("scale")]
    public class DBScale : Section
    {
        [UmcElement("min")]
        public int? Min { get; set; }
        [UmcElement("step")]
        public int Step { get; set; }
        [UmcElement("mute")]
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

        [UmcElement("file"), UmcExclusive("value")]
        public string File { get; set; }

        [UmcElement("bytes"), UmcExclusive("value")]
        public string BytesString
        {
            get
            {
                return (Bytes == null) ? null :
                    string.Join(", ", Bytes.Select(e => $"0x{e.ToString("X2")}"));
            }

            set
            {
                Bytes = (value != null) ? value.ToBytes() : null;
            }
        }

        [UmcElement("shorts"), UmcExclusive("value")]
        public string ShortsString
        {
            get
            {
                return (Shorts == null) ? null :
                    string.Join(", ", Shorts.Select(e => $"0x{e.ToString("X4")}"));
            }

            set
            {
                Shorts = (value != null) ? value.ToUInts16() : null;
            }
        }

        [UmcElement("words"), UmcExclusive("value")]
        public string WordsString
        {
            get
            {
                return (Words == null) ? null :
                    string.Join(", ", Words.Select(e => $"0x{e.ToString("X8")}"));
            }

            set
            {
                Words = (value != null) ? value.ToUInts32() : null;
            }
        }

        [UmcElement("tuples"), UmcExclusive("value")]
        public string Tuples { get; set; }
        [UmcElement("type")]
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

        public static int GetElementSize<T>()
        {
            Type type = typeof(T);

            if (type == typeof(string))
                return CTL_ELEM_ID_NAME_MAXLEN;
            else if (type == typeof(Guid))
                return 16 * sizeof(byte);
            return sizeof(uint);
        }

        public VendorTuples(string identifier)
            : base(identifier)
        {
        }

        public VendorTuples()
            : this(null)
        {
        }

        public abstract int Size();
    }

    [UmcSection("tuples")]
    public class VendorTuples<T> : VendorTuples
    {
        static readonly Dictionary<Type, string> tupleTypes =
            new Dictionary<Type, string>()
            {
                { typeof(string), "string" },
                { typeof(Guid), "uuid" },
                { typeof(bool), "bool" },
                { typeof(byte), "byte" },
                { typeof(ushort), "short" },
                { typeof(uint), "word" },
            };

        [UmcIgnore]
        public static string TupleType { get; }

        [UmcArray(Inline = true)]
        public Tuple<string, T>[] Tuples { get; set; }

        [UmcIdentifier]
        public override string Identifier
        {
            get
            {
                return base.Identifier;
            }

            set
            {
                base.Identifier = (value == null) ? TupleType : $"{TupleType}.{value}";
            }
        }

        static VendorTuples()
        {
            Type type = typeof(T);

            if (tupleTypes.ContainsKey(type))
                TupleType = tupleTypes[type];
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
        [UmcArray(Inline = true)]
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
        [UmcElement("tokens")]
        public string Tokens { get; set; }

        [UmcArray(Inline = true)]
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
        [UmcEnum(Name = "read")]
        READ          = (1 << 0),
        [UmcEnum(Name = "write")]
        WRITE         = (1 << 1),
        [UmcEnum(Name = "read_write")]
        READWRITE     = (READ | WRITE),
        [UmcEnum(Name = "volatile")]
        VOLATILE      = (1 << 2),   // control value may be changed without a notification
        [UmcEnum(Name = "timestamp")]
        TIMESTAMP     = (1 << 3),   // when was control changed
        [UmcEnum(Name = "tlv_read")]
        TLV_READ      = (1 << 4),   // TLV read is possible
        [UmcEnum(Name = "tlv_write")]
        TLV_WRITE     = (1 << 5),   // TLV write is possible
        [UmcEnum(Name = "tlv_read_write")]
        TLV_READWRITE = (TLV_READ | TLV_WRITE),
        [UmcEnum(Name = "tlv_command")]
        TLV_COMMAND   = (1 << 6),   // TLV command is possible
        [UmcEnum(Name = "inactive")]
        INACTIVE      = (1 << 8),   // control does actually nothing, but may be updated
        [UmcEnum(Name = "lock")]
        LOCK          = (1 << 9),   // write lock
        [UmcEnum(Name = "owner")]
        OWNER         = (1 << 10),  // write lock owner
        [UmcEnum(Name = "tlv_callback")]
        TLV_CALLBACK  = (1 << 28),  // kernel use a TLV callback
        [UmcEnum(Name = "user")]
        USER          = (1 << 29)   // user space element
    }

    public abstract class SectionControl : Section
    {
        [UmcElement("index")]
        public uint Index { get; set; }

        [UmcArray("channel", Inline = true)]
        public ChannelMap[] Channel { get; set; }
        [UmcSection("ops")]
        public Ops Ops { get; set; }

        [UmcArray("access")]
        public CTL_ELEM_ACCESS[] Access { get; set; }

        [UmcElement("data")]
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
        [UmcElement("max")]
        public int? Max { get; set; }
        [UmcElement("invert")]
        public bool Invert { get; set; }

        [UmcElement("tlv")]
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
        [UmcSection("extops")]
        public Ops ExtOps { get; set; }

        [UmcElement("base")]
        public int? Base { get; set; }
        [UmcElement("num_regs")]
        public int? NumRegs { get; set; }
        [UmcElement("mask")]
        public int? Mask { get; set; }
        [UmcElement("max")]
        public int? Max { get; set; }

        [UmcElement("tlv")]
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
        [UmcElement("texts")]
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
        [UmcArray("values")]
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
        [UmcElement("index")]
        public uint Index { get; set; }
        [UmcArray("lines")]
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
        [UmcEnum(Name = "input")] INPUT,
        [UmcEnum(Name = "output")] OUTPUT,
        [UmcEnum(Name = "mux")] MUX,
        [UmcEnum(Name = "mixer")] MIXER,
        [UmcEnum(Name = "pga")] PGA,
        [UmcEnum(Name = "out_drv")] OUT_DRV,
        [UmcEnum(Name = "adc")] ADC,
        [UmcEnum(Name = "dac")] DAC,
        [UmcEnum(Name = "switch")] SWITCH,
        [UmcEnum(Name = "pre")] PRE,
        [UmcEnum(Name = "post")] POST,
        [UmcEnum(Name = "aif_in")] AIF_IN,
        [UmcEnum(Name = "aif_out")] AIF_OUT,
        [UmcEnum(Name = "dai_in")] DAI_IN,
        [UmcEnum(Name = "dai_out")] DAI_OUT,
        [UmcEnum(Name = "dai_link")] DAI_LINK,
        [UmcEnum(Name = "buffer")] BUFFER,
        [UmcEnum(Name = "scheduler")] SCHEDULER,
        [UmcEnum(Name = "effect")] EFFECT,
        [UmcEnum(Name = "siggen")] SIGGEN,
        [UmcEnum(Name = "src")] SRC,
        [UmcEnum(Name = "asrc")] ASRC,
        [UmcEnum(Name = "encoder")] ENCODER,
        [UmcEnum(Name = "decoder")] DECODER
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
        [UmcElement("index")]
        public uint Index { get; set; }

        [UmcElement("type")]
        public TPLG_DAPM Type { get; set; }
        [UmcElement("stream_name")]
        public string StreamName { get; set; }

        [UmcElement("no_pm")]
        public bool? NoPm { get; set; }
        [UmcElement("reg")]
        public int? Reg { get; set; }
        [UmcElement("shift")]
        public int? Shift { get; set; }
        [UmcElement("invert")]
        public bool? Invert { get; set; }
        [UmcElement("subseq")]
        public uint? Subseq { get; set; }

        [UmcElement("event_type")]
        public uint? EventType { get; set; }
        [UmcElement("event_flags")]
        public DAPM_EVENT? EventFlags { get; set; }

        [UmcElement("mixer"), UmcExclusive("control")]
        public string[] Mixer { get; set; }
        [UmcElement("enum"), UmcExclusive("control")]
        public string[] Enum { get; set; }
        [UmcElement("bytes"), UmcExclusive("control")]
        public string[] Bytes { get; set; }

        [UmcElement("data")]
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

    public class SectionPCMCapabilities : Section
    {
        public ulong Formats;  // PCM_FORMAT mask

        [UmcElement("formats")]
        public string FormatsString
        {
            get
            {
                var values = Enum.GetValues(typeof(PCM_FORMAT)).Cast<int>();
                List<string> names = new List<string>();
                foreach (var value in values)
                    if ((Formats & (1uL << value)) != 0)
                        names.Add(Enum.GetName(typeof(PCM_FORMAT), value));
                return string.Join(", ", names);
            }

            set
            {
                string formats = value ?? string.Empty;
                string[] names = formats.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                Formats = 0;
                foreach (var name in names)
                {
                    int format = (int)Enum.Parse(typeof(PCM_FORMAT), name);
                    Formats += 1uL << format;
                }
            }
        }

        [UmcElement("rates")]
        public string Rates { get; set; }
        [UmcElement("rate_min")]
        public uint? RateMin { get; set; }
        [UmcElement("rate_max")]
        public uint? RateMax { get; set; }
        [UmcElement("channels_min")]
        public uint? ChannelsMin { get; set; }
        [UmcElement("channels_max")]
        public uint? ChannelsMax { get; set; }
        [UmcElement("periods_min")]
        public uint? PeriodsMin { get; set; }
        [UmcElement("periods_max")]
        public uint? PeriodsMax { get; set; }
        [UmcElement("period_size_min")]
        public uint? PeriodSizeMin { get; set; }
        [UmcElement("period_size_max")]
        public uint? PeriodSizeMax { get; set; }
        [UmcElement("buffer_size_min")]
        public uint? BufferSizeMin { get; set; }
        [UmcElement("buffer_size_max")]
        public uint? BufferSizeMax { get; set; }
        [UmcElement("sig_bits")]
        public uint? SigBits { get; set; }

        public SectionPCMCapabilities(string identifier)
            : base(identifier)
        {
        }

        public SectionPCMCapabilities()
            : this(null)
        {
        }
    }

    public class FE_DAI : Section
    {
        [UmcElement("id")]
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
        [UmcElement("capabilities")]
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
        [UmcElement("index")]
        public uint Index { get; set; }
        [UmcElement("id")]
        public uint ID { get; set; }

        [UmcSection("dai")]
        public FE_DAI DAI { get; set; }
        [UmcSection("pcm")]
        public PCMStream Playback { get; set; }
        [UmcSection("pcm")]
        public PCMStream Capture { get; set; }
        [UmcSection("compress")]
        public bool? Compress { get; set; }

        [UmcElement("symmetric_rates")]
        public bool? SymmetricRates { get; set; }
        [UmcElement("symmetric_channels")]
        public bool? SymmetricChannels { get; set; }
        [UmcElement("symmetric_sample_bits")]
        public bool? SymmetricSampleBits { get; set; }

        [UmcElement("data")]
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
        [UmcElement("index")]
        public uint Index { get; set; }
        [UmcElement("id")]
        public uint ID { get; set; }

        [UmcElement("stream_name")]
        public string StreamName { get; set; }
        [UmcArray("hw_configs")]
        public string[] HwConfigs { get; set; }
        [UmcElement("default_hw_conf_id")]
        public uint DefaultHwConfId { get; set; }

        [UmcElement("symmetric_rates")]
        public bool? SymmetricRates { get; set; }
        [UmcElement("symmetric_channels")]
        public bool? SymmetricChannels { get; set; }
        [UmcElement("symmetric_sample_bits")]
        public bool? SymmetricSampleBits { get; set; }

        [UmcElement("data")]
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
        [UmcElement("id")]
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
        [UmcEnum(Name = "I2S")] I2S          = 1, // I2S mode
        [UmcEnum(Name = "RIGHT_J")] RIGHT_J  = 2, // Right Justified mode
        [UmcEnum(Name = "LEFT_J")] LEFT_J    = 3, // Left Justified mode
        [UmcEnum(Name = "DSP_A")] DSP_A      = 4, // L data MSB after FRM LRC
        [UmcEnum(Name = "DSP_B")] DSP_B      = 5, // L data MSB during FRM LRC
        [UmcEnum(Name = "AC97")] AC97        = 6, // AC97
        [UmcEnum(Name = "PDM")] PDM          = 7  // Pulse density modulation
    }

    /// <summary>
    /// DAI topology BCLK parameter.
    /// For the backwards capability, by default codec is bclk master.
    /// </summary>
    public enum TPLG_BCLK
    {
        [UmcEnum("codec_master")] CM  = 0, // codec is bclk master
        [UmcEnum("codec_slave")] CS   = 1  // codec is bclk slave
    }

    /// <summary>
    /// DAI topology FSYNC parameter.
    /// For the backwards capability, by default codec is fsync master.
    /// </summary>
    public enum TPLG_FSYNC
    {
        [UmcEnum("codec_master")] CM  = 0, // codec is fsync master
        [UmcEnum("codec_slave")] CS   = 1  // codec is fsync slave
    }

    /// <summary>
    /// DAI mclk_direction.
    /// </summary>
    public enum TPLG_MCLK
    {
        [UmcEnum("codec_mclk_out")] CO  = 0, // for codec, mclk is output
        [UmcEnum("codec_mclk_in")] CI   = 1  // for codec, mclk is input
    }

    public class SectionHWConfig : Section
    {
        [UmcElement("id")]
        public uint ID { get; set; }
        [UmcElement("format")]
        public DAI_FORMAT? Format { get; set; }

        [UmcElement("bclk")]
        public TPLG_BCLK? Bclk { get; set; }
        [UmcElement("invert_bclk")]
        public bool? InvertBclk { get; set; }
        [UmcElement("bclk_rate")]
        public uint? BclkRate { get; set; }

        [UmcElement("fsync")]
        public TPLG_FSYNC? Fsync { get; set; }
        [UmcElement("invert_fsync")]
        public bool? InvertFsync { get; set; }
        [UmcElement("fsync_rate")]
        public uint? FsyncRate { get; set; }

        [UmcElement("mclk")]
        public TPLG_MCLK? Mclk { get; set; }
        [UmcElement("mclk_rate")]
        public uint? MclkRate { get; set; }

        [UmcElement("clock_gated")]
        public bool? ClockGated { get; set; }
        [UmcElement("tdm_slots")]
        public uint? TdmSlots { get; set; }
        [UmcElement("tdm_slot_width")]
        public uint? TdmSlotWidth { get; set; }
        [UmcElement("tx_slots")]
        public uint? TxSlots { get; set; }
        [UmcElement("rx_slots")]
        public uint? RxSlots { get; set; }
        [UmcElement("tx_channels")]
        public uint? TxChannels { get; set; }
        [UmcElement("rx_channels")]
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
        [UmcElement("index")]
        public uint Index { get; set; }
        [UmcElement("id")]
        public uint ID { get; set; }

        [UmcSection("playback")]
        public uint? SupportsPlayback { get; set; }
        [UmcSection("capture")]
        public uint? SupportsCapture { get; set; }
        [UmcSection("pcm")]
        public PCMStream Playback { get; set; }
        [UmcSection("pcm")]
        public PCMStream Capture { get; set; }

        [UmcElement("symmetric_rates")]
        public bool? SymmetricRates { get; set; }
        [UmcElement("symmetric_channels")]
        public bool? SymmetricChannels { get; set; }
        [UmcElement("symmetric_sample_bits")]
        public bool? SymmetricSampleBits { get; set; }

        [UmcElement("data")]
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
        [UmcArray("data")]
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
