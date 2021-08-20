using System;
using System.Globalization;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace avstplg
{
    public class Library
    {
        public string FileName { get; set; }
        public string FwName { get; set; }
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
        public uint? CprOutAudioFormatId { get; set; }
        public uint? CprBlobFormatId { get; set; }
        public uint? CprFeatureMask { get; set; }
        public byte? CprVirtualIndex { get; set; }
        public uint? CprDMAType { get; set; }
        public uint? CprDMABufferSize { get; set; }
        public uint? MicselOutAudioFormatId { get; set; }
        public uint? IntelWOVCpcLowPowerMode { get; set; }
        public uint? SrcOutFrequency { get; set; }
        public uint? PeakvolChanId { get; set; }
        public uint? PeakvolTargetVolume { get; set; }
        public uint? PeakvolCurveType { get; set; }
        public ulong? PeakvolCurveDuration { get; set; }
        public uint? MuxRefFrequency { get; set; }
        public uint? MuxOutFrequency { get; set; }
        public uint? AecRefFrequency { get; set; }
        public uint? UpDownMixOutChanCfg { get; set; }
        public uint? UpDownMixCoeffSelect { get; set; }
        [XmlArray("UpDownMixCoeff")]
        [XmlArrayItem("int")]
        public int[] UpDownMixCoeff { get; set; }
        public uint? UpDownMixChanMap { get; set; }
    }

    public class Pipeline
    {
        [XmlAttribute("id")]
        public int Id { get; set; }
        public ushort RequiredSize { get; set; }
        public byte Priority { get; set; }
        public bool LowPower { get; set; }
        public ushort Attributes { get; set; }
        public uint Trigger { get; set; }
        public Module[] Modules;
    }

    public class Binding
    {
        public string TargetTopologyName { get; set; }
        public uint TargetPathObjId { get; set; }
        public uint TargetRouteObjId { get; set; }
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
        public bool IgnoreSuspend { get; set; }
        public Path[] Paths;
    }

    public class Condpath
    {
        [XmlAttribute("variant_id")]
        public int VariantId { get; set; }
        public uint SourceVariantId;
        public uint SinkVariantId;
        public Route[] Routes;
    }

    public class CondpathTemplate
    {
        [XmlAttribute("obj_id")]
        public int ObjectId { get; set; }
        public string SourceTopologyName { get; set; }
        public uint SourcePathObjId { get; set; }
        public string SinkTopologyName { get; set; }
        public uint SinkPathObjId { get; set; }
        public uint ConditionType { get; set; }
        public bool Overriddable { get; set; }
        public byte Priority { get; set; }
        public Condpath[] Condpaths;
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
        public bool IgnoreSuspend { get; set; }
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
        public CondpathTemplate[] CondpathTemplates;
        public FEDAI[] FEDAIs;
        public DAPMGraph[] Graphs;
    }
}
