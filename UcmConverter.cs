//
// Copyright (c) 2018-2022, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using NUcmSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace itt
{
    public static class UcmConverter
    {
        static Tuple<string, T> GetTuple<T>(SKL_TKN token, T value)
        {
            return Tuple.Create(token.GetName(), value);
        }

        static string GetModuleShortName(string module)
        {
            switch (module)
            {
                case ModuleNames.Copier:
                    return "cpr";

                case ModuleNames.Mixin:
                    return "mi";

                case ModuleNames.Mixout:
                    return "mo";

                default:
                    return module;
            }
        }

        static string GetWidgetName(string path, string module, uint instance)
        {
            SKL_MODULE_TYPE type = module.GetModuleType();
            string result;

            switch (type)
            {
                case SKL_MODULE_TYPE.NONE:
                    result = $"{path}";
                    break;
                case SKL_MODULE_TYPE.MIXER:
                    result = $"{path} {GetModuleShortName(module)}";
                    break;
                default:
                    result = $"{path} {GetModuleShortName(module)} {instance}";
		    break;
            }

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

        static string GetWidgetName(InputOutput endpoint)
        {
            return GetWidgetName(endpoint.PathName, endpoint.Module, endpoint.Instance);
        }

        static string GetMixerName(string path, string module)
        {
            return $"{GetWidgetName(path, module, 0)} Switch";
        }

        static string GetParamName(Param param)
        {
            return $"{param.Name} params";
        }

        public static uint GetDirPinCount(PinDir dir, int index)
        {
                // The same token is used for both direction of pin and
                // pin count value. First 4 bits denote direction with the
                // rest storing the count (index).
                return ((uint)dir & 0xF) | ((uint)index << 4);
        }

        static ModuleType GetTemplate(ModuleType[] templates, string type)
        {
            return templates.SingleOrDefault(t => t.Name.Equals(type));
        }

        public static IEnumerable<Section> GetTopologySections(System topology)
        {
            if (topology == null)
                throw new ArgumentNullException(nameof(topology));

            var result = new List<Section>();
            result.Add(new SectionSkylakeTokens());

            FirmwareInfo[] manifestData = topology.GetManifestData();
            FirmwareConfig firmwareConfig = topology.GetFirmwareConfig();

            result.AddRange(GetFirmwareInfosSections(manifestData));
            result.AddRange(GetFirmwareConfigSections(firmwareConfig));

            ModuleType[] templates = topology.GetModuleTypes();
            /* Nothing else to do if module types aren't there */
            if (templates == null)
                goto exit;

            result.AddRange(GetModuleTypesSections(templates));

            Path[] paths = topology.GetPaths()?.Path;
            PathConnector[] connectors = topology.GetPathConnectors()?.PathConnector;
            if (paths == null || connectors == null)
                goto exit;

            result.AddRange(GetPathsSections(templates, connectors, paths));
            result.AddRange(GetPathConnectorsSections(connectors));
            result.Add(GetGraphSection(paths, connectors));
            result.AddRange(GetPCMSections(paths));
        exit:
            result.AddRange(GetManifestSections(topology, result));
            return result;
        }

        static VendorTuples GetFirmwareInfoTuples(FirmwareInfo info, int id)
        {
            var strings = new VendorTuples<string>($"lib_name_{id}");
            strings.Tuples = new[]
            {
                GetTuple(SKL_TKN.STR_LIB_NAME, info.BinaryName)
            };
            return strings;
        }

        public static IEnumerable<Section> GetFirmwareInfosSections(FirmwareInfo[] infos)
        {
            var result = new List<Section>();
            if (infos == null)
                return result;
            var section = new SectionSkylakeTuples("lib_data");
            var tuples = new List<VendorTuples>();

            var words = new VendorTuples<uint>("lib_count");
            words.Tuples = new[] { GetTuple(SKL_TKN.U32_LIB_COUNT, (uint)infos.Length) };
            tuples.Add(words);

            for (int i = 0; i < infos.Length; i++)
                tuples.Add(GetFirmwareInfoTuples(infos[i], i));

            section.Tuples = tuples.ToArray();
            SectionVendorTuples desc = section.GetSizeDescriptor();
            result.Add(desc);
            result.Add(desc.GetPrivateData());
            result.Add(section);
            result.Add(section.GetPrivateData());

            return result;
        }

        static IEnumerable<Section> GetClockControlSections(ClockControl control, int id)
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
            var dmactrl = new DMACtrlCfg();
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

        static IEnumerable<Section> GetClockControlsSections(ClockControls controls)
        {
            var result = new List<Section>();

            var ctrls = new List<ClockControl>();
            if (controls.I2SClockControls != null)
                ctrls.AddRange(controls.I2SClockControls);
            if (controls.MClockControls != null)
                ctrls.AddRange(controls.MClockControls);

            for (int i = 0; i < ctrls.Count; i++)
                result.AddRange(GetClockControlSections(ctrls[i], i));
            return result;
        }

        static IEnumerable<VendorTuples> GetDMABufferConfigTuples(DMABufferConfig buffer, int id)
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

        static IEnumerable<VendorTuples> GetAstateTableConfigTuples(AstateTableConfig astate, int id)
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

        static IEnumerable<VendorTuples> GetSchedulerConfigurationTuples(SchedulerConfiguration scheduler)
        {
            LowLatencySourceConfig[] configs = scheduler.LowLatencySourceConfigs;
            uint config = configs[0].DmaType << 8 | configs[0].VIndex;

            var words = new VendorTuples<uint>($"u32_sch_cfg");
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.U32_SCH_TYPE, Constants.SCHEDULER_CONFIG),
                // 4 DWORDs precede flex array, see firmware's struct SchedulerConfig
                GetTuple(SKL_TKN.U32_SCH_SIZE, sizeof(uint) * (uint)(4 + configs.Length)),
                GetTuple(SKL_TKN.U32_SCH_SYS_TICK_MUL, scheduler.SystemTickMultiplier),
                GetTuple(SKL_TKN.U32_SCH_SYS_TICK_DIV, scheduler.SystemTickDivider),
                GetTuple(SKL_TKN.U32_SCH_SYS_TICK_LL_SRC, (uint)scheduler.LowLatencyInterruptSource),
                GetTuple(SKL_TKN.U32_SCH_SYS_TICK_CFG_LEN, (uint)configs.Length), // we support only 1 anyway
                GetTuple(SKL_TKN.U32_SCH_SYS_TICK_CFG, config)
            };

            return new[] { words };
        }

        public static IEnumerable<Section> GetFirmwareConfigSections(FirmwareConfig config)
        {
            var result = new List<Section>();
            if (config == null)
                return result;

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
                for (int i = 0; i < Math.Min(Constants.DMA_BUFFER_COUNT, length); i++)
                    tuples.AddRange(GetDMABufferConfigTuples(config.DMABufferConfigs[i], i));
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
                    tuples.AddRange(GetAstateTableConfigTuples(astates[i], i));
            }

            if (config.SchedulerConfiguration != null)
                tuples.AddRange(GetSchedulerConfigurationTuples(config.SchedulerConfiguration));

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
                result.AddRange(GetClockControlsSections(config.ClockControls));
            return result;
        }

        static IEnumerable<VendorTuples> GetInterfaceTuples(Interface iface, int mod, int intf, int id)
        {
            string dir = (iface.Dir == PinDir.IN) ? "input" : "output";
            AudioFormat fmt = iface.AudioFormat;

            var words = new VendorTuples<uint>($"u32_mod_type_{mod}_intf_{intf}_{dir}_{id}");
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.U32_DIR_PIN_COUNT, GetDirPinCount(iface.Dir, id)),
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

        static IEnumerable<VendorTuples> GetInterfacesTuples(Interfaces ifaces, int mod, int id)
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
                result.AddRange(GetInterfaceTuples(inputIfaces[i], mod, id, i));
            for (int i = 0; i < outputIfaces.Length; i++)
                result.AddRange(GetInterfaceTuples(outputIfaces[i], mod, id, i));
            return result;
        }

        static IEnumerable<VendorTuples> GetOutputPinFormatTuples(OutputPinFormat format, int mod, int res, int id)
        {
            var words = new VendorTuples<uint>($"u32_mod_type_{mod}_res_{res}_output_{id}");
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.U32_DIR_PIN_COUNT, GetDirPinCount(PinDir.OUT, id)),
                GetTuple(SKL_TKN.MM_U32_RES_PIN_ID, format.PinIndex),
                GetTuple(SKL_TKN.MM_U32_PIN_BUF, format.Obs),
            };

            return new[] { words };
        }

        static IEnumerable<VendorTuples> GetInputPinFormatTuples(InputPinFormat format, int mod, int res, int id)
        {
            var words = new VendorTuples<uint>($"u32_mod_type_{mod}_res_{res}_input_{id}");
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.U32_DIR_PIN_COUNT, GetDirPinCount(PinDir.IN, id)),
                GetTuple(SKL_TKN.MM_U32_RES_PIN_ID, format.PinIndex),
                GetTuple(SKL_TKN.MM_U32_PIN_BUF, format.Ibs),
            };

            return new[] { words };
        }

        static IEnumerable<VendorTuples> GetModuleResourcesTuples(ModuleResources resources, int mod, int id)
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
                result.AddRange(GetInputPinFormatTuples(resources.InputPins[i], mod, id, i));
            for (int i = 0; i < resources.OutputPins.Length; i++)
                result.AddRange(GetOutputPinFormatTuples(resources.OutputPins[i], mod, id, i));
            return result;
        }

        static IEnumerable<VendorTuples> GetModuleTypeTuples(ModuleType template, int id)
        {
            var result = new List<VendorTuples>();

            var uuids = new VendorTuples<Guid>($"mod_{id}");
            uuids.Tuples = new[] { GetTuple(SKL_TKN.UUID, template.Uuid) };
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
                result.AddRange(GetModuleResourcesTuples(template.ModuleResourceList[i], id, i));
            for (int i = 0; i < template.ModuleInterfaceList.Length; i++)
                result.AddRange(GetInterfacesTuples(template.ModuleInterfaceList[i], id, i));
            return result;
        }

        public static IEnumerable<Section> GetModuleTypesSections(ModuleType[] templates)
        {
            var result = new List<Section>();
            if (templates == null)
                return result;
            var section = new SectionSkylakeTuples("mod_type_data");
            var tuples = new List<VendorTuples>();

            var bytes = new VendorTuples<byte>("u8_num_mod");
            bytes.Tuples = new[] { GetTuple(SKL_TKN.U8_NUM_MOD, (byte)templates.Length) };
            tuples.Add(bytes);

            for (int i = 0; i < templates.Length; i++)
                tuples.AddRange(GetModuleTypeTuples(templates[i], i));

            section.Tuples = tuples.ToArray();
            SectionVendorTuples desc = section.GetSizeDescriptor();
            result.Add(desc);
            result.Add(desc.GetPrivateData());
            result.Add(section);
            result.Add(section.GetPrivateData());

            return result;
        }

        static IEnumerable<Section> GetInitParamSections(InitParam param, string moduleId)
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

        static IEnumerable<VendorTuples> GetModuleParamsTuples(ModuleParams param, int id)
        {
            var shorts = new VendorTuples<ushort>($"u16_pipe_mod_cfg_{id}");
            shorts.Tuples = new[]
            {
                GetTuple(SKL_TKN.CFG_MOD_RES_ID, param.ResIdx),
                GetTuple(SKL_TKN.CFG_MOD_FMT_ID, param.IntfIdx)
            };

            return new[] { shorts };
        }

        static IEnumerable<VendorTuples> GetPcmFormatTuples(PcmFormat format, int id)
        {
            var result = new List<VendorTuples>();
            string dir = (format.Dir == PinDir.IN) ? "in" : "out";

            var words = new VendorTuples<uint>($"_pipe_u32_cfg_{dir}_fmt_{id}");
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.U32_DIR_PIN_COUNT, GetDirPinCount(format.Dir, id)),
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

        static IEnumerable<VendorTuples> GetPathConfigurationTuples(PathConfiguration config, Module module, int id)
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
                result.AddRange(GetPcmFormatTuples(formats[i], id));
            formats = config.PcmFormats.Except(formats).ToArray();
            for (int i = 0; i < formats.Length; i++)
                result.AddRange(GetPcmFormatTuples(formats[i], id));

            ModuleParams param = config.ModuleParams.FirstOrDefault(
                p => p.Module.Equals(module.Type) && p.Instance == module.Instance);
            if (param != null)
                result.AddRange(GetModuleParamsTuples(param, id));

            return result;
        }

        static IEnumerable<VendorTuples> GetPathConfigurationsTuples(PathConfigurations configs, Module module)
        {
            var result = new List<VendorTuples>();

            for (int i = 0; i < configs.PathConfiguration.Length; i++)
                result.AddRange(GetPathConfigurationTuples(configs.PathConfiguration[i], module, i));

            return result;
        }

        static SKL_PIPE_CONN_TYPE GetConnType(Path path, Module module)
        {
            if (module.Type.Equals(ModuleNames.Copier))
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
            if (module.Type.Equals(ModuleNames.Copier))
            {
                // Capture paths are expected to be always connected directly,
		// and without any kcontrol switches involved.
                if ((path.ConnType == ConnType.HOST_DMA || path.ConnType == ConnType.HDMI_HOST_DMA)
                    && path.Direction != Direction.CAPTURE)
                    return SKL_PIPE_CONN_TYPE.FE;
                else if (path.ConnType == ConnType.LINK_DMA)
                    return SKL_PIPE_CONN_TYPE.BE;
            }

            return path.ConnType.GetValue();
        }

        static IEnumerable<VendorTuples> GetPinDirTuples(ModuleType[] templates, PinDir dir, uint pinCount,
            IEnumerable<Tuple<FromTo, FromTo>> pairs)
        {
            int anyCount = pairs.Count(p => p.Item1.Interface == InterfaceName.ANY);
            bool dynamic = anyCount == pairs.Count();
            if (!dynamic && anyCount > 0)
                throw new InvalidOperationException("static and dynamic pins cannot coexist");

            string str = dir.ToString().ToLower();
            var result = new List<VendorTuples>();

            for (int i = 0; i < pinCount; i++)
            {
                uint moduleId = 0, instanceId = 0;
                Guid uuid = Guid.Empty;
                if (!dynamic)
                {
                    Tuple<FromTo, FromTo> pair = pairs.FirstOrDefault(
                        p => (uint)p.Item1.Interface % Constants.MAX_QUEUE == i);
                    if (pair != null)
                    {
                        ModuleType template = GetTemplate(templates, pair.Item2.Module);
                        moduleId = template.ModuleId;
                        instanceId = pair.Item2.Instance;
                        uuid = template.Uuid;
                    }
                }

                var words = new VendorTuples<uint>($"{str}_pin_{i}");
                words.Tuples = new[]
                {
                    GetTuple(SKL_TKN.U32_DIR_PIN_COUNT, GetDirPinCount(dir, i)),
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

        static IEnumerable<VendorTuples> GetModuleTuples(ModuleType[] templates, PathConnector[] connectors, Module module, Path path, PinDir dir)
        {
            ModuleType template = GetTemplate(templates, module.Type);
            IEnumerable<Tuple<FromTo, FromTo>> pairs;
            uint pinCount;

            if (dir == PinDir.IN)
            {
                pinCount = template.InputPins;

                pairs = path.Links.Select(l => Tuple.Create(l.To, l.From));
                if (connectors != null)
                {
                    var query = connectors
                        .Where(c => c.Output[0].PathName.Equals(path.Name))
                        .Select(c => Tuple.Create<FromTo, FromTo>(c.Output[0], c.Input[0]));

                    pairs = pairs.Concat(query);
                }
            }
            else
            {
                pinCount = template.OutputPins;

                pairs = path.Links.Select(l => Tuple.Create(l.From, l.To));
                if (connectors != null)
                {
                    var query = connectors
                        .Where(c => c.Input[0].PathName.Equals(path.Name))
                        .Select(c => Tuple.Create<FromTo, FromTo>(c.Input[0], c.Output[0]));

                    pairs = pairs.Concat(query);
                }
            }

            pairs = pairs.Where(
                p => p.Item1.Module.Equals(module.Type) &&
                     p.Item1.Instance == module.Instance);
            return GetPinDirTuples(templates, dir, pinCount, pairs);
        }

        static IEnumerable<Section> GetModuleSections(ModuleType[] templates, PathConnector[] connectors, Module module, Path path)
        {
            ModuleType template = GetTemplate(templates, module.Type);
            var inTuples = GetModuleTuples(templates, connectors, module, path, PinDir.IN);
            var outTuples = GetModuleTuples(templates, connectors, module, path, PinDir.OUT);

            var tuples = new List<VendorTuples>();
            var uuids = new VendorTuples<Guid>();
            uuids.Tuples = new[] { GetTuple(SKL_TKN.UUID, template.Uuid) };

            tuples.Add(uuids);
            var bytes = new VendorTuples<byte>("u8_data");
            // Pin configuration is considered dynamic if no explicit connections (Guids) are set
            Func<IEnumerable<VendorTuples>, bool> isDynamic = (vts) =>
            {
                return vts.All(t => t.GetType() != typeof(VendorTuples<Guid>));
            };

            bytes.Tuples = new[]
            {
                GetTuple(SKL_TKN.U8_IN_PIN_TYPE, (byte)template.InputPinType),
                GetTuple(SKL_TKN.U8_OUT_PIN_TYPE, (byte)template.OutputPinType),
                GetTuple(SKL_TKN.U8_DYN_IN_PIN, Convert.ToByte(isDynamic(inTuples))),
                GetTuple(SKL_TKN.U8_DYN_OUT_PIN, Convert.ToByte(isDynamic(outTuples))),
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
                // Implicit assumption that copier's dma_buffer_size equals ibs|obs times two
                GetTuple(SKL_TKN.U32_DMA_BUF_SIZE, 2u),
            };

            tuples.Add(words);
            var configs = GetPathConfigurationsTuples(path.PathConfigurations, module);
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
            result.Add(section);
            result.Add(section.GetPrivateData());

            int num = 1;
            InitParam[] initParams = module.InitParams;
            if (initParams == null)
                initParams = template.InitParams;
            if (initParams != null)
                for (int i = 0; i < initParams.Length; i++, num += 2)
                    result.AddRange(GetInitParamSections(initParams[i], moduleId));

            desc = section.GetNumDescriptor(num);
            result.Insert(0, desc);
            result.Insert(1, desc.GetPrivateData());
            return result;
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
            SKL_MODULE_TYPE type = module.Type.GetModuleType();
            if (type != SKL_MODULE_TYPE.COPIER &&
                type != SKL_MODULE_TYPE.MIXER)
                return null;

            uint? subseq = 0;
            bool isSource = (module.ModulePosition == ModulePosition.SOURCE);
            // FE pipeline
            if (path.ConnType == ConnType.HOST_DMA ||
                path.ConnType == ConnType.HDMI_HOST_DMA)
            {
                if (path.Order == 0)
                    subseq = isSource ? HDA_DAPM_SUBSEQ.FE_SRC_MIX
                                      : HDA_DAPM_SUBSEQ.FE_SRC_PGA;
                else if (path.Order == 7)
                    subseq = isSource ? HDA_DAPM_SUBSEQ.FE_SINK_MIX
                                      : HDA_DAPM_SUBSEQ.FE_SINK_PGA;
            }
            // BE pipeline
            else if (path.ConnType == ConnType.LINK_DMA)
            {
                // First pipeline
                if (path.Order == 0)
                    subseq = isSource ? HDA_DAPM_SUBSEQ.BE_SRC_MIX
                                      : HDA_DAPM_SUBSEQ.BE_SRC_PGA;
                // Last pipeline
                else if (path.Order == 7)
                    subseq = isSource ? HDA_DAPM_SUBSEQ.BE_SINK_MIX
                                      : HDA_DAPM_SUBSEQ.BE_SINK_PGA;
            }
            // Intermediate pipeline
            else
            {
                subseq = isSource ? HDA_DAPM_SUBSEQ.INTERMEDIATE_MIX
                                  : HDA_DAPM_SUBSEQ.INTERMEDIATE_PGA;
            }

            return (subseq > 0) ? subseq : null;
        }

        static Ops GetControlBytesExtOps(string module)
        {
            uint call;
            if (module.Equals(ModuleNames.Probe))
                call = Constants.SKL_CTL_TLV_PROBE;
            else
                call = Constants.SKL_CTL_TLV_BYTE;

            return new Ops("ctl") { Get = call, Put = call };
        }

        static IEnumerable<Section> GetBytesControls(ModuleType[] templates, Module module)
        {
            var result = new List<Section>();
            Param[] prms = module.Params;

            if (prms == null)
                prms = GetTemplate(templates, module.Type).Params;

            if (prms != null)
            {
                Ops ops = GetControlBytesExtOps(module.Type);
                foreach (var param in prms)
                    result.AddRange(GetParamSections(param, ops));
            }

            return result;
        }

        static SectionControlMixer GetMixerControl(string name,
            int max, uint get, uint put,
            int reg = Constants.NOPM,
            int rreg = Constants.NOPM,
            uint info = TPLG_CTL.VOLSW)
        {
            var control = new SectionControlMixer(name);
            control.Index = 0;
            control.Invert = false;
            control.Channel = new ChannelMap[]
            {
                new ChannelMap(ChannelName.FrontLeft) { Reg = reg },
                new ChannelMap(ChannelName.FrontRight) { Reg = rreg }
            };
            control.Ops = new Ops("ctl") { Get = get, Put = put, Info = info };
            control.Max = max;

            return control;
        }

        static IEnumerable<Section> GetModuleControls(Module module)
        {
            var result = new List<Section>();

            if (module.Type.Equals(ModuleNames.Gain))
            {
                result.Add(GetMixerControl("Ramp Duration",
                    Constants.GAIN_TC_MAX,
                    Constants.SKL_CTL_RAMP_DURATION,
                    Constants.SKL_CTL_RAMP_DURATION));

                result.Add(GetMixerControl("Ramp Type",
                    Constants.GAIN_RT_MAX,
                    Constants.SKL_CTL_RAMP_TYPE,
                    Constants.SKL_CTL_RAMP_TYPE));

                result.Add(GetMixerControl("Volume",
                    Constants.GAIN_MAX_INDEX,
                    Constants.SKL_CTL_VOLUME,
                    Constants.SKL_CTL_VOLUME,
                    Constants.NOPM + 1, Constants.NOPM + 2,
                    Constants.SKL_CTL_VOLUME));
            }

            return result;
        }

        static IEnumerable<Section> GetPathModuleSections(ModuleType[] templates, PathConnector[] connectors, Path path, Module module)
        {
            var result = new List<Section>();

            IEnumerable<Section> sections = GetModuleSections(templates, connectors, module, path);
            var widget = new SectionWidget(GetWidgetName(path, module));
            widget.Type = module.ModulePosition.ToDapm();
            widget.NoPm = true;
            widget.Data = sections.Where(s => s is SectionData)
                .Select(s => s.Identifier).ToArray();
            widget.EventType = GetEventType(path, module);
            widget.EventFlags = GetEventFlags(path, module);
            widget.Subseq = GetSubseq(path, module);

            result.AddRange(sections);
            result.Add(widget);
            result.AddRange(GetModuleControls(module));
            result.AddRange(GetBytesControls(templates, module));

            IEnumerable<string> ids = result.OfType<SectionControlBytes>()
                .Select(c => c.Identifier);
            if (ids.Any())
                widget.Bytes = ids.ToArray();

            ids = result.OfType<SectionControlMixer>()
                .Select(c => c.Identifier);
            // Append mixers from path connectors
            if (connectors != null)
            {
                IEnumerable<PathConnector> query = connectors.Where(
                    c => c.Type == LinkType.MIXER && c.Output.Any(
                        o => o.PathName.Equals(path.Name) &&
                             o.Module.Equals(module.Type) &&
                             o.Instance == module.Instance));

                IEnumerable<InputOutput> inputs = query.SelectMany(c => c.Input);
                ids = ids.Concat(inputs.Select(
                    i => GetMixerName(i.PathName, i.Module)));
            }

            if (ids.Any())
                widget.Mixer = ids.ToArray();
            return result;
        }

        static IEnumerable<Section> GetPathConfigurationsSections(Path path)
        {
            var result = new List<Section>();
            PathConfiguration[] cfgs = path.PathConfigurations.PathConfiguration;
            if (cfgs.Length <= 1)
                return result;
            if (path.ConnType != ConnType.LINK_DMA)
            {
                PinDir dir = (path.Direction == Direction.PLAYBACK) ? PinDir.OUT : PinDir.IN;
                IEnumerable<PcmFormat> fmts = cfgs.Select(c => c.PcmFormats.First(f => f.Dir == dir));
                if (fmts.Distinct().Count() <= 1)
                    return result;
            }

            var control = new SectionControlEnum($"{path.Name} pcm cfg");
            var text = new SectionText($"enum_{path.Name} pcm cfg");
            var values = new List<string>();
            var value = new StringBuilder();

            foreach (var cfg in cfgs)
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

        static IEnumerable<Section> GetPathSections(ModuleType[] templates, PathConnector[] connectors, Path path)
        {
            var result = new List<Section>();

            foreach (var module in path.Modules.Module)
                result.AddRange(GetPathModuleSections(templates, connectors, path, module));
            result.AddRange(GetPathConfigurationsSections(path));

            if (path.Port != null)
            {
                var widget = new SectionWidget(path.Port);
                widget.Type = (path.Direction == Direction.CAPTURE) ? TPLG_DAPM.AIF_IN
                                                                    : TPLG_DAPM.AIF_OUT;
                widget.NoPm = true;
                result.Add(widget);
            }

            return result;
        }

        static IEnumerable<Section> GetParamSections(Param param, Ops extOps)
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

        public static IEnumerable<Section> GetPathsSections(ModuleType[] templates, PathConnector[] connectors, Path[] paths)
        {
            var result = new List<Section>();
            if (paths == null)
                return result;

            foreach (var path in paths)
                result.AddRange(GetPathSections(templates, connectors, path));

            result = result.Distinct(new SectionComparer()).ToList();
            return result;
        }

        public static IEnumerable<Section> GetPathConnectorsSections(PathConnector[] connectors)
        {
            var result = new List<Section>();
            if (connectors == null)
                return result;

            IEnumerable<PathConnector> query = connectors
                .Where(c => c.Type == LinkType.MIXER);
            foreach (var entry in query)
            {
                foreach (var input in entry.Input)
                    result.Add(GetMixerControl(
                        GetMixerName(input.PathName, input.Module), 1,
                        TPLG_CTL.DAPM_VOLSW,
                        TPLG_CTL.DAPM_VOLSW));
            }

            query = connectors
                .Where(c => c.Type == LinkType.SWITCH);
            foreach (var entry in query)
            {
                SectionControlMixer mixer = GetMixerControl(
                    "Switch", 1,
                    TPLG_CTL.DAPM_VOLSW,
                    TPLG_CTL.DAPM_VOLSW);

                var widget = new SectionWidget(entry.Name);
                widget.Index = 0;
                widget.Type = TPLG_DAPM.SWITCH;
                widget.NoPm = true;
                widget.Mixer = new[] { mixer.Identifier };
                result.Add(mixer);
                result.Add(widget);
            }

            result = result.Distinct(new SectionComparer()).ToList();
            return result;
        }

        static string GetPathFirstRoute(Path path)
        {
            var route = new StringBuilder();
            Func<Module, bool> predicate = (m => true);

            if (path.Modules.Module.Length > 1)
                predicate = (m => m.ModulePosition != ModulePosition.SINK);
            Module source = path.Modules.Module.First(predicate);

            if (path.Direction == Direction.PLAYBACK &&
                path.Device != null)
            {
                route.Append(GetWidgetName(path, source));
                route.Append($", , {path.Device}");
            }
            else if (path.Direction == Direction.CAPTURE &&
                path.Port != null)
            {
                route.Append(GetWidgetName(path, source));
                route.Append($", , {path.Port}");
            }

            return route.ToString();
        }

        static string GetPathLastRoute(Path path)
        {
            var route = new StringBuilder();
            Func<Module, bool> predicate = (m => true);

            if (path.Modules.Module.Length > 1)
                predicate = (m => m.ModulePosition == ModulePosition.SINK);
            Module sink = path.Modules.Module.Last(predicate);

            if (path.Direction == Direction.PLAYBACK &&
                path.Port != null)
            {
                route.Append($"{path.Port}, , ");
                route.Append(GetWidgetName(path, sink));
            }
            else if (path.Direction == Direction.CAPTURE &&
                path.Device != null)
            {
                route.Append($"{path.Device}, , ");
                route.Append(GetWidgetName(path, sink));
            }

            return route.ToString();
        }

        static IEnumerable<string> GetPathRoutes(Path path)
        {
            var result = new List<string>();
            var routes = new StringBuilder();

            string route = GetPathFirstRoute(path);
            if (!string.IsNullOrEmpty(route))
                result.Add(route);

            foreach (var link in path.Links)
            {
                routes.Clear();
                routes.Append(GetWidgetName(path, link.To));
                routes.Append(", , ");
                routes.Append(GetWidgetName(path, link.From));
                result.Add(routes.ToString());
            }

            route = GetPathLastRoute(path);
            if (!string.IsNullOrEmpty(route))
                result.Add(route);

            return result;
        }

        static IEnumerable<string> GetConnectorRoutes(PathConnector connector)
        {
            var result = new List<string>();
            var route = new StringBuilder();

            foreach (var input in connector.Input)
            {
                foreach (var output in connector.Output)
                {
                    route.Clear();
                    route.Append(GetWidgetName(output));
                    string control = string.Empty;
                    if (connector.Type == LinkType.MIXER)
                        control = GetMixerName(input.PathName, input.Module);
                    route.Append($", {control}, ");

                    if (connector.Type == LinkType.SWITCH)
                    {
                        route.Append(connector.Name);
                        result.Add(route.ToString());
                        route.Clear();
                        route.Append(connector.Name);
                        route.Append(", Switch, ");
                    }

                    route.Append(GetWidgetName(input));
                    result.Add(route.ToString());
                }
            }

            return result;
        }

        public static SectionGraph GetGraphSection(Path[] paths, PathConnector[] connectors)
        {
            var graph = new SectionGraph("Pipeline 1 Graph");
            var routes = new List<string>();

            if (paths != null)
                foreach (var path in paths)
                    routes.AddRange(GetPathRoutes(path));
            if (connectors != null)
                foreach (var connector in connectors)
                    routes.AddRange(GetConnectorRoutes(connector));

            graph.Lines = routes.ToArray();
            return graph;
        }

        static SectionPCMCapabilities GetPCMCapabilities(Path path)
        {
            PinDir dir = (path.Direction == Direction.PLAYBACK) ? PinDir.IN
                                                                : PinDir.OUT;
            PathConfiguration[] configurations = path.PathConfigurations.PathConfiguration;
            IEnumerable<PcmFormat> formats = configurations.SelectMany(
                p => p.PcmFormats.Where(f => f.Dir == dir));

            IEnumerable<PCM_RATE> rates = formats.Select(f => f.SampleRate.ToRate());
            IEnumerable<uint> channels = formats.Select(f => f.ChannelCount).Distinct();
            IEnumerable<uint> bps = formats.Select(f => f.Bps);

            var result = new SectionPCMCapabilities(path.Device);
            result.Formats.UnionWith(formats.Select(f => f.ToFormat()));
            result.Rates.UnionWith(rates);
            result.ChannelsMin = channels.Min();
            result.ChannelsMax = channels.Max();
            result.SigBits = bps.Max();

            return result;
        }

        public static IEnumerable<Section> GetPCMSections(Path[] paths)
        {
            var result = new List<Section>();
            if (paths == null)
                return result;

            IEnumerable<Path> fePaths = paths.Where(
                p => p.Device != null && p.DaiName != null && p.DaiLinkName != null);

            uint i = 0;
            var groups = fePaths.GroupBy(p => p.DaiLinkName).ToArray();
            foreach (var group in groups)
            {
                var section = new SectionPCM(group.Key);
                section.DAI = new FE_DAI(group.First().DaiName) { ID = i++ };

                Path path = group.FirstOrDefault(p => p.Direction == Direction.PLAYBACK);
                // Check playback path connection type for HDMI
                if (path != null)
                {
                    if (path.ConnType == ConnType.HDMI_HOST_DMA)
                        section.ID = 0xFF;
                    SectionPCMCapabilities caps = GetPCMCapabilities(path);
                    result.Add(caps);
                    section.Playback = new PCMStream("playback");
                    section.Playback.Capabilities = caps.Identifier;
                }

                path = group.FirstOrDefault(p => p.Direction == Direction.CAPTURE);
                if (path != null)
                {
                    SectionPCMCapabilities caps = GetPCMCapabilities(path);
                    result.Add(caps);
                    section.Capture = new PCMStream("capture");
                    section.Capture.Capabilities = caps.Identifier;
                }

                result.Add(section);
            }

            return result;
        }

        public static IEnumerable<Section> GetManifestSections(System topology, IEnumerable<Section> current)
        {
            if (current == null)
                throw new ArgumentNullException(nameof(current));

            int num = 0;
            if (topology.GetManifestData() != null)
                num++;
            if (topology.GetModuleTypes() != null)
                num++;
            FirmwareConfig config = topology.GetFirmwareConfig();
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
            IEnumerable<Section> sections = result.Concat(current);

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
