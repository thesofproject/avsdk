using NUmcSerializer;
using System;
using System.Collections.Generic;
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

            return result;
        }
    }
}
