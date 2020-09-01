using System;
using System.Globalization;
using System.Xml.Serialization;

namespace avstplg
{
    public class Library
    {
        public string FileName { get; set; }
    }

    public class AudioFormat
    {
        internal uint channelMap;

        [XmlAttribute("id")]
        public int Id { get; set; }
        public uint SampleRate { get; set; }
        public uint BitDepth { get; set; }
        public string ChannelMap
        {
            get
            {
                return string.Format("0x{0:X8}", channelMap);
            }
            set
            {
                channelMap = (value != null) ? value.ToUInt32() : 0;
            }
        }
        public uint ChannelConfig { get; set; }
        public uint Interleaving { get; set; }
        public uint NumChannels { get; set; }
        public uint ValidBitDepth { get; set; }
        public uint SampleType { get; set; }
    }

    public class ModuleConfigBase
    {
        [XmlAttribute("id")]
        public int Id { get; set; }
        public uint Cpc { get; set; }
        public uint Ibs { get; set; }
        public uint Obs { get; set; }
        public uint Pages { get; set; }
        public uint AudioFormatId { get; set; }
    }

    public class Module
    {
        internal Guid uuid;
        public string UUID
        {
            get
            {
                return uuid.ToString();
            }

            set
            {
                try
                {
                    uuid = new Guid(value);
                }
                catch { }
            }
        }

        [XmlAttribute("obj_id")]
        public int ObjectId { get; set; }
        public uint ModuleConfigBaseId { get; set; }
        public byte CoreId { get; set; }
        public byte ProcessingDomain { get; set; }

        // module-type specific fields
        internal uint cprDMAType;
        internal uint cprDMABufferSize;

        public uint? CprOutAudioFormatId { get; set; }
        public uint? CprFeatureMask { get; set; }
        public string CprDMAType
        {
            get
            {
                return string.Format("0x{0:X8}", cprDMAType);
            }
            set
            {
                cprDMAType = (value != null) ? value.ToUInt32() : 0;
            }
        }
        public string CprDMABufferSize
        {
            get
            {
                return string.Format("0x{0:X8}", cprDMABufferSize);
            }
            set
            {
                cprDMABufferSize = (value != null) ? value.ToUInt32() : 0;
            }
        }
        public uint? MicselOutAudioFormatId { get; set; }
        public uint? IntelWOVCpcLowPowerMode { get; set; }
        public uint? SrcOutFrequency { get; set; }
    }

    public class Pipeline
    {
        [XmlAttribute("id")]
        public int Id { get; set; }
        public ushort RequiredSize { get; set; }
        public byte Priority { get; set; }
        public bool LowPower { get; set; }
        public Module[] Modules;
    }

    public class Binding
    {
        public string TargetTopologyName { get; set; }
        public uint TargetPathObjId { get; set; }
        public uint TargetPipeObjId { get; set; }
        public uint TargetModuleObjId { get; set; }
        public byte TargetModulePin { get; set; }
        public uint ModuleObjId { get; set; }
        public byte ModulePin { get; set; }
        public bool IsSink { get; set; }
    }

    public class Route
    {
        [XmlAttribute("obj_id")]
        public int ObjectId { get; set; }
        public uint ImplementingPipelineId { get; set; }
        public Binding[] Bindings;
    }

    public class Path
    {
        internal uint feAudioFormatId;
        internal uint beAudioFormatId;

        [XmlAttribute("variant_id")]
        public int VariantId { get; set; }
        public string FEAudioFormatId
        {
            get
            {
                return string.Format("0x{0:X8}", feAudioFormatId);
            }
            set
            {
                feAudioFormatId = (value != null) ? value.ToUInt32() : 0;
            }
        }
        public string BEAudioFormatId
        {
            get
            {
                return string.Format("0x{0:X8}", beAudioFormatId);
            }
            set
            {
                beAudioFormatId = (value != null) ? value.ToUInt32() : 0;
            }
        }
        public Route[] Routes;
    }

    public class PathTemplate
    {
        [XmlAttribute("obj_id")]
        public int ObjectId { get; set; }
        [XmlAttribute("widget_name")]
        public string WidgetName { get; set; }
        public string DAIStreamName { get; set; }
        public Path[] Paths;
    }

    public class PCMCapabilities
    {
        public string Formats { get; set; }
        public string Rates { get; set; }
        public string Channels { get; set; }
    }

    public class FEDAI
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        public PCMCapabilities CaptureCapabilities;
        public PCMCapabilities PlaybackCapabilities;
    }

    public class DAPMRoute
    {
        [XmlAttribute("sink")]
        public string Sink { get; set; }
        [XmlAttribute("control")]
        public string Control { get; set; }
        [XmlAttribute("source")]
        public string Source { get; set; }
    }

    [XmlType("Graph")]
    public class DAPMGraph
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlArrayItem("Route")]
        public DAPMRoute[] Routes;
    }

    [XmlRoot]
    public class Topology
    {
        public string Name { get; set; }
        public Library[] Libraries;
        public AudioFormat[] AudioFormats;
        public ModuleConfigBase[] ModuleConfigsBase;
        public Pipeline[] Pipelines;
        public PathTemplate[] PathTemplates;
        public FEDAI[] FEDAIs;
        public DAPMGraph[] Graphs;
    }
}
