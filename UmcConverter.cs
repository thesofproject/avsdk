using NUmcSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace itt
{
    public sealed class UmcConverter
    {
        readonly FirmwareInfo[] manifestData;
        readonly FirmwareConfig firmwareConfig;

        readonly ModuleType[] moduleType;

        readonly Paths paths;
        readonly PathConnectors pathConnectors;

        public UmcConverter(System system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));
            if (system.SubsystemType == null)
                throw new ArgumentNullException(nameof(system.SubsystemType));

            SubsystemType[] subsystems = system.SubsystemType;

            // Retrieve manifest and firmware config
            SubsystemType sub = subsystems.SingleOrDefault(
                e => e.ManifestData != null && e.FirmwareConfig != null);
            if (sub != null)
            {
                manifestData = sub.ManifestData;
                firmwareConfig = sub.FirmwareConfig;
            }

            // Retrieve module types. If none found, bail out
            sub = subsystems.SingleOrDefault(e => e.ModuleTypes != null);
            if (sub == null)
                return;

            moduleType = sub.ModuleTypes;

            // Retrieve paths and connectors
            sub = subsystems.SingleOrDefault(
                e => e.Paths != null && e.PathConnectors != null);
            if (sub != null)
            {
                paths = sub.Paths;
                pathConnectors = sub.PathConnectors;
            }
        }

        static Tuple<string, T> GetTuple<T>(SKL_TKN token, T value)
        {
            return Tuple.Create(token.GetName(), value);
        }

        static string GetModuleShortName(string module)
        {
            switch (module)
            {
                case "copier":
                    return "cpr";

                case "mixin":
                    return "mi";

                case "mixout":
                    return "mo";

                default:
                    return module;
            }
        }

        static string GetWidgetName(string path, string module, uint instance)
        {
            string result = $"{path} {GetModuleShortName(module)}";

            if (module.GetModuleType() != SKL_MODULE_TYPE.MIXER)
                result += $" {instance}";
            return result;
        }

        static string GetWidgetName(Path path, Module module)
        {
            return GetWidgetName(path.Name, module.Type, module.Instance);
        }

        static string GetWidgetName(Path path, FromTo endpoint)
        {
            return GetWidgetName(path.Name, endpoint.Module, endpoint.Instance);
        }

        static string GetWidgetName(Path path, InputOutput endpoint)
        {
            return GetWidgetName(path.Name, endpoint.Module, endpoint.Instance);
        }

        static string GetMixerName(string path, string module)
        {
            return $"{GetWidgetName(path, module, 0)} Switch";
        }

        static string GetParamName(Param param)
        {
            return $"{param.Name} params";
        }

        public ModuleType GetTemplate(string type)
        {
            return moduleType.SingleOrDefault(t => t.Name.Equals(type));
        }

        public IEnumerable<Section> GetAllSections()
        {
            var result = new List<Section>();
            result.Add(new SectionSkylakeTokens());

            if (manifestData != null)
                result.AddRange(GetFirmwareInfoSections());
            if (firmwareConfig != null)
                result.AddRange(GetFirmwareConfigSections());
            if (moduleType != null)
                result.AddRange(GetModuleTypeSections());

            if (paths != null && pathConnectors != null)
            {
                result.AddRange(GetPipelineSections());
                result.Add(GetGraphSection());
                result.AddRange(GetPCMSections());
            }

            result.AddRange(GetManifestSections(result));
            return result;
        }

        public IEnumerable<Section> GetFirmwareInfoSections()
        {
            var result = new List<Section>();
            var section = new SectionSkylakeTuples("lib_data");
            var tuples = new List<VendorTuples>();

            var words = new VendorTuples<uint>("lib_count");
            words.Tuples = new[] { GetTuple(SKL_TKN.U32_LIB_COUNT, (uint)manifestData.Length) };
            tuples.Add(words);

            for (int i = 0; i < manifestData.Length; i++)
            {
                var strings = new VendorTuples<string>($"lib_name_{i}");
                strings.Tuples = new[] { GetTuple(SKL_TKN.STR_LIB_NAME, manifestData[i].BinaryName) };
                tuples.Add(strings);
            }

            section.Tuples = tuples.ToArray();
            SectionVendorTuples desc = section.GetSizeDescriptor();
            result.Add(desc);
            result.Add(desc.GetPrivateData());
            result.Add(section);
            result.Add(section.GetPrivateData());

            return result;
        }

        IEnumerable<Section> GetSections(ClockControl control, int id)
        {
            var result = new List<Section>();
            AudioFormat fmt = control.AudioFormat;
            var section = new SectionSkylakeTuples($"dmactrl_cfg{id}");

            var words = new VendorTuples<uint>("u32_data");
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.U32_DMACTRL_CFG_IDX, (uint)id),
                GetTuple(SKL_TKN.U32_VBUS_ID, control.Port.GetValue()),
                GetTuple(SKL_TKN.U32_FMT_FREQ, fmt.SampleRate),
                GetTuple(SKL_TKN.U8_TIME_SLOT, 0u),
                GetTuple(SKL_TKN.U32_FMT_BIT_DEPTH, fmt.SampleContainer),
                GetTuple(SKL_TKN.U32_PIPE_DIRECTION, (uint)control.Direction),
                GetTuple(SKL_TKN.U32_FMT_CH, fmt.ChannelCount),
                GetTuple(SKL_TKN.U32_DMACTRL_CFG_SIZE, 32u),
            };

            section.Tuples = new[] { words };
            SectionVendorTuples desc = section.GetSizeDescriptor();
            result.Add(desc);
            result.Add(desc.GetPrivateData());
            result.Add(section);
            result.Add(section.GetPrivateData());

            var priv = new SectionData($"dmactrl_data{id}");
            var dmactrl = new DmactrlCtrl();
            if (control.EnableSclk && control.EnableFsclk)
            {
                dmactrl.Type = Constants.DMA_TRANSMISSION_START;
                dmactrl.Size = (uint)Marshal.SizeOf(typeof(SclkfsCfg));
                dmactrl.Sclkfs = new SclkfsCfg()
                {
                    SamplingFrequency = fmt.SampleRate,
                    BitDepth = fmt.SampleContainer,
                    ChannelMap = fmt.channelMap,
                    ChannelConfig = (uint)fmt.ChannelConfig,
                    InterleavingStyle = fmt.interleaving,
                    NumberOfChannels = (byte)fmt.ChannelCount,
                    ValidBitDepth = (byte)fmt.SampleSize,
                    SampleType = (byte)fmt.sampleType,
                };
            }
            else
            {
                dmactrl.Type = Constants.DMA_CLK_CONTROLS;
                dmactrl.Size = (uint)Marshal.SizeOf(typeof(MclkCfg));
                dmactrl.Mclk = new MclkCfg()
                {
                    Mclk = (byte)(control.EnableSclk ? 0 : 1),
                    WarmUpOver = 1,
                    KeepRunning = 1
                };
            }

            int size = (int)(2 * Marshal.SizeOf(typeof(uint)) + dmactrl.Size);            
            priv.Bytes = MarshalHelper.StructureToBytes(dmactrl);
            Array.Resize(ref priv.Bytes, size);

            desc = priv.GetSizeDescriptor(size, SKL_BLOCK_TYPE.BINARY);
            result.Add(desc);
            result.Add(desc.GetPrivateData());
            result.Add(priv);

            return result;
        }

        IEnumerable<Section> GetClockControlSections()
        {
            ClockControls controls = firmwareConfig.ClockControls;
            var result = new List<Section>();

            var ctrls = new List<ClockControl>();
            if (controls.I2SClockControls != null)
                ctrls.AddRange(controls.I2SClockControls);
            if (controls.MClockControls != null)
                ctrls.AddRange(controls.MClockControls);

            for (int i = 0; i < ctrls.Count; i++)
                result.AddRange(GetSections(ctrls[i], i));
            return result;
        }

        IEnumerable<VendorTuples> GetTuples(DMABufferConfig buffer, int id)
        {
            var words = new VendorTuples<uint>($"u32_dma_buf_index_{id}");
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.U32_DMA_IDX, (uint)id),
                GetTuple(SKL_TKN.U32_DMA_MIN_SIZE, buffer.MinSizeBytes),
                GetTuple(SKL_TKN.U32_DMA_MAX_SIZE, buffer.MaxSizeBytes)
            };

            return new[] { words };
        }

        IEnumerable<VendorTuples> GetTuples(AstateTableConfig astate, int id)
        {
            var words = new VendorTuples<uint>($"u32_astate_table_index_{id}");
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.U32_ASTATE_IDX, (uint)id),
                GetTuple(SKL_TKN.U32_ASTATE_KCPS, astate.Kcps),
                GetTuple(SKL_TKN.U32_ASTATE_CLK_SRC, astate.ClkSrc)
            };

            return new[] { words };
        }

        IEnumerable<VendorTuples> GetTuples(SchedulerConfiguration scheduler)
        {
            LowLatencySourceConfig[] configs = scheduler.LowLatencySourceConfigs;
            uint config = configs[0].DmaType << 8 | configs[0].VIndex;

            var words = new VendorTuples<uint>($"u32_sch_cfg");
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.U32_SCH_TYPE, Constants.SCHEDULER_CONFIG),
                GetTuple(SKL_TKN.U32_SCH_SIZE, sizeof(uint) * (uint)(4 + configs.Length)),
                GetTuple(SKL_TKN.U32_SCH_SYS_TICK_MUL, scheduler.SystemTickMultiplier),
                GetTuple(SKL_TKN.U32_SCH_SYS_TICK_DIV, scheduler.SystemTickDivider),
                GetTuple(SKL_TKN.U32_SCH_SYS_TICK_LL_SRC, (uint)scheduler.LowLatencyInterruptSource),
                GetTuple(SKL_TKN.U32_SCH_SYS_TICK_CFG_LEN, (uint)configs.Length), // we support only 1 anyway
                GetTuple(SKL_TKN.U32_SCH_SYS_TICK_CFG, config)
            };

            return new[] { words };
        }

        public IEnumerable<Section> GetFirmwareConfigSections()
        {
            FirmwareConfig config = firmwareConfig;
            var tuples = new List<VendorTuples>();
            VendorTuples<uint> words;

            if (config.DMABufferConfigs != null)
            {
                int length = config.DMABufferConfigs.Length;
                words = new VendorTuples<uint>("u32_dma_buf");
                words.Tuples = new[]
                {
                    GetTuple(SKL_TKN.U32_DMA_TYPE, 4u),
                    GetTuple(SKL_TKN.U32_DMA_SIZE, (uint)(length *
                            Marshal.SizeOf(typeof(DMABufferConfig))))
                };

                tuples.Add(words);
                for (int i = 0; i < Constants.DMA_BUFFER_COUNT; i++)
                {
                    DMABufferConfig buf;
                    if (i < length)
                        buf = config.DMABufferConfigs[i];
                    else
                        buf = new DMABufferConfig();
                    tuples.AddRange(GetTuples(buf, i));
                }
            }

            if (config.AstateTableConfigs != null)
            {
                AstateTableConfig[] astates = config.AstateTableConfigs;
                words = new VendorTuples<uint>("u32_astate_table");
                words.Tuples = new[]
                {
                    GetTuple(SKL_TKN.U32_ASTATE_COUNT, (uint)astates.Length)
                };

                tuples.Add(words);
                for (int i = 0; i < astates.Length; i++)
                    tuples.AddRange(GetTuples(astates[i], i));
            }

            if (config.SchedulerConfiguration != null)
                tuples.AddRange(GetTuples(config.SchedulerConfiguration));

            var result = new List<Section>();
            if (tuples.Any())
            {
                var section = new SectionSkylakeTuples("fw_cfg_data");
                section.Tuples = tuples.ToArray();
                SectionVendorTuples desc = section.GetSizeDescriptor();
                result.Add(desc);
                result.Add(desc.GetPrivateData());
                result.Add(section);
                result.Add(section.GetPrivateData());
            }

            if (config.ClockControls != null)
                result.AddRange(GetClockControlSections());
            return result;
        }

        IEnumerable<VendorTuples> GetTuples(Interface iface, int mod, int intf, int id)
        {
            string dir = (iface.Dir == PinDir.IN) ? "input" : "output";
            AudioFormat fmt = iface.AudioFormat;

            var words = new VendorTuples<uint>($"u32_mod_type_{mod}_intf_{intf}_{dir}_{id}");
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.U32_DIR_PIN_COUNT, (uint)iface.Dir),
                GetTuple(SKL_TKN.MM_U32_INTF_PIN_ID, iface.PinId),
                GetTuple(SKL_TKN.U32_FMT_CH, fmt.ChannelCount),
                GetTuple(SKL_TKN.U32_FMT_FREQ, fmt.SampleRate),
                GetTuple(SKL_TKN.U32_FMT_BIT_DEPTH, fmt.SampleContainer),
                GetTuple(SKL_TKN.U32_FMT_CH_CONFIG, (uint)fmt.ChannelConfig),
                GetTuple(SKL_TKN.U32_FMT_INTERLEAVE, fmt.interleaving),
                GetTuple(SKL_TKN.U32_FMT_SAMPLE_SIZE, fmt.SampleSize),
                GetTuple(SKL_TKN.U32_FMT_SAMPLE_TYPE, fmt.sampleType),
                GetTuple(SKL_TKN.U32_FMT_CH_MAP, fmt.channelMap),
            };

            return new[] { words };
        }

        IEnumerable<VendorTuples> GetTuples(Interfaces ifaces, int mod, int id)
        {
            var result = new List<VendorTuples>();
            var inputIfaces = ifaces.Interface.Where(intf => intf.Dir == PinDir.IN).ToArray();
            var outputIfaces = ifaces.Interface.Except(inputIfaces).ToArray();

            var words = new VendorTuples<uint>($"u32_mod_type_{mod}_intf_{id}");
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.MM_U32_FMT_ID, ifaces.IntfIdx),
                GetTuple(SKL_TKN.MM_U32_NUM_IN_FMT, (uint)inputIfaces.Length),
                GetTuple(SKL_TKN.MM_U32_NUM_OUT_FMT, (uint)outputIfaces.Length),
            };

            result.Add(words);

            for (int i = 0; i < inputIfaces.Length; i++)
                result.AddRange(GetTuples(inputIfaces[i], mod, id, i));
            for (int i = 0; i < outputIfaces.Length; i++)
                result.AddRange(GetTuples(outputIfaces[i], mod, id, i));
            return result;
        }

        IEnumerable<VendorTuples> GetTuples(OutputPinFormat format, int mod, int res, int id)
        {
            var words = new VendorTuples<uint>($"u32_mod_type_{mod}_res_{res}_output_{id}");
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.U32_DIR_PIN_COUNT, (uint)PinDir.OUT),
                GetTuple(SKL_TKN.MM_U32_RES_PIN_ID, format.PinIndex),
                GetTuple(SKL_TKN.MM_U32_PIN_BUF, format.Obs),
            };

            return new[] { words };
        }

        IEnumerable<VendorTuples> GetTuples(InputPinFormat format, int mod, int res, int id)
        {
            var words = new VendorTuples<uint>($"u32_mod_type_{mod}_res_{res}_input_{id}");
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.U32_DIR_PIN_COUNT, (uint)PinDir.IN),
                GetTuple(SKL_TKN.MM_U32_RES_PIN_ID, format.PinIndex),
                GetTuple(SKL_TKN.MM_U32_PIN_BUF, format.Ibs),
            };

            return new[] { words };
        }

        IEnumerable<VendorTuples> GetTuples(ModuleResources resources, int mod, int id)
        {
            var result = new List<VendorTuples>();
            var words = new VendorTuples<uint>($"u32_mod_type_{mod}_res_{id}");
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.MM_U32_RES_ID, (uint)id),
                GetTuple(SKL_TKN.U32_MEM_PAGES, resources.IsPages),
                GetTuple(SKL_TKN.MM_U32_CPS, resources.Cps),
                GetTuple(SKL_TKN.U32_IBS, resources.Ibs),
                GetTuple(SKL_TKN.U32_OBS, resources.Obs),
                GetTuple(SKL_TKN.MM_U32_DMA_SIZE, resources.DmaBufferSize),
                GetTuple(SKL_TKN.MM_U32_CPC, resources.Cpc),
            };

            result.Add(words);

            for (int i = 0; i < resources.InputPins.Length; i++)
                result.AddRange(GetTuples(resources.InputPins[i], mod, id, i));
            for (int i = 0; i < resources.OutputPins.Length; i++)
                result.AddRange(GetTuples(resources.OutputPins[i], mod, id, i));
            return result;
        }

        IEnumerable<VendorTuples> GetTuples(ModuleType template, int id)
        {
            var result = new List<VendorTuples>();

            var uuids = new VendorTuples<Guid>($"mod_{id}");
            uuids.Tuples = new[] { GetTuple(SKL_TKN.UUID, template.uuid) };
            result.Add(uuids);

            var bytes = new VendorTuples<byte>($"u8_mod_type_{id}");
            bytes.Tuples = new[]
            {
                GetTuple(SKL_TKN.MM_U8_MOD_IDX, (byte)id),
                GetTuple(SKL_TKN.U8_IN_PIN_TYPE, (byte)template.InputPinType),
                GetTuple(SKL_TKN.U8_OUT_PIN_TYPE, (byte)template.OutputPinType),
                GetTuple(SKL_TKN.U8_IN_QUEUE_COUNT, (byte)template.InputPins),
                GetTuple(SKL_TKN.U8_OUT_QUEUE_COUNT, (byte)template.OutputPins),
                GetTuple(SKL_TKN.MM_U8_NUM_RES, (byte)template.ModuleResourceList.Length),
                GetTuple(SKL_TKN.MM_U8_NUM_INTF, (byte)template.ModuleInterfaceList.Length),
            };

            result.Add(bytes);

            for (int i = 0; i < template.ModuleResourceList.Length; i++)
                result.AddRange(GetTuples(template.ModuleResourceList[i], id, i));
            for (int i = 0; i < template.ModuleInterfaceList.Length; i++)
                result.AddRange(GetTuples(template.ModuleInterfaceList[i], id, i));
            return result;
        }

        public IEnumerable<Section> GetModuleTypeSections()
        {
            var result = new List<Section>();
            var section = new SectionSkylakeTuples("mod_type_data");
            var tuples = new List<VendorTuples>();

            var bytes = new VendorTuples<byte>("u8_num_mod");
            bytes.Tuples = new[] { GetTuple(SKL_TKN.U8_NUM_MOD, (byte)moduleType.Length) };
            tuples.Add(bytes);

            for (int i = 0; i < moduleType.Length; i++)
                tuples.AddRange(GetTuples(moduleType[i], i));

            section.Tuples = tuples.ToArray();
            SectionVendorTuples desc = section.GetSizeDescriptor();
            result.Add(desc);
            result.Add(desc.GetPrivateData());
            result.Add(section);
            result.Add(section.GetPrivateData());

            return result;
        }

        IEnumerable<Section> GetSections(InitParam param, string moduleId)
        {
            var result = new List<Section>();

            string prefix = $"{moduleId} bin_blk_{(uint)param.SetParams}";
            var section = new SectionSkylakeTuples($"{prefix}_tkn");
            byte[] defVal = param.DefaultValue.ToBytes();

            var words = new VendorTuples<uint>("u32_data");
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.U32_FMT_CFG_IDX, (uint)param.SetParams),
                GetTuple(SKL_TKN.U32_CAPS_SIZE, (uint)defVal.Length),
                GetTuple(SKL_TKN.U32_CAPS_SET_PARAMS, (uint)param.SetParams),
                GetTuple(SKL_TKN.U32_CAPS_PARAMS_ID, param.ParamId)
            };

            section.Tuples = new[] { words };
            SectionVendorTuples desc = section.GetSizeDescriptor();
            result.Add(desc);
            result.Add(desc.GetPrivateData());
            result.Add(section);
            result.Add(section.GetPrivateData());

            var priv = new SectionData($"{prefix}_data");
            priv.Bytes = defVal;
            desc = priv.GetSizeDescriptor(defVal.Length, SKL_BLOCK_TYPE.BINARY);
            result.Add(desc);
            result.Add(desc.GetPrivateData());
            result.Add(priv);

            return result;
        }

        IEnumerable<VendorTuples> GetTuples(ModuleParams param, int id)
        {
            var shorts = new VendorTuples<ushort>($"u16_pipe_mod_cfg_{id}");
            shorts.Tuples = new[]
            {
                GetTuple(SKL_TKN.CFG_MOD_RES_ID, param.ResIdx),
                GetTuple(SKL_TKN.CFG_MOD_FMT_ID, param.IntfIdx)
            };

            return new[] { shorts };
        }

        IEnumerable<VendorTuples> GetTuples(PcmFormat format, int id)
        {
            var result = new List<VendorTuples>();
            string dir = (format.Dir == PinDir.IN) ? "in" : "out";

            var words = new VendorTuples<uint>($"_pipe_u32_cfg_{dir}_fmt_{id}");
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.U32_DIR_PIN_COUNT, (uint)id << 4 | (uint)format.Dir),
                GetTuple(SKL_TKN.U32_CFG_FREQ, format.SampleRate)
            };

            result.Add(words);
            words = new VendorTuples<uint>($"_pipe_u8_cfg_{dir}_fmt_{id}");
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.U8_CFG_BPS, format.Bps),
                GetTuple(SKL_TKN.U8_CFG_CHAN, format.ChannelCount)
            };

            result.Add(words);
            return result;
        }

        IEnumerable<VendorTuples> GetTuples(PathConfiguration config, Module module, int id)
        {
            var result = new List<VendorTuples>();
            var words = new VendorTuples<uint>($"_pipe_{id}");
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.U32_PIPE_CONFIG_ID, config.ConfigIdx),
                GetTuple(SKL_TKN.U32_PATH_MEM_PGS, config.PathResources.MemPages)
            };

            result.Add(words);
            var formats = config.PcmFormats.Where(f => f.Dir == PinDir.IN).ToArray();
            for (int i = 0; i < formats.Length; i++)
                result.AddRange(GetTuples(formats[i], i));
            formats = config.PcmFormats.Except(formats).ToArray();
            for (int i = 0; i < formats.Length; i++)
                result.AddRange(GetTuples(formats[i], i));

            ModuleParams param = config.ModuleParams.First(p => p.Module.Equals(module.Type));
            result.AddRange(GetTuples(param, id));

            return result;
        }

        IEnumerable<VendorTuples> GetTuples(PathConfigurations configs, Module module)
        {
            var result = new List<VendorTuples>();

            for (int i = 0; i < configs.PathConfiguration.Length; i++)
                result.AddRange(GetTuples(configs.PathConfiguration[i], module, i));

            return result;
        }

        static SKL_PIPE_CONN_TYPE GetConnType(Path path, Module module)
        {
            if (module.Type.Equals("copier"))
            {
                if ((path.ConnType == ConnType.HOST_DMA || path.ConnType == ConnType.HDMI_HOST_DMA)
                    && path.Direction != Direction.CAPTURE)
                    return SKL_PIPE_CONN_TYPE.FE;
                else if (path.ConnType == ConnType.LINK_DMA)
                    return SKL_PIPE_CONN_TYPE.BE;
            }

            return SKL_PIPE_CONN_TYPE.NONE;
        }

        static SKL_PIPE_CONN_TYPE GetPipeConnType(Path path, Module module)
        {
            if (module.Type.Equals("copier"))
            {
                if ((path.ConnType == ConnType.HOST_DMA || path.ConnType == ConnType.HDMI_HOST_DMA)
                    && path.Direction != Direction.CAPTURE)
                    return SKL_PIPE_CONN_TYPE.FE;
                else if (path.ConnType == ConnType.LINK_DMA)
                    return SKL_PIPE_CONN_TYPE.BE;
            }

            return path.ConnType.GetValue();
        }

        IEnumerable<VendorTuples> GetTuples(PinDir dir, uint pinCount, uint maxQueue,
            IEnumerable<Tuple<FromTo, FromTo>> pairs)
        {
            int anyCount = pairs.Count(p => p.Item1.Interface == InterfaceName.ANY);
            bool dynamic = anyCount == pairs.Count();
            if (!dynamic && anyCount > 0)
                throw new InvalidOperationException("static and dynamic pins cannot coexist");

            string str = dir.ToString().ToLower();
            var result = new List<VendorTuples>();

            for (uint i = 0; i < pinCount; i++)
            {
                uint moduleId = 0, instanceId = 0;
                Guid uuid = Guid.Empty;
                if (!dynamic)
                {
                    Tuple<FromTo, FromTo> pair = pairs.FirstOrDefault(
                        p => (uint)p.Item1.Interface % maxQueue == i);
                    if (pair != null)
                    {
                        ModuleType template = GetTemplate(pair.Item2.Module);
                        moduleId = template.ModuleId;
                        instanceId = pair.Item2.Instance;
                        uuid = template.uuid;
                    }
                }

                uint dirPinCount = (uint)dir | (((uint)dir & 0xf0 | i) << 4);
                var words = new VendorTuples<uint>($"{str}_pin_{i}");
                words.Tuples = new[]
                {
                    GetTuple(SKL_TKN.U32_DIR_PIN_COUNT, dirPinCount),
                    GetTuple(SKL_TKN.U32_PIN_MOD_ID, moduleId),
                    GetTuple(SKL_TKN.U32_PIN_INST_ID, instanceId)
                };

                result.Add(words);
                if (dynamic)
                    continue;

                var uuids = new VendorTuples<Guid>($"{str}_pin_{i}");
                uuids.Tuples = new[] { GetTuple(SKL_TKN.UUID, uuid) };
                result.Add(uuids);
            }

            return result;
        }

        IEnumerable<VendorTuples> GetTuples(Module module, Path path, PinDir dir)
        {
            ModuleType template = GetTemplate(module.Type);
            IEnumerable<Tuple<FromTo, FromTo>> pairs;
            uint pinCount, maxQueue;

            if (dir == PinDir.IN)
            {
                pinCount = template.InputPins;
                maxQueue = Constants.MAX_IN_QUEUE;

                var links = path.Links.Select(l => Tuple.Create(l.To, l.From));
                var connectors = pathConnectors.PathConnector
                    .Where(c => c.Output[0].PathName.Equals(path.Name))
                    .Select(c => Tuple.Create<FromTo, FromTo>(c.Output[0], c.Input[0]));

                pairs = links.Concat(connectors);
            }
            else
            {
                pinCount = template.OutputPins;
                maxQueue = Constants.MAX_OUT_QUEUE;

                var links = path.Links.Select(l => Tuple.Create(l.From, l.To));
                var connectors = pathConnectors.PathConnector
                    .Where(c => c.Input[0].PathName.Equals(path.Name))
                    .Select(c => Tuple.Create<FromTo, FromTo>(c.Input[0], c.Output[0]));

                pairs = links.Concat(connectors);
            }

            pairs = pairs.Where(
                p => p.Item1.Module.Equals(module.Type) &&
                     p.Item1.Instance == module.Instance);
            return GetTuples(dir, pinCount, maxQueue, pairs);
        }

        IEnumerable<Section> GetSections(Module module, Path path, int id)
        {
            ModuleType template = GetTemplate(module.Type);
            var inTuples = GetTuples(module, path, PinDir.IN);
            var outTuples = GetTuples(module, path, PinDir.OUT);

            var tuples = new List<VendorTuples>();
            var uuids = new VendorTuples<Guid>();
            uuids.Tuples = new[] { GetTuple(SKL_TKN.UUID, template.uuid) };

            tuples.Add(uuids);
            var bytes = new VendorTuples<byte>("u8_data");
            bytes.Tuples = new[]
            {
                GetTuple(SKL_TKN.U8_IN_PIN_TYPE, (byte)template.InputPinType),
                GetTuple(SKL_TKN.U8_OUT_PIN_TYPE, (byte)template.OutputPinType),
                GetTuple(SKL_TKN.U8_DYN_IN_PIN, Convert.ToByte(IsDynamic(inTuples))),
                GetTuple(SKL_TKN.U8_DYN_OUT_PIN, Convert.ToByte(IsDynamic(outTuples))),
                GetTuple(SKL_TKN.U8_TIME_SLOT, (byte)module.TdmSlot),
                GetTuple(SKL_TKN.U8_CORE_ID, (byte)module.Affinity),
                GetTuple(SKL_TKN.U8_MODULE_TYPE, (byte)module.Type.GetModuleType()),
                GetTuple(SKL_TKN.U8_CONN_TYPE, (byte)GetConnType(path, module)),
                GetTuple(SKL_TKN.U8_HW_CONN_TYPE, (byte)path.Direction.GetHwConnType()),
                GetTuple(SKL_TKN.U8_DEV_TYPE, (byte)module.DevType),
            };

            tuples.Add(bytes);
            var shorts = new VendorTuples<ushort>("u16_data");
            shorts.Tuples = new[]
            {
                GetTuple(SKL_TKN.U16_MOD_INST_ID, (ushort)module.Instance)
            };

            tuples.Add(shorts);
            var words = new VendorTuples<uint>("u32_data");
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.U32_VBUS_ID, module.Port.GetValue()),
                GetTuple(SKL_TKN.U32_PARAMS_FIXUP, module.FixupMask),
                GetTuple(SKL_TKN.U32_CONVERTER, module.ConverterMask),
                GetTuple(SKL_TKN.U32_PIPE_ID, path.Id),
                GetTuple(SKL_TKN.U32_PIPE_CONN_TYPE, (uint)GetPipeConnType(path, module)),
                GetTuple(SKL_TKN.U32_PIPE_PRIORITY, path.Priority),
                GetTuple(SKL_TKN.U32_PMODE, Convert.ToUInt32(path.LpMode)),
                GetTuple(SKL_TKN.U32_D0I3_CAPS, (uint)path.D0i3Caps),
                GetTuple(SKL_TKN.U32_PROC_DOMAIN, (uint)module.Domain),
                GetTuple(SKL_TKN.U32_PIPE_DIRECTION, (uint)path.Direction),
                GetTuple(SKL_TKN.U32_NUM_CONFIGS, (uint)path.PathConfigurations.PathConfiguration.Length),
                GetTuple(SKL_TKN.U32_DMA_BUF_SIZE, 2u),
            };

            tuples.Add(words);
            var configs = GetTuples(path.PathConfigurations, module);
            tuples.AddRange(configs);
            tuples.AddRange(inTuples);
            tuples.AddRange(outTuples);

            var result = new List<Section>();
            string moduleId = GetWidgetName(path, module);
            var section = new SectionSkylakeTuples(moduleId);
            section.Tuples = tuples.ToArray();
            SectionVendorTuples desc = section.GetSizeDescriptor();
            result.Add(desc);
            result.Add(desc.GetPrivateData());
            desc = section.GetNumDescriptor(1);
            result.Add(desc);
            result.Add(desc.GetPrivateData());
            result.Add(section);
            result.Add(section.GetPrivateData());

            InitParam[] initParams = module.InitParams;
            if (initParams == null)
                initParams = template.InitParams;
            if (initParams != null)
                foreach (var initParam in initParams)
                    result.AddRange(GetSections(initParam, moduleId));

            return result;

            bool IsDynamic(IEnumerable<VendorTuples> vts)
            {
                return vts.All(t => t.GetType() != typeof(VendorTuples<Guid>));
            }
        }

        static uint? GetEventType(Path path, Module module)
        {
            uint? result = (uint)SKL_EVENT_TYPE.PGA;
            if (module.ModulePosition == ModulePosition.SOURCE)
                if (path.Order == 0)
                    result = (uint)SKL_EVENT_TYPE.VMIXER;
                else
                    result = (uint)SKL_EVENT_TYPE.MIXER;
            return result;
        }

        static DAPM_EVENT? GetEventFlags(Path path, Module module)
        {
            if (module.ModulePosition == ModulePosition.SOURCE)
                if (path.Order == 0)
                    return Constants.VMIX;
                else
                    return Constants.MIX;
            else if (module.ModulePosition == ModulePosition.SINK &&
                    path.Order != 7 && path.ConnType != ConnType.NONE)
                return Constants.PGAL;
            return null;
        }

        static uint? GetSubseq(Path path, Module module)
        {
            bool isSource = (module.ModulePosition == ModulePosition.SOURCE);
            // FE pipeline
            if (path.ConnType == ConnType.HOST_DMA ||
                path.ConnType == ConnType.HDMI_HOST_DMA)
            {
                if (path.Order == 0)
                    return isSource ? HDA_DAPM_SUBSEQ.FE_SRC_MIX
                                    : HDA_DAPM_SUBSEQ.FE_SRC_PGA;
                else if (path.Order == 7)
                    return isSource ? HDA_DAPM_SUBSEQ.FE_SINK_MIX
                                    : HDA_DAPM_SUBSEQ.FE_SINK_PGA;
            }
            // BE pipeline
            else if (path.ConnType == ConnType.LINK_DMA)
            {
                // First pipeline
                if (path.Order == 0)
                    return isSource ? HDA_DAPM_SUBSEQ.BE_SRC_MIX
                                    : HDA_DAPM_SUBSEQ.BE_SRC_PGA;
                // Last pipeline
                else if (path.Order == 7)
                    return isSource ? HDA_DAPM_SUBSEQ.BE_SINK_MIX
                                    : HDA_DAPM_SUBSEQ.BE_SINK_PGA;
            }

            // Intermediate pipeline
            return isSource ? HDA_DAPM_SUBSEQ.INTERMEDIATE_MIX : HDA_DAPM_SUBSEQ.INTERMEDIATE_PGA;
        }

        IEnumerable<Section> GetPathModulesSections(Path path)
        {
            var result = new List<Section>();

            for (int i = 0; i < path.Modules.Module.Length; i++)
            {
                Module module = path.Modules.Module[i];
                var sections = GetSections(module, path, i);

                var widget = new SectionWidget(GetWidgetName(path, module));
                if (i == 0)
                    widget.Type = TPLG_DAPM.MIXER;
                else
                    widget.Type = TPLG_DAPM.PGA;
                widget.NoPm = true;
                widget.Data = sections.Where(s => s is SectionData)
                    .Select(s => s.Identifier).ToArray();
                widget.EventType = GetEventType(path, module);
                widget.EventFlags = GetEventFlags(path, module);
                widget.Subseq = GetSubseq(path, module);

                result.AddRange(sections);
                result.Add(widget);

                // Set widget's Bytes property if any
                Param[] prms = module.Params;
                if (prms == null)
                    prms = GetTemplate(module.Type).Params;
                if (prms != null)
                {
                    var bytes = new List<string>();
                    foreach (var param in prms)
                        bytes.Add(GetParamName(param));
                    widget.Bytes = bytes.ToArray();
                }

                // Set widget's Mixer property if any
                if (!module.Type.Equals("mixout"))
                    continue;

                IEnumerable<PathConnector> connectors =
                    pathConnectors.PathConnector.Where(c => c.Output.Any(
                        o => o.PathName.Equals(path.Name) && o.Module.Equals(module.Type)));
                if (connectors.Any())
                {
                    var mixers = new List<string>();
                    foreach (var connector in connectors)
                    {
                        InputOutput input = connector.Input.First(
                            io => io.Module.Equals("mixin"));
                        mixers.Add(GetMixerName(input.PathName, input.Module));
                    }

                    widget.Mixer = mixers.ToArray();
                }
            }

            return result;
        }

        IEnumerable<Section> GetPathConfigurationsSections(Path path)
        {
            var result = new List<Section>();
            if (path.PathConfigurations.PathConfiguration.Length <= 1)
                return result;

            var control = new SectionControlEnum($"{path.Name} pcm cfg");
            var text = new SectionText($"enum_{path.Name} pcm cfg");
            var values = new List<string>();
            var value = new StringBuilder();

            foreach (var cfg in path.PathConfigurations.PathConfiguration)
            {
                value.Clear();
                PcmFormat fmt = cfg.PcmFormats.First(f => f.Dir == PinDir.IN);
                value.Append($"IN:f{fmt.SampleRate}-c{fmt.ChannelCount}-b{fmt.Bps}");
                fmt = cfg.PcmFormats.First(f => f.Dir == PinDir.OUT);
                value.Append($" OUT:f{fmt.SampleRate}-c{fmt.ChannelCount}-b{fmt.Bps}");
                values.Add(value.ToString());
            }

            text.Values = values.ToArray();
            control.Ops = new Ops("ctl")
            {
                Get = Constants.SKL_CTL_MULTI_IO_SELECT,
                Put = Constants.SKL_CTL_MULTI_IO_SELECT,
                Info = TPLG_CTL.ENUM
            };
            control.Access = new[]
            {
                CTL_ELEM_ACCESS.READ,
                CTL_ELEM_ACCESS.WRITE,
                CTL_ELEM_ACCESS.READWRITE
            };
            control.Texts = text.Identifier;
            control.Data = control.Identifier;

            var priv = new SectionData(control.Identifier);
            priv.Bytes = new byte[] { (byte)path.Id, 0, 0, 0 };
            result.Add(control);
            result.Add(text);
            result.Add(priv);

            return result;
        }

        IEnumerable<Section> GetPathSections(Path path)
        {
            var result = new List<Section>();

            result.AddRange(GetPathModulesSections(path));
            result.AddRange(GetPathConfigurationsSections(path));

            var widget = new SectionWidget(path.Name);
            widget.Type = (path.Direction == Direction.CAPTURE) ? TPLG_DAPM.AIF_IN
                                                                : TPLG_DAPM.AIF_OUT;
            widget.NoPm = true;
            result.Add(widget);

            return result;
        }

        IEnumerable<Section> GetSections(Param param, Ops extOps)
        {
            var section = new SectionData(GetParamName(param));
            byte[] defVal = param.DefaultValue.ToBytes();

            // Round size to dwords
            int size = (int)Math.Ceiling(param.Size / 4d) * 4;
            size = Math.Max(size, defVal.Length);

            var data = new DfwAlgoData();
            data.SetParams = param.SetParams;
            data.RuntimeApplicable = param.RuntimeApplicable;
            data.ValueCacheable = param.ValueCacheable;
            data.NotificationCtrl = param.NotificationCtrl;
            data.ParamId = param.paramId;
            data.Size = (uint)size;

            byte[] bytes = MarshalHelper.StructureToBytes(data);
            int offset = bytes.Length - Marshal.SizeOf(data.Data);
            Array.Resize(ref bytes, offset + size);
            defVal.CopyTo(bytes, offset);
            section.Bytes = bytes;

            var control = new SectionControlBytes(section.Identifier);
            control.Max = size;
            control.Mask = 0;
            control.Base = 0;
            control.NumRegs = 0;
            control.Ops = new Ops("ctl") { Info = TPLG_CTL.BYTES };
            control.Access = new[]
            {
                CTL_ELEM_ACCESS.TLV_READ,
                CTL_ELEM_ACCESS.TLV_WRITE,
                CTL_ELEM_ACCESS.TLV_READWRITE,
                CTL_ELEM_ACCESS.TLV_CALLBACK
            };
            control.ExtOps = extOps;
            control.Data = section.Identifier;

            return new Section[] { control, section };
        }

        static Ops GetControlBytesExtOps(string module)
        {
            uint call;
            if (module.Equals("probe"))
                call = Constants.SKL_CTL_TLV_PROBE;
            else
                call = Constants.SKL_CTL_TLV_BYTE;

            return new Ops("ctl") { Get = call, Put = call };
        }

        IEnumerable<Section> GetBytesControls()
        {
            var result = new List<Section>();

            foreach (var path in paths.Path)
            {
                foreach (var module in path.Modules.Module)
                {
                    Param[] prms = module.Params;
                    if (prms == null)
                        prms = GetTemplate(module.Type).Params;

                    if (prms == null)
                        continue;

                    Ops ops = GetControlBytesExtOps(module.Type);
                    foreach (var param in prms)
                    {
                        string name = GetParamName(param);
                        if (result.Any(s => s.Identifier.Equals(name)))
                            continue;

                        result.AddRange(GetSections(param, ops));
                    }
                }
            }

            return result;
        }

        static SectionControlMixer GetMixerControl(string name, int min, int max,
            CTL_ELEM_ACCESS[] access, int reg, uint get, uint put, uint info)
        {
            var control = new SectionControlMixer(name);
            control.Index = 0;
            control.Invert = false;
            control.Channel = new ChannelMap[]
            {
                new ChannelMap(ChannelName.FrontLeft) { Reg = reg },
                new ChannelMap(ChannelName.FrontRight) { Reg = reg },
            };
            control.Ops = new Ops("ctl") { Get = get, Put = put, Info = info };
            control.Min = min;
            control.Max = max;
            control.NoPm = (reg == Constants.NOPM) ? true : (bool?)null;
            control.Access = access;

            return control;
        }

        IEnumerable<Section> GetMixerControls()
        {
            var result = new List<Section>();
            bool gainExists = false;

            foreach (var path in paths.Path)
            {
                foreach (var module in path.Modules.Module)
                {
                    if (module.Type.Equals("gain"))
                        gainExists = true;
                    else if (module.Type.Equals("mixin"))
                        result.Add(GetMixerControl(
                            GetMixerName(path.Name, module.Type),
                            0, 1, null,
                            Constants.NOPM,
                            TPLG_CTL.DAPM_VOLSW,
                            TPLG_CTL.DAPM_VOLSW,
                            TPLG_CTL.DAPM_VOLSW));
                }
            }

            if (gainExists)
            {
                result.Add(GetMixerControl("Ramp Duration",
                    Constants.GAIN_TC_MIN, Constants.GAIN_TC_MAX, null,
                    Constants.NOPM, Constants.SKL_CTL_RAMP_DURATION,
                    Constants.SKL_CTL_RAMP_DURATION, TPLG_CTL.VOLSW));

                result.Add(GetMixerControl("Ramp Type",
                    Constants.GAIN_RT_MIN, Constants.GAIN_RT_MAX, null,
                    Constants.NOPM, Constants.SKL_CTL_RAMP_TYPE,
                    Constants.SKL_CTL_RAMP_TYPE, TPLG_CTL.VOLSW));

                CTL_ELEM_ACCESS[] access = new[]
                {
                    CTL_ELEM_ACCESS.TLV_READ,
                    CTL_ELEM_ACCESS.READWRITE
                };

                result.Add(GetMixerControl("Volume",
                    Constants.GAIN_MIN_INDEX, Constants.GAIN_MAX_INDEX,
                    access, 0, Constants.SKL_CTL_VOLUME,
                    Constants.SKL_CTL_VOLUME, TPLG_CTL.VOLSW));
            }

            return result;
        }

        public IEnumerable<Section> GetPipelineSections()
        {
            var result = new List<Section>();
            foreach (var path in paths.Path)
                result.AddRange(GetPathSections(path));

            result.AddRange(GetMixerControls());
            result.AddRange(GetBytesControls());
            return result;
        }

        static string GetPathFirstLine(Path path)
        {
            var line = new StringBuilder();

            if (path.Direction == Direction.PLAYBACK &&
                path.Device != null)
            {
                Link first = path.Links.First();
                line.Append(GetWidgetName(path, first.From));
                line.Append($", , {path.Device}");
            }
            else if (path.Direction == Direction.CAPTURE &&
                path.Port != null)
            {
                Module source = path.Modules.Module.First(
                    m => m.ModulePosition == ModulePosition.SOURCE);
                line.Append(GetWidgetName(path, source));
                line.Append($", , {path.Port}");
            }

            return line.ToString();
        }

        static string GetPathLastLine(Path path)
        {
            var line = new StringBuilder();

            if (path.Direction == Direction.PLAYBACK &&
                path.Port != null)
            {
                Module sink = path.Modules.Module.Last(
                    m => m.ModulePosition == ModulePosition.SINK);
                line.Append($"{path.Port}, , ");
                line.Append(GetWidgetName(path, sink));
            }
            else if (path.Direction == Direction.CAPTURE &&
                path.Device != null)
            {
                Link last = path.Links.Last();
                line.Append($"{path.Device}, , ");
                line.Append(GetWidgetName(path, last.To));
            }

            return line.ToString();
        }

        static IEnumerable<string> GetPathLines(Path path)
        {
            var result = new List<string>();
            var lines = new StringBuilder();

            string line = GetPathFirstLine(path);
            if (!string.IsNullOrEmpty(line))
                result.Add(line);

            foreach (var link in path.Links)
            {
                lines.Clear();
                lines.Append(GetWidgetName(path, link.To));
                lines.Append(", , ");
                lines.Append(GetWidgetName(path, link.From));
                result.Add(lines.ToString());
            }

            line = GetPathLastLine(path);
            if (!string.IsNullOrEmpty(line))
                result.Add(line);

            return result;
        }

        IEnumerable<string> GetConnectorLines(PathConnector connector)
        {
            var result = new List<string>();
            var line = new StringBuilder();

            foreach (var input in connector.Input)
            {
                Path source = paths.Path.First(p => p.Name.Equals(input.PathName));
                foreach (var output in connector.Output)
                {
                    Path sink = paths.Path.First(p => p.Name.Equals(output.PathName));
                    Link link = sink.Links.First(l => l.From.Module.Equals(output.Module));
                    line.Clear();
                    line.Append(GetWidgetName(sink, link.From));
                    line.Append($", {GetMixerName(source.Name, input.Module)}, ");
                    line.Append(GetWidgetName(source, input));
                    result.Add(line.ToString());
                }
            }

            return result;
        }

        public SectionGraph GetGraphSection()
        {
            var graph = new SectionGraph("Pipeline 1 Graph");
            var lines = new List<string>();

            foreach (var path in paths.Path)
                lines.AddRange(GetPathLines(path));
            //
            // One could try to provide mixin-path <-> mixout-path ordering for Switches,
            // yet sorting the paths and connectors is not as easy as it seems to be.
            // There can be multiple paths matching multiple connectors with thier
            // mixouts or mixins being relevant for multiple Switches simulatnously.
            //
            // Because of this, we simply dump Switches at the end of the graph.
            //
            foreach (var connector in pathConnectors.PathConnector)
                lines.AddRange(GetConnectorLines(connector));

            graph.Lines = lines.ToArray();
            return graph;
        }

        SectionPCMCapabilities GetPCMCapabilities(Path path)
        {
            PinDir dir = (path.Direction == Direction.PLAYBACK) ? PinDir.IN
                                                                : PinDir.OUT;
            PathConfiguration[] configurations = path.PathConfigurations.PathConfiguration;
            IEnumerable<PcmFormat> formats = configurations.SelectMany(
                p => p.PcmFormats.Where(f => f.Dir == dir));

            IEnumerable<PCM_RATE> rates = formats.Select(f => f.SampleRate.ToRate()).Distinct();
            IEnumerable<uint> channels = formats.Select(f => f.ChannelCount).Distinct();
            IEnumerable<PCM_FMTBIT> bps = formats.Select(f => f.Bps.ToFmtbit()).Distinct();

            var result = new SectionPCMCapabilities(path.Device);
            result.Formats = string.Join(", ", bps);
            result.Rates = string.Join(", ", rates.Select(r => r.GetString()));
            result.ChannelsMin = channels.Min();
            result.ChannelsMax = channels.Max();

            return result;
        }

        public IEnumerable<Section> GetPCMSections()
        {
            var result = new List<Section>();
            IEnumerable<Path> fePaths = paths.Path.Where(
                p => p.Device != null && p.DaiName != null && p.DaiLinkName != null);

            if (fePaths.Count() == 0)
                return result;

            var groups = fePaths.GroupBy(p => p.DaiLinkName).ToArray();
            for (int i = 0; i < groups.Length; i++)
            {
                var group = groups[i];
                var section = new SectionPCM(group.Key);
                section.ID = 0u;
                section.DAI = new DAI(group.First().DaiName) { ID = (uint)i };

                Path path = group.FirstOrDefault(p => p.Direction == Direction.PLAYBACK);
                if (path != null)
                {
                    SectionPCMCapabilities caps = GetPCMCapabilities(path);
                    result.Add(caps);
                    section.Playback = new DAILink("playback");
                    section.Playback.Capabilities = caps.Identifier;
                }

                path = group.FirstOrDefault(p => p.Direction == Direction.CAPTURE);
                if (path != null)
                {
                    SectionPCMCapabilities caps = GetPCMCapabilities(path);
                    result.Add(caps);
                    section.Capture = new DAILink("capture");
                    section.Capture.Capabilities = caps.Identifier;
                }

                result.Add(section);
            }

            return result;
        }

        public IEnumerable<Section> GetManifestSections(IEnumerable<Section> current)
        {
            int num = 0;
            if (manifestData != null)
                num++;
            if (moduleType != null)
                num++;
            FirmwareConfig config = firmwareConfig;
            if (config != null)
            {
                if (config.DMABufferConfigs?.Length > 0 ||
                    config.AstateTableConfigs?.Length > 0 ||
                    config.SchedulerConfiguration?.LowLatencySourceConfigs?.Length > 0)
                    num++;
                if (config.ClockControls?.I2SClockControls != null)
                    num += config.ClockControls.I2SClockControls.Length * 2;
                if (config.ClockControls?.MClockControls != null)
                    num += config.ClockControls.MClockControls.Length * 2;
            }

            var result = new List<Section>();
            var manifest = new SectionManifest("manifest_data");
            SectionVendorTuples desc = manifest.GetNumDescriptor(num);
            result.Add(desc);
            result.Add(desc.GetPrivateData());
            result.Add(manifest);
            IEnumerable<Section> sections = current.Concat(result);

            var controls = sections.Where(s => s is SectionControl).Cast<SectionControl>();
            controls = controls.Where(c => c.Data != null);
            var widgets = sections.Where(s => s is SectionWidget).Cast<SectionWidget>();
            widgets = widgets.Where(w => w.Data != null);

            var privs = sections.Where(s => s is SectionData && s.Identifier != null);
            privs = privs.Where(
                p => !controls.Any(c => c.Data.Equals(p.Identifier)) &&
                     !widgets.Any(w => w.Data.Contains(p.Identifier)));

            manifest.Data = privs.Select(p => p.Identifier).ToArray();
            return result;
        }
    }
}
