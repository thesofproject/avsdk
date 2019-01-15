using NUmcSerializer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

namespace itt
{
    public static class UmcConvert
    {
        internal static Tuple<string, T> GetTuple<T>(SKL_TKN token, T value)
        {
            return Tuple.Create(token.GetName(), value);
        }

        public static IEnumerable<Section> GetSections(FirmwareInfo[] infos)
        {
            var result = new List<Section>();
            var section = new SectionSkylakeTuples("lib_data");
            var tuples = new List<VendorTuples>();

            var words = new VendorTuples<uint>();
            words.Identifier = "lib_count";
            words.Tuples = new[] { GetTuple(SKL_TKN.U32_LIB_COUNT, (uint)infos.Length) };
            tuples.Add(words);

            for (int i = 0; i < infos.Length; i++)
            {
                var strings = new VendorTuples<string>();
                strings.Identifier = $"lib_name_{i}";
                strings.Tuples = new[] { GetTuple(SKL_TKN.STR_LIB_NAME, infos[i].BinaryName) };
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

        public static IEnumerable<Section> ToSections(ClockControl control, int id)
        {
            var result = new List<Section>();
            AudioFormat fmt = control.AudioFormat;
            var section = new SectionSkylakeTuples($"dmactrl_cfg{id}");

            var words = new VendorTuples<uint>();
            words.Identifier = "u32_data";
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

            var priv = new SectionData();
            priv.Identifier = $"dmactrl_data{id}";
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

        public static IEnumerable<Section> ToSections(ClockControls controls)
        {
            var result = new List<Section>();
            var ctrls = new List<ClockControl>();

            if (controls.I2SClockControls != null)
                ctrls.AddRange(controls.I2SClockControls);
            if (controls.MClockControls != null)
                ctrls.AddRange(controls.MClockControls);

            for (int i = 0; i < ctrls.Count; i++)
                result.AddRange(ToSections(ctrls[i], i));
            return result;
        }

        public static IEnumerable<VendorTuples> ToTuples(DMABufferConfig buffer, int id)
        {
            var words = new VendorTuples<uint>();
            words.Identifier = $"u32_dma_buf_index_{id}";
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.U32_DMA_IDX, (uint)id),
                GetTuple(SKL_TKN.U32_DMA_MIN_SIZE, buffer.MinSizeBytes),
                GetTuple(SKL_TKN.U32_DMA_MAX_SIZE, buffer.MaxSizeBytes)
            };

            return new[] { words };
        }

        public static IEnumerable<Section> ToSections(FirmwareConfig config)
        {
            var result = new List<Section>();

            var tuples = new List<VendorTuples>();
            var words = new VendorTuples<uint>();
            words.Identifier = "u32_dma_buf";
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.U32_DMA_TYPE, 4u),
                GetTuple(SKL_TKN.U32_DMA_SIZE, (uint)(config.DMABufferConfigs.Length *
                                            Marshal.SizeOf(typeof(DMABufferConfig))))
            };

            tuples.Add(words);
            for (int i = 0; i < 24; i++)
            {
                DMABufferConfig buf;
                if (i < config.DMABufferConfigs.Length)
                    buf = config.DMABufferConfigs[i];
                else
                    buf = new DMABufferConfig();
                tuples.AddRange(ToTuples(buf, i));
            }

            var section = new SectionSkylakeTuples("fw_cfg_data");
            section.Tuples = tuples.ToArray();
            SectionVendorTuples desc = section.GetSizeDescriptor();
            result.Add(desc);
            result.Add(desc.GetPrivateData());
            result.Add(section);
            result.Add(section.GetPrivateData());
            result.AddRange(ToSections(config.ClockControls));

            return result;
        }

        public static IEnumerable<VendorTuples> ToTuples(Interface iface, int mod, int intf, int id)
        {
            string dir = (iface.Dir == PinDir.IN) ? "input" : "output";
            AudioFormat fmt = iface.AudioFormat;

            var words = new VendorTuples<uint>();
            words.Identifier = $"u32_mod_type_{mod}_intf_{intf}_{dir}_{id}";
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

        public static IEnumerable<VendorTuples> ToTuples(Interfaces ifaces, int mod, int id)
        {
            var result = new List<VendorTuples>();
            var inputIfaces = ifaces.Interface.Where(intf => intf.Dir == PinDir.IN).ToArray();
            var outputIfaces = ifaces.Interface.Where(intf => intf.Dir == PinDir.OUT).ToArray();

            var words = new VendorTuples<uint>();
            words.Identifier = $"u32_mod_type_{mod}_res_{id}";
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.MM_U32_FMT_ID, ifaces.IntfIdx),
                GetTuple(SKL_TKN.MM_U32_NUM_IN_FMT, (uint)inputIfaces.Length),
                GetTuple(SKL_TKN.MM_U32_NUM_OUT_FMT, (uint)outputIfaces.Length),
            };

            result.Add(words);

            for (int i = 0; i < ifaces.Interface.Length; i++)
                result.AddRange(ToTuples(ifaces.Interface[i], mod, id, i));
            return result;
        }

        public static IEnumerable<VendorTuples> ToTuples(OutputPinFormat format, int mod, int res, int id)
        {
            var words = new VendorTuples<uint>();
            words.Identifier = $"u32_mod_type_{mod}_res_{res}_output_{id}";
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.U32_DIR_PIN_COUNT, (uint)PinDir.OUT),
                GetTuple(SKL_TKN.MM_U32_RES_PIN_ID, format.PinIndex),
                GetTuple(SKL_TKN.MM_U32_PIN_BUF, format.Obs),
            };

            return new[] { words };
        }

        public static IEnumerable<VendorTuples> ToTuples(InputPinFormat format, int mod, int res, int id)
        {
            var words = new VendorTuples<uint>();
            words.Identifier = $"u32_mod_type_{mod}_res_{res}_input_{id}";
            words.Tuples = new[]
            {
                GetTuple(SKL_TKN.U32_DIR_PIN_COUNT, (uint)PinDir.IN),
                GetTuple(SKL_TKN.MM_U32_RES_PIN_ID, format.PinIndex),
                GetTuple(SKL_TKN.MM_U32_PIN_BUF, format.Ibs),
            };

            return new[] { words };
        }

        public static IEnumerable<VendorTuples> ToTuples(ModuleResources resources, int mod, int id)
        {
            var result = new List<VendorTuples>();
            var words = new VendorTuples<uint>();
            words.Identifier += $"u32_mod_type_0_res_{id}";
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
                result.AddRange(ToTuples(resources.InputPins[i], mod, id, i));
            for (int i = 0; i < resources.OutputPins.Length; i++)
                result.AddRange(ToTuples(resources.OutputPins[i], mod, id, i));
            return result;
        }

        internal static byte[] ToBytes(this string value)
        {
            var result = new List<byte>();
            var substrs = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim());

            foreach (var substr in substrs)
            {
                if (substr.StartsWith("0x") &&
                    byte.TryParse(substr.Substring(2), NumberStyles.HexNumber,
                                        CultureInfo.InvariantCulture, out byte val))
                    result.Add(val);
                else if (byte.TryParse(substr, out val))
                    result.Add(val);
            }

            return result.ToArray();
        }

        public static IEnumerable<Section> ToSections(Param param, uint get, uint put)
        {
            var section = new SectionData();
            section.Identifier = $"{param.Name} params";
            byte[] defVal = param.DefaultValue.ToBytes();

            var data = new DfwAlgoData();
            data.SetParams = param.SetParams;
            data.RuntimeApplicable = param.RuntimeApplicable;
            data.ValueCacheable = param.ValueCacheable;
            data.NotificationCtrl = param.NotificationCtrl;
            data.ParamId = param.paramId;
            data.Size = (uint)defVal.Length;

            byte[] bytes = MarshalHelper.StructureToBytes(data);
            int offset = bytes.Length - Marshal.SizeOf(data.Data);
            Array.Resize(ref bytes, offset + defVal.Length);
            defVal.CopyTo(bytes, offset);
            section.Bytes = bytes;

            var control = new SectionControlBytes();
            control.Identifier = section.Identifier;
            control.Max = (int)data.Size;
            control.Mask = 0;
            control.Base = 0;
            control.NumRegs = 0;
            control.Ops = new Ops { Identifier = "ctl", Info = TPLG_CTL.BYTES };
            control.Access = new[]
            {
                CTL_ELEM_ACCESS.TLV_READ,
                CTL_ELEM_ACCESS.TLV_WRITE,
                CTL_ELEM_ACCESS.TLV_READWRITE,
                CTL_ELEM_ACCESS.TLV_CALLBACK
            };
            control.ExtOps = new Ops { Get = get, Put = put };
            control.Data = section.Identifier;

            return new Section[] { control, section };
        }

        public static IEnumerable<VendorTuples> ToTuples(ModuleType template, int id)
        {
            var result = new List<VendorTuples>();

            var uuids = new VendorTuples<Guid>();
            uuids.Identifier = $"mod_{id}";
            uuids.Tuples = new[] { GetTuple(SKL_TKN.UUID, template.uuid) };
            result.Add(uuids);

            var bytes = new VendorTuples<byte>();
            bytes.Identifier = $"u8_mod_type_{id}";
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
                result.AddRange(ToTuples(template.ModuleResourceList[i], id, i));
            for (int i = 0; i < template.ModuleInterfaceList.Length; i++)
                result.AddRange(ToTuples(template.ModuleInterfaceList[i], id, i));
            return result;
        }

        public static IEnumerable<Section> GetSections(ModuleType[] templates)
        {
            var result = new List<Section>();
            var section = new SectionSkylakeTuples("mod_type_data");
            var tuples = new List<VendorTuples>();

            var bytes = new VendorTuples<byte>();
            bytes.Identifier = "u8_num_mod";
            bytes.Tuples = new[] { GetTuple(SKL_TKN.U8_NUM_MOD, (byte)templates.Length) };
            tuples.Add(bytes);

            for (int i = 0; i < templates.Length; i++)
                tuples.AddRange(ToTuples(templates[i], i));

            section.Tuples = tuples.ToArray();
            SectionVendorTuples desc = section.GetSizeDescriptor();
            result.Add(desc);
            result.Add(desc.GetPrivateData());
            result.Add(section);
            result.Add(section.GetPrivateData());

            return result;
        }

        public static IEnumerable<Section> ToSections(System system)
        {
            if (system == null)
                throw new ArgumentNullException("system");
            if (system.SubsystemType == null)
                throw new ArgumentException("system.SubsystemType");

            var result = new List<Section>();
            result.Add(new SectionSkylakeTokens());
            SubsystemType[] subsystems = system.SubsystemType;

            // Convert Subsystem with firmware config
            SubsystemType sub = subsystems.SingleOrDefault(
                e => e.ManifestData != null && e.FirmwareConfig != null);
            if (sub != null)
            {
                result.AddRange(GetSections(sub.ManifestData));
                result.AddRange(ToSections(sub.FirmwareConfig));
            }

            // Convert Subsystem with module types, if none found, bail out
            sub = subsystems.SingleOrDefault(e => e.ModuleTypes != null);
            if (sub == null)
                return result;

            ModuleType[] templates = sub.ModuleTypes;
            result.AddRange(GetSections(templates));

            return result;
        }
    }
}
