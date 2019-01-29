using System;
using System.Globalization;
using System.Xml.Serialization;

namespace itt
{
    [XmlRoot("System")]
    public class System
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlElement]
        public SubsystemType[] SubsystemType { get; set; }
    }

    public class SubsystemType
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("type")]
        public string Type { get; set; }
        [XmlAttribute("version")]
        public string Version { get; set; }

        public FirmwareInfo[] ManifestData { get; set; }
        public FirmwareConfig FirmwareConfig;
        public DriverConfig DriverConfig;

        public ModuleType[] ModuleTypes { get; set; }

        public Paths Paths;
        public PathConnectors PathConnectors;
    }

    public class FeatureMask
    {
        [XmlAttribute("feature1")]
        public bool Feature1 { get; set; }
        [XmlAttribute("feature2")]
        public bool Feature2 { get; set; }
    }

    [XmlType("DriverConfig")]
    public class DriverConfig
    {
        [XmlAttribute("description")]
        public string Description { get; set; }
        [XmlElement]
        public FeatureMask[] FeatureMask;
    }

    [Serializable]
    public enum ChannelConfig
    {
        [XmlEnum(Name = "CHANNEL_CONFIG_MONO")] MONO,
        [XmlEnum(Name = "CHANNEL_CONFIG_STEREO")] STEREO,
        [XmlEnum(Name = "CHANNEL_CONFIG_2_1")] C2_1,
        [XmlEnum(Name = "CHANNEL_CONFIG_3_0")] C3_0,
        [XmlEnum(Name = "CHANNEL_CONFIG_3_1")] C3_1,
        [XmlEnum(Name = "CHANNEL_CONFIG_QUATRO")] QUATRO,
        [XmlEnum(Name = "CHANNEL_CONFIG_4_0")] C4_0,
        [XmlEnum(Name = "CHANNEL_CONFIG_5_0")] C5_0,
        [XmlEnum(Name = "CHANNEL_CONFIG_5_1")] C5_1,
        [XmlEnum(Name = "CHANNEL_CONFIG_DUAL_MONO")] DUAL_MONO,
        [XmlEnum(Name = "CHANNEL_CONFIG_I2S_DUAL_STEREO_0")] I2S_DUAL_STEREO_0,
        [XmlEnum(Name = "CHANNEL_CONFIG_I2S_DUAL_STEREO_1")] I2S_DUAL_STEREO_1,
        [XmlEnum(Name = "CHANNEL_CONFIG_4_CHANNEL")] C4_CHANNEL,
        [XmlEnum(Name = "CHANNEL_CONFIG_7_0")] C7_0,
        [XmlEnum(Name = "CHANNEL_CONFIG_7_1")] C7_1,
        // No invalid due to inconsistency between INVALID on linux kernel
        // and 7_0 and 7_1 configurations
    };

    public class AudioFormat
    {
        internal uint channelMap;
        internal uint interleaving;
        internal uint sampleType;

        [XmlAttribute("sample_rate")]
        public uint SampleRate { get; set; }
        [XmlAttribute("sample_container")]
        public uint SampleContainer { get; set; }
        [XmlAttribute("channel_count")]
        public uint ChannelCount { get; set; }
        [XmlAttribute("sample_size")]
        public uint SampleSize { get; set; }
        [XmlAttribute("channel_map")]
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
        [XmlAttribute("channel_config")]
        public ChannelConfig ChannelConfig { get; set; }
        [XmlAttribute("interleaving")]
        public string Interleaving
        {
            get
            {
                return string.Format("0x{0:X8}", interleaving);
            }
            set
            {
                interleaving = (value != null) ? value.ToUInt32() : 0;
            }
        }
        [XmlAttribute("sample_type")]
        public string SampleType
        {
            get
            {
                return string.Format("0x{0:X8}", sampleType);
            }
            set
            {
                sampleType = (value != null) ? value.ToUInt32() : 0;
            }
        }
    };

    [Serializable]
    public enum Direction : uint
    {
        [XmlEnum(Name = "playback")] PLAYBACK,
        [XmlEnum(Name = "capture")] CAPTURE,
        [XmlEnum(Name = "invalid")] INVALID = 0xFFFFFFFF
    };

    [Serializable]
    public enum Port : uint
    {
        [XmlEnum(Name = "NONE")] NONE,
        [XmlEnum(Name = "SSP0")] SSP0,
        [XmlEnum(Name = "SSP1")] SSP1,
        [XmlEnum(Name = "SSP2")] SSP2,
        [XmlEnum(Name = "SSP3")] SSP3,
        [XmlEnum(Name = "SSP4")] SSP4,
        [XmlEnum(Name = "SSP5")] SSP5,
        [XmlEnum(Name = "SSP6")] SSP6,
        [XmlEnum(Name = "HDA")] HDA, // non existant?
        [XmlEnum(Name = "PDM")] PDM,
        [XmlEnum(Name = "SDW0")] SDW0,
        [XmlEnum(Name = "SDW1")] SDW1,
        [XmlEnum(Name = "SDW2")] SDW2,
        [XmlEnum(Name = "SDW3")] SDW3,
        [XmlEnum(Name = "invalid")] INVALID = 0xFFFFFFFF
    };

    public class ClockControl
    {
        [XmlAttribute("port")]
        public Port Port { get; set; }
        [XmlAttribute("direction")]
        public Direction Direction { get; set; }
        [XmlAttribute("enable_sclk")]
        public bool EnableSclk { get; set; } // optional
        [XmlAttribute("enable_fsclk")]
        public bool EnableFsclk { get; set; } // optional
        public AudioFormat AudioFormat { get; set; }
    }

    public class ClockControls
    {
        [XmlArrayItem("I2SClockControl")]
        public ClockControl[] I2SClockControls { get; set; }
        [XmlArrayItem("MClockControl")]
        public ClockControl[] MClockControls { get; set; }
    }

    public class LowLatencySourceConfig
    {
        [XmlAttribute("lls_idx")]
        public uint LlsIdx { get; set; } // ignored
        [XmlAttribute("v_index")]
        public uint VIndex { get; set; }
        [XmlAttribute("dma_type")]
        public uint DmaType { get; set; }
    }

    [Serializable]
    public enum InterruptSource
    {
        [XmlEnum(Name = "LOW_POWER_TIMER_INTERRUPT_SOURCE")] LOW_POWER_TIMER,
        [XmlEnum(Name = "DMIC_INTERRUPT_SOURCE")] DMIC,
        [XmlEnum(Name = "DMA_GATEWAY_INTERRUPT_SOURCE")] DMA_GATEWAY,
        [XmlEnum(Name = "HIGH_DEFINITION_AUDIO_INTERRUPT_SOURCE")] HIGH_DEFINITION_AUDIO
    };

    public class SchedulerConfiguration
    {
        [XmlAttribute("system_tick_multiplier")]
        public uint SystemTickMultiplier { get; set; }
        [XmlAttribute("system_tick_divider")]
        public uint SystemTickDivider { get; set; }
        [XmlAttribute("low_latency_interrupt_source")]
        public InterruptSource LowLatencyInterruptSource { get; set; }
        public LowLatencySourceConfig[] LowLatencySourceConfigs;
    }

    public class AstateTableConfig
    {
        [XmlAttribute("kcps")]
        public uint Kcps { get; set; }
        [XmlAttribute("clk_src")]
        public uint ClkSrc { get; set; }
    }

    public struct DMABufferConfig
    {
        [XmlAttribute("min_size_bytes")]
        public uint MinSizeBytes { get; set; }
        [XmlAttribute("max_size_bytes")]
        public uint MaxSizeBytes { get; set; }
    }

    public class FirmwareConfig
    {
        [XmlAttribute("memory_reclaimed")]
        public bool MemoryReclaimed { get; set; }
        [XmlAttribute("slow_clock_frequency_hz")]
        public uint SlowClockFrequencyHz { get; set; }
        [XmlAttribute("fast_clock_frequency_hz")]
        public uint FastClockFrequencyHz { get; set; }

        public DMABufferConfig[] DMABufferConfigs;
        public AstateTableConfig[] AstateTableConfigs;
        public SchedulerConfiguration SchedulerConfiguration { get; set; }
        public ClockControls ClockControls { get; set; }
    }

    [Serializable]
    public enum BinaryType
    {
        [XmlEnum(Name = "base")] BASE,
        [XmlEnum(Name = "library")] LIBRARY
    };

    public class FirmwareInfo
    {
        [XmlAttribute("binary_name")]
        public string BinaryName { get; set; }
        [XmlAttribute("binary_type")]
        public BinaryType BinaryType { get; set; }
        [XmlAttribute("man_major_version")]
        public uint ManMajorVersion { get; set; }
        [XmlAttribute("man_minor_version")]
        public uint ManMinorVersion { get; set; }
        [XmlAttribute("man_hotfix_version")]
        public uint ManHotfixVersion { get; set; }
        [XmlAttribute("man_build_version")]
        public uint ManBuildVersion { get; set; }
        [XmlAttribute("num_modules_entries")]
        public uint NumModulesEntries { get; set; }
        [XmlAttribute("ext_major_version")]
        public uint ExtManMajorVersion { get; set; }
        [XmlAttribute("ext_minor_version")]
        public uint ExtManMinorVersion { get; set; }
        [XmlAttribute("ext_man_module_entries")]
        public uint ExtManModuleEntries { get; set; }
        [XmlAttribute("pre_load_pages")]
        public uint PreloadPages { get; set; }
    }

    [Serializable]
    public enum PinDir
    {
        [XmlEnum(Name = "in")] IN,
        [XmlEnum(Name = "out")] OUT
    };

    public class Interface
    {
        [XmlAttribute("dir")]
        public PinDir Dir { get; set; }
        [XmlAttribute("pin_id")]
        public uint PinId { get; set; }
        public AudioFormat AudioFormat { get; set; }
    }

    [XmlType("Interfaces")]
    public class Interfaces
    {
        [XmlAttribute("intf_idx")]
        public uint IntfIdx { get; set; }

        [XmlElement("Interface")]
        public Interface[] Interface { get; set; }
    }

    public class OutputPinFormat
    {
        [XmlAttribute("pin_index")]
        public uint PinIndex { get; set; }
        [XmlAttribute("obs")]
        public uint Obs { get; set; }
    }

    public class InputPinFormat
    {
        [XmlAttribute("pin_index")]
        public uint PinIndex { get; set; }
        [XmlAttribute("ibs")]
        public uint Ibs { get; set; }
    }

    public class ModuleResources
    {
        [XmlAttribute("res_idx")]
        public uint ResIdx { get; set; }
        [XmlAttribute("is_pages")]
        public uint IsPages { get; set; }
        [XmlAttribute("cps")]
        public uint Cps { get; set; }
        [XmlAttribute("ibs")]
        public uint Ibs { get; set; }
        [XmlAttribute("obs")]
        public uint Obs { get; set; }
        [XmlAttribute("dma_buffer_size")]
        public uint DmaBufferSize { get; set; }
        [XmlAttribute("cpc")]
        public uint Cpc { get; set; }
        [XmlAttribute("module_flags")]
        public uint ModuleFlags { get; set; }
        [XmlAttribute("obls")]
        public uint Obls { get; set; }

        public InputPinFormat[] InputPins;
        public OutputPinFormat[] OutputPins;
    }

    [Serializable]
    public enum IpcType
    {
        [XmlEnum(Name = "small")] SMALL,
        [XmlEnum(Name = "large")] LARGE
    };

    [Serializable]
    public enum AccessType
    {
        [XmlEnum(Name = "read")] READ = 1,
        [XmlEnum(Name = "write")] WRITE,
        [XmlEnum(Name = "read_write")] READ_WRITE
    };

    [Serializable]
    public enum SetParams
    {
        [XmlEnum(Name = "default")] DEFAULT,
        [XmlEnum(Name = "init")] INIT,
        [XmlEnum(Name = "postinit")] SET,
        [XmlEnum(Name = "postbind")] BIND
    };

    public class Param
    {
		internal uint paramId;
      
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("param_id")]
        public string ParamId
		{
			get
            {
                return string.Format("0x{0:X8}", paramId);
            }
            set
            {
                paramId = (value != null) ? value.ToUInt32() : 0;
            }
		}

        [XmlAttribute("size")]
        public uint Size { get; set; }
        [XmlAttribute("default_value")]
        public string DefaultValue { get; set; } // optional
        [XmlAttribute("set_params")]
        public SetParams SetParams { get; set; }
        [XmlAttribute("access_type")]
        public AccessType AccessType { get; set; } // ignored
        [XmlAttribute("value_cacheable")]
        public bool ValueCacheable { get; set; }
        [XmlAttribute("ipc_type")]
        public IpcType IpcType { get; set; } // not-used
        [XmlAttribute("notification_ctrl")]
        public bool NotificationCtrl { get; set; }
        [XmlAttribute("runtime_applicable")]
        public bool RuntimeApplicable { get; set; }

        public Param()
        {
            RuntimeApplicable = true;
        }
    }

    public class InitParam
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("param_id")]
        public uint ParamId { get; set; }
        [XmlAttribute("size")]
        public uint Size { get; set; }
        [XmlAttribute("default_value")]
        public string DefaultValue { get; set; }
        [XmlAttribute("set_params")]
        public SetParams SetParams { get; set; }
    }

    [Serializable]
    public enum PinType
    {
        [XmlEnum(Name = "homogeneous")] HOMOGENEOUS,
        [XmlEnum(Name = "heterogeneous")] HETEROGENEOUS
    };

    public class ModuleType
    {
        internal Guid uuid;
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("uuid")]
        public string Uuid
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

        [XmlAttribute("module_id")]
        public uint ModuleId { get; set; }
        [XmlAttribute("loadable")]
        public bool Loadable { get; set; }
        [XmlAttribute("library_name")]
        public string LibraryName { get; set; }
        [XmlAttribute("input_pin_type")]
        public PinType InputPinType { get; set; }
        [XmlAttribute("output_pin_type")]
        public PinType OutputPinType { get; set; }
        [XmlAttribute("input_pins")]
        public uint InputPins { get; set; }
        [XmlAttribute("output_pins")]
        public uint OutputPins { get; set; }
        [XmlAttribute("auto_start")]
        public bool AutoStart { get; set; }
        [XmlAttribute("instance_max_count")]
        public uint InstanceMaxCount { get; set; }
        [XmlAttribute("major_version")]
        public uint MajorVersion { get; set; }
        [XmlAttribute("minor_version")]
        public uint MinorVersion { get; set; }
        [XmlAttribute("hotfix_version")]
        public uint HotfixVersion { get; set; }
        [XmlAttribute("build_version")]
        public uint BuildVersion { get; set; }

        [XmlElement]
        public InitParam[] InitParams;
        public Param[] Params;
        public ModuleResources[] ModuleResourceList { get; set; }
        [XmlArray("ModuleInterfaceList")]
        public Interfaces[] ModuleInterfaceList { get; set; }
    }

    [Serializable]
    public enum ModulePosition
    {
        [XmlEnum(Name = "SOURCE")]
        SOURCE,
        [XmlEnum(Name = "INTERMEDIATE")]
        INTERMEDIATE,
        [XmlEnum(Name = "SINK")]
        SINK
    };

    [Serializable]
    public enum Domain
    {
        [XmlEnum(Name = "LL")]
        LL,
        [XmlEnum(Name = "DP")]
        DP,
    };

    [Serializable]
    public enum DEV_TYPE
    {
        [XmlEnum(Name = "BT")] BT,
        [XmlEnum(Name = "DMIC")] DMIC,
        [XmlEnum(Name = "I2S")] I2S,
        [XmlEnum(Name = "SLIMBUS")] SLIMBUS,
        [XmlEnum(Name = "HDA_LINK")] HDALINK,
        [XmlEnum(Name = "LINK_DMA")] LINK_DMA = HDALINK,
        [XmlEnum(Name = "HDA_HOST")] HDAHOST,
        [XmlEnum(Name = "HOST_DMA")] HOST_DMA = HDAHOST,
        [XmlEnum(Name = "NONE")] NONE,
        [XmlEnum(Name = "SDW_PCM")] SDW_PCM,
        [XmlEnum(Name = "SDW_PDM")] SDW_PDM,
    };

    public class Module
    {
        [XmlAttribute("type")]
        public string Type { get; set; }
        [XmlAttribute("description")]
        public string Description { get; set; }
        [XmlAttribute("instance")]
        public uint Instance { get; set; }
        [XmlAttribute("affinity")]
        public uint Affinity { get; set; }
        [XmlAttribute("dev_type")]
        public DEV_TYPE DevType { get; set; }
        [XmlAttribute("port")]
        public Port Port { get; set; }
        [XmlAttribute("domain")]
        public Domain Domain { get; set; }
        [XmlAttribute("converter_mask")]
        public uint ConverterMask { get; set; }
        [XmlAttribute("fixup_mask")]
        public uint FixupMask { get; set; }
        [XmlAttribute("tdm_slot")]
        public uint TdmSlot { get; set; }
        [XmlAttribute("fast_mode")]
        public bool FastMode { get; set; }
        [XmlAttribute("module_position")]
        public ModulePosition ModulePosition { get; set; }

        [XmlElement]
        public InitParam[] InitParams;
        public Param[] Params;
    }

	[XmlType("Modules")]
	public class Modules
    {
        [XmlAttribute("description")]
        public string Description { get; set; }
        [XmlElement]
        public Module[] Module { get; set; }
    }

    public enum InterfaceName
    {
        [XmlEnum(Name = "in0")] IN0,
        [XmlEnum(Name = "in1")] IN1,
        [XmlEnum(Name = "in2")] IN2,
        [XmlEnum(Name = "in3")] IN3,
        [XmlEnum(Name = "in4")] IN4,
        [XmlEnum(Name = "in5")] IN5,
        [XmlEnum(Name = "in6")] IN6,
        [XmlEnum(Name = "in7")] IN7,
        [XmlEnum(Name = "out0")] OUT0,
        [XmlEnum(Name = "out1")] OUT1,
        [XmlEnum(Name = "out2")] OUT2,
        [XmlEnum(Name = "out3")] OUT3,
        [XmlEnum(Name = "out4")] OUT4,
        [XmlEnum(Name = "out5")] OUT5,
        [XmlEnum(Name = "out6")] OUT6,
        [XmlEnum(Name = "out7")] OUT7,
        [XmlEnum(Name = "any")] ANY,
    };

    public class FromTo
    {
        [XmlAttribute("module")]
        public string Module { get; set; }
        [XmlAttribute("instance")]
        public uint Instance { get; set; }
        [XmlAttribute("interface")]
        public InterfaceName Interface { get; set; }
    }

    [Serializable]
    public enum LinkType
    {
        [XmlEnum(Name = "direct")]
        DIRECT,
        [XmlEnum(Name = "switch")]
        SWITCH,
        [XmlEnum(Name = "mixer")]
        MIXER
    };

    public class Link
    {
        [XmlAttribute("type")]
        public LinkType Type;
        public FromTo From;
        public FromTo To;
    }

    public class PathResources
    {
        [XmlAttribute("mem_pages")]
        public uint MemPages { get; set; }
    }

	public class ModuleParams
    {
        [XmlAttribute("module")]
        public string Module { get; set; }
        [XmlAttribute("instance")]
        public uint Instance { get; set; }
        [XmlAttribute("res_idx")]
		public ushort ResIdx { get; set; }
        [XmlAttribute("intf_idx")]
		public ushort IntfIdx { get; set; }
	}

	public class PcmFormat
    {
        [XmlAttribute("dir")]
        public PinDir Dir { get; set; }
        [XmlAttribute("sample_rate")]
        public uint SampleRate { get; set; }
        [XmlAttribute("channel_count")]
        public uint ChannelCount { get; set; }
        [XmlAttribute("bps")]
        public uint Bps { get; set; }
    }

	public class PathConfiguration
    {
        [XmlAttribute("config_name")]
        public string ConfigName { get; set; }
        [XmlAttribute("config_idx")]
        public uint ConfigIdx { get; set; }
        public PcmFormat[] PcmFormats { get; set; }
        [XmlElement]
        public ModuleParams[] ModuleParams { get; set; }
        public PathResources PathResources;
	}

    [XmlType("PathConfigurations")]
	public class PathConfigurations
    {
        [XmlAttribute("description")]
        public string Description { get; set; }
        [XmlElement]
        public PathConfiguration[] PathConfiguration { get; set; }
	}

    [Serializable]
    public enum D0i3Caps
    {
        [XmlEnum(Name = "D0I3_NONE")]
        D0I3_NONE,
        [XmlEnum(Name = "D0I3_STREAMING")]
        D0I3_STREAMING,
        [XmlEnum(Name = "D0I3_NON_STREAMING")]
        D0I3_NON_STREAMING
    };

    [Serializable]
    public enum Mode
    {
        [XmlEnum(Name = "epmode")]
        EPMODE,
        [XmlEnum(Name = "dspmode")]
        DSPMODE
    };

    [Serializable]
    public enum ConnType
    {
        [XmlEnum(Name = "NONE")]
        NONE,
        [XmlEnum(Name = "HOST_DMA")]
        HOST_DMA,
        [XmlEnum(Name = "HDMI_HOST_DMA")]
        HDMI_HOST_DMA,
        [XmlEnum(Name = "LINK_DMA")]
        LINK_DMA,
    };

	public class Path
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("id")]
        public uint Id { get; set; }
        [XmlAttribute("priority")]
        public uint Priority { get; set; }
        [XmlAttribute("create_priority")]
        public uint CreatePriority { get; set; }
        [XmlAttribute("order")]
        public uint Order { get; set; }
        [XmlAttribute("direction")]
        public Direction Direction { get; set; }
        [XmlAttribute("conn_type")]
        public ConnType ConnType { get; set; }
        [XmlAttribute("device")]
        public string Device { get; set; }
        [XmlAttribute("dai_name")]
        public string DaiName { get; set; }
        [XmlAttribute("dai_link_name")]
        public string DaiLinkName { get; set; }
        [XmlAttribute("port")]
        public string Port { get; set; }
        [XmlAttribute("mode")]
        public Mode Mode { get; set; }
        [XmlAttribute("lp_mode")]
        public bool LpMode { get; set; }
        [XmlAttribute("d0i3_caps")]
        public D0i3Caps D0i3Caps { get; set; }
        [XmlAttribute("sync_id")]
        public uint SyncId { get; set; }

        public PathConfigurations PathConfigurations { get; set; }
        public Link[] Links { get; set; }
        public Modules Modules { get; set; }
	}

    [XmlType("Paths")]
	public class Paths
    {
        [XmlAttribute("description")]
        public string Description { get; set; }
        [XmlElement]
        public Path[] Path { get; set; }
	}

    public class InputOutput
    {
        [XmlAttribute("path_name")]
        public string PathName { get; set; }
        [XmlAttribute("module")]
        public string Module { get; set; }
        [XmlAttribute("instance")]
        public uint Instance { get; set; }
        [XmlAttribute("interface")]
        public InterfaceName Interface { get; set; }
    }

    public class PathConnector
    {
        [XmlAttribute("type")]
        public LinkType Type;
        [XmlElement]
        public InputOutput[] Input;
        [XmlElement]
        public InputOutput[] Output;
    }

    [XmlType("PathConnectors")]
    public class PathConnectors
    {
        [XmlAttribute("description")]
        public string Description { get; set; }
        [XmlElement]
        public PathConnector[] PathConnector { get; set; }
    }
}
