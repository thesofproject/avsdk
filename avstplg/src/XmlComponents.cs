using System;
using System.Globalization;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace avstplg
{
    public class Library
    {
        [XmlAttribute("id")]
        public uint Id { get; set; }
        public string FileName { get; set; }
    }

    public class AudioFormat
    {
        internal uint channelMap;

        [XmlAttribute("id")]
        public uint Id { get; set; }
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
        public uint Id { get; set; }
        public uint Cpc { get; set; }
        public uint Ibs { get; set; }
        public uint Obs { get; set; }
        public uint Pages { get; set; }
    }

    public class ModuleConfigExt
    {
        internal Guid uuid;
        internal uint cprDMAType;

        [XmlAttribute("id")]
        public uint Id { get; set; }
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

        // module-type specific fields
        public uint? CprOutAudioFormatId { get; set; }
        public uint? CprBlobFormatId { get; set; }
        public uint? CprFeatureMask { get; set; }
        public byte? CprVirtualIndex { get; set; }
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
        public uint? CprDMABufferSize { get; set; }
        public uint? MicselOutAudioFormatId { get; set; }
        public uint? IntelWOVCpcLowPowerMode { get; set; }
        public uint? SrcOutFrequency { get; set; }
        public uint? PeakvolChanId { get; set; }
        public uint? PeakvolTargetVolume { get; set; }
        public uint? PeakvolCurveType { get; set; }
        public ulong? PeakvolCurveDuration { get; set; }
        public uint? MuxRefAudioFormatId { get; set; }
        public uint? MuxOutAudioFormatId { get; set; }
        public uint? AecRefAudioFormatId { get; set; }
        public uint? UpDownMixOutChanCfg { get; set; }
        public uint? UpDownMixCoeffSelect { get; set; }
        [XmlArray("UpDownMixCoeff")]

        [XmlArrayItem("int")]
        public int[] UpDownMixCoeff { get; set; }
        public uint? UpDownMixChanMap { get; set; }
        public uint? ASrcOutFrequency { get; set; }
        public byte? ASrcMode { get; set; }
        public byte? ASrcDisableJitterBuffer { get; set; }
    }

    public class PipelineConfig
    {
        [XmlAttribute("id")]
        public uint Id { get; set; }
        public ushort RequiredSize { get; set; }
        public byte? Priority { get; set; }
        public bool? LowPower { get; set; }
        public ushort? Attributes { get; set; }
        public uint? Trigger { get; set; }
    }

    public class Binding
    {
        [XmlAttribute("id")]
        public uint Id { get; set; }
        public string TargetTopologyName { get; set; }
        public uint TargetPathTemplateId { get; set; }
        public uint TargetPipelineId { get; set; }
        public uint TargetModuleId { get; set; }
        public byte TargetModulePin { get; set; }
        public uint ModuleId { get; set; }
        public byte ModulePin { get; set; }
        public bool IsSink { get; set; }
    }

    public class Module
    {
        [XmlAttribute("id")]
        public uint Id { get; set; }
        public uint ConfigBaseId { get; set; }
        public uint InAudioFormatId { get; set; }
        public byte? CoreId { get; set; }
        public byte? ProcessingDomain { get; set; }
        public uint ConfigExtId { get; set; }
    }

    public class Pipeline
    {
        [XmlAttribute("id")]
        public uint Id { get; set; }
        public uint ConfigId { get; set; }
        public Module[] Modules;
        [XmlElement]
        public uint[] BindingId;
    }

    public class Path
    {
        [XmlAttribute("id")]
        public uint Id { get; set; }
        public uint FEAudioFormatId;
        public uint BEAudioFormatId;
        public Pipeline[] Pipelines;
    }

    public class PathTemplate
    {
        [XmlAttribute("id")]
        public uint Id { get; set; }
        [XmlAttribute("widget_name")]
        public string WidgetName { get; set; }
        public bool IgnoreSuspend { get; set; }
        public Path[] Paths;
    }

    public class Condpath
    {
        [XmlAttribute("id")]
        public uint Id { get; set; }
        public uint SourcePathId;
        public uint SinkPathId;
        public Pipeline[] Pipelines;
    }

    public class CondpathTemplate
    {
        [XmlAttribute("id")]
        public uint Id { get; set; }
        public string SourceTopologyName { get; set; }
        public uint SourcePathTemplateId { get; set; }
        public string SinkTopologyName { get; set; }
        public uint SinkPathTemplateId { get; set; }
        public uint ConditionType { get; set; }
        public bool Overridable { get; set; }
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
        public uint Version { get; set; }
        public Library[] Libraries;
        public AudioFormat[] AudioFormats;
        public ModuleConfigBase[] ModuleConfigsBase;
        public ModuleConfigExt[] ModuleConfigsExt;
        public PipelineConfig[] PipelineConfigs;
        public Binding[] Bindings;
        public PathTemplate[] PathTemplates;
        public CondpathTemplate[] CondpathTemplates;
        public FEDAI[] FEDAIs;
        public DAPMGraph[] Graphs;
    }
}
