//
// Copyright (c) 2020-2022, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.Xml.Serialization;
using NUcmSerializer;

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

    public class IOPinFormat
    {
        public uint IObs { get; set; }
        public uint AudioFormatId { get; set; }
    }

    public class ModuleConfigExt
    {
        internal Guid uuid;
        public uint? cprDMAType;
        public uint? whmDMAType;

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
                return cprDMAType.HasValue ? string.Format("0x{0:X8}", cprDMAType) : null;
            }
            set
            {
                if (value != null)
                    cprDMAType = value.ToUInt32();
            }
        }
        public uint? CprDMABufferSize { get; set; }
        public uint? MicselOutAudioFormatId { get; set; }
        public uint? IntelWOVCpcLowPowerMode { get; set; }
        public uint? SrcOutFrequency { get; set; }
        public uint? MuxRefAudioFormatId { get; set; }
        public uint? MuxOutAudioFormatId { get; set; }
        public uint? AecRefAudioFormatId { get; set; }
        public uint? AecOutAudioFormatId { get; set; }
        public uint? AecCpcLowPowerMode { get; set; }
        public uint? UpDownMixOutChanCfg { get; set; }
        public uint? UpDownMixCoeffSelect { get; set; }
        [XmlArray("UpDownMixCoeff")]

        [XmlArrayItem("int")]
        public int[] UpDownMixCoeff { get; set; }
        public uint? UpDownMixChanMap { get; set; }
        public uint? ASrcOutFrequency { get; set; }
        public byte? ASrcMode { get; set; }
        public byte? ASrcDisableJitterBuffer { get; set; }
        [XmlArrayItem("InPinFormat")]
        public IOPinFormat[] InPinFormats;
        [XmlArrayItem("OutPinFormat")]
        public IOPinFormat[] OutPinFormats;

        public uint? WhmRefAudioFormatId { get; set; }
        public uint? WhmOutAudioFormatId { get; set; }
        public uint? WhmBlobFormatId { get; set; }
        public uint? WhmWakeTickPeriod { get; set; }
        public byte? WhmVirtualIndex { get; set; }
        public string WhmDMAType
        {
            get
            {
                return whmDMAType.HasValue ? string.Format("0x{0:X8}", whmDMAType) : null;
            }
            set
            {
                if (value != null)
                    whmDMAType = value.ToUInt32();
            }
        }
        public uint? WhmDMABufferSize { get; set; }
        public uint? PeakVolVolume { get; set; }
        public uint? PeakVolCurveType { get; set; }
        public uint? PeakVolCurveDuration { get; set; }
        public uint? CprNHLTConfigId { get; set; }
    }

    public class ModuleInitConfig
    {
        [XmlAttribute("id")]
        public uint Id { get; set; }
        public byte Param { get; set; }
        public HexBLOB Data { get; set; }
    }

    public class NHLTConfig
    {
        [XmlAttribute("id")]
        public uint Id { get; set; }
        public HexBLOB Data { get; set; }
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
        public uint? KcontrolId { get; set; }
        [XmlArrayItem("InitConfigId")]
        public uint[] InitConfigIds { get; set; }
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
        [XmlElement("VolumeMixer")]
        public Kcontrol Kcontrol { get; set; }
        [XmlElement("MuteMixer")]
        public Kcontrol MuteKcontrol { get; set; }
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
        public uint SigBits { get; set; }
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

    public enum KcontrolType : uint
    {
        Mixer = TPLG_CTL.VOLSW,
        Bytes = TPLG_CTL.BYTES,
        Enum = TPLG_CTL.ENUM,
    }

    public class Kcontrol
    {
        internal int? max;

        [XmlAttribute("id")]
        public uint Id { get; set; }
        public string Name { get; set; }
        public KcontrolType Type { get; set; }
        public string Max
        {
            get
            {
                return max.HasValue ? string.Format("0x{0:X8}", max) : null;
            }
            set
            {
                if (value != null)
                    max = value.ToInt32();
            }
        }
        public bool Invert { get; set; }
        public int NumChannels { get; set; }
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
        public ModuleInitConfig[] ModuleInitConfigs;
        public NHLTConfig[] NHLTConfigs;
        public PipelineConfig[] PipelineConfigs;
        public Binding[] Bindings;
        public PathTemplate[] PathTemplates;
        public CondpathTemplate[] CondpathTemplates;
        public FEDAI[] FEDAIs;
        public DAPMGraph[] Graphs;
        public Kcontrol[] Kcontrols;
    }
}
