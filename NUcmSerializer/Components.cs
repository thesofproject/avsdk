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
using System.Runtime.InteropServices;

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
        [UmcEnum(Name = "volsw")]
        public const uint VOLSW = 1;
        [UmcEnum(Name = "volsw_sx")]
        public const uint VOLSW_SX = 2;
        [UmcEnum(Name = "volsw_xr_sx")]
        public const uint VOLSW_XR_SX = 3;
        [UmcEnum(Name = "enum")]
        public const uint ENUM = 4;
        [UmcEnum(Name = "bytes")]
        public const uint BYTES = 5;
        [UmcEnum(Name = "enum_value")]
        public const uint ENUM_VALUE = 6;
        [UmcEnum(Name = "range")]
        public const uint RANGE = 7;
        [UmcEnum(Name = "strobe")]
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
        [UmcElement("max")]
        public int? Max { get; set; }
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
                return (Bytes != null) ? string.Join(",", Bytes) : null;
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
                return (Shorts != null) ? string.Join(",", Shorts) : null;
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
                return (Words != null) ? string.Join(",", Words) : null;
            }

            set
            {
                Words = (value != null) ? value.ToUInts32() : null;
            }
        }

        [UmcElement("tuples"), UmcExclusive("value")]
        public string Tuples { get; set; }

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
            TypeCode code = Type.GetTypeCode(type);
            switch (code)
            {
                case TypeCode.String:
                    return CTL_ELEM_ID_NAME_MAXLEN;

                case TypeCode.Object:
                    if (type == typeof(Guid))
                        return 16 * sizeof(byte);
                    else
                        return Marshal.SizeOf(type);

                case TypeCode.Boolean:
                    return sizeof(bool);

                case TypeCode.Byte:
                    return sizeof(byte);

                case TypeCode.UInt16:
                    return sizeof(ushort);

                case TypeCode.UInt32:
                    return sizeof(uint);

                default:
                    return Marshal.SizeOf(type);
            }
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
        [UmcElement("no_pm")]
        public bool? NoPm { get; set; }

        [UmcElement("max")]
        public int? Max { get; set; }
        [UmcElement("min")]
        public int? Min { get; set; }
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
        [UmcElement("min")]
        public int? Min { get; set; }
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
        public string Reg { get; set; }
        [UmcElement("shift")]
        public string Shift { get; set; }
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

    public class SectionPCMCapabilities : Section
    {
        [UmcElement("formats")]
        public string Formats { get; set; }
        [UmcElement("rates")]
        public string Rates { get; set; }
        [UmcElement("rate_min")]
        public uint? RateMin { get; set; }
        [UmcElement("rate_max")]
        public uint? RateMax { get; set; }
        [UmcElement("channel_min")]
        public uint ChannelMin { get; set; }
        [UmcElement("channel_max")]
        public uint ChannelMax { get; set; }

        public SectionPCMCapabilities(string identifier)
            : base(identifier)
        {
        }

        public SectionPCMCapabilities()
            : this(null)
        {
        }
    }

    public class PCMConfig : Section
    {
        [UmcElement("format")]
        public string Formats { get; set; }
        [UmcElement("rate")]
        public uint Rate { get; set; }
        [UmcElement("channels")]
        public byte Channels { get; set; }
        [UmcElement("tdm_slot")]
        public byte TDMSlot { get; set; }

        public PCMConfig(string identifier)
            : base(identifier)
        {
        }

        public PCMConfig()
            : this(null)
        {
        }
    }

    public class SectionPCMConfig : Section
    {
        [UmcSection("config")]
        public PCMConfig Playback { get; set; }
        [UmcSection("config")]
        public PCMConfig Capture { get; set; }

        public SectionPCMConfig(string identifier)
            : base(identifier)
        {
        }

        public SectionPCMConfig()
            : this(null)
        {
        }
    }

    public class DAI : Section
    {
        [UmcElement("id")]
        public uint ID { get; set; }

        public DAI(string identifier)
            : base(identifier)
        {
        }

        public DAI()
            : this(null)
        {
        }
    }

    public class DAILink : Section
    {
        [UmcElement("capabilities")]
        public string Capabilities { get; set; }
        [UmcArray("configs")]
        public string[] Configs { get; set; }

        public DAILink(string identifier)
            : base(identifier)
        {
        }

        public DAILink()
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
        public DAI DAI { get; set; }
        [UmcSection("pcm")]
        public DAILink Playback { get; set; }
        [UmcSection("pcm")]
        public DAILink Capture { get; set; }

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
        public string[] Configs { get; set; }
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

    public class SectionHWConfig : Section
    {
        [UmcElement("id")]
        public uint ID { get; set; }
        [UmcElement("format")]
        public string Format { get; set; }
        [UmcElement("blkc")]
        public string Bclk { get; set; }
        [UmcElement("fsync")]
        public string Fsync { get; set; }

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

        [UmcSection("pcm")]
        public DAILink Playback { get; set; }
        [UmcSection("pcm")]
        public DAILink Capture { get; set; }

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
