//
// Copyright (c) 2020-2022, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.Collections.Generic;
using System.Linq;
using NUcmSerializer;

namespace avstplg
{
    public static class SectionProvider
    {
        static Tuple<string, U> GetTuple<T, U>(T token, U value)
            where T : struct
        {
            Type type = typeof(T);

            return Tuple.Create($"{type.Name}_{Enum.GetName(type, token)}", value);
        }

        static SectionVendorTokens GetSectionTokens<T>(string identifier)
            where T : struct
        {
            Type type = typeof(T);
            IEnumerable<uint> values = Enum.GetValues(type).Cast<uint>();

            var section = new SectionVendorTokens(identifier);
            section.Tokens = values.Select(
                v => Tuple.Create($"{type.Name}_{Enum.GetName(type, v)}", v)).ToArray();

            return section;
        }

        public static IEnumerable<Section> GetAllSectionTokens()
        {
            var sections = new List<Section>();

            sections.Add(GetSectionTokens<AVS_TKN_MANIFEST>("avs_manifest_tokens"));
            sections.Add(GetSectionTokens<AVS_TKN_LIBRARY>("avs_library_tokens"));
            sections.Add(GetSectionTokens<AVS_TKN_AFMT>("avs_audio_format_tokens"));
            sections.Add(GetSectionTokens<AVS_TKN_MODCFG_BASE>("avs_modcfg_base_tokens"));
            sections.Add(GetSectionTokens<AVS_TKN_MODCFG>("avs_modcfg_ext_tokens"));
            sections.Add(GetSectionTokens<AVS_TKN_PPLCFG>("avs_pplcfg_tokens"));
            sections.Add(GetSectionTokens<AVS_TKN_BINDING>("avs_binding_tokens"));
            sections.Add(GetSectionTokens<AVS_TKN_PPL>("avs_pipeline_tokens"));
            sections.Add(GetSectionTokens<AVS_TKN_MOD>("avs_module_tokens"));
            sections.Add(GetSectionTokens<AVS_TKN_PATH_TMPL>("avs_path_template_tokens"));
            sections.Add(GetSectionTokens<AVS_TKN_PATH>("avs_path_tokens"));
            sections.Add(GetSectionTokens<AVS_TKN_CONDPATH_TMPL>("avs_condpath_template_tokens"));
            sections.Add(GetSectionTokens<AVS_TKN_CONDPATH>("avs_condpath_tokens"));
            sections.Add(GetSectionTokens<AVS_TKN_PIN_FMT>("avs_pin_format_tokens"));
            sections.Add(GetSectionTokens<AVS_TKN_KCONTROL>("avs_kcontrol_tokens"));
            sections.Add(GetSectionTokens<AVS_TKN_INIT_CONFIG>("avs_init_config_tokens"));

            return sections;
        }

        public static Section GetLibrarySection(Library library, int id)
        {
            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_LIBRARY.ID_U32, library.Id),
            };

            var strings = new VendorTuples<string>();
            strings.Tuples = new[]
            {
                GetTuple(AVS_TKN_LIBRARY.FILENAME_STRING, library.FileName),
            };

            var section = new SectionVendorTuples($"library{id}_tuples");
            section.Tokens = "avs_library_tokens";
            section.Tuples = new VendorTuples[] { words, strings };

            return section;
        }

        public static IEnumerable<Section> GetLibrariesSections(Library[] libraries)
        {
            var sections = new List<Section>();

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_MANIFEST.NUM_LIBRARIES_U32, (uint)libraries.Length),
            };

            var tuples = new SectionVendorTuples("library_hdr_tuples");
            tuples.Tokens = "avs_manifest_tokens";
            tuples.Tuples = new VendorTuples[] { words };
            sections.Add(tuples);

            for (int i = 0; i < libraries.Length; i++)
                sections.Add(GetLibrarySection(libraries[i], i));

            // create private section referencing all added entries
            var data = new SectionData("library_data");
            data.Tuples = sections.Select(s => s.Identifier).ToArray();
            sections.Add(data);

            return sections;
        }

        public static Section GetAudioFormatSection(AudioFormat format, int id)
        {
            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_AFMT.ID_U32, format.Id),
                GetTuple(AVS_TKN_AFMT.SAMPLE_RATE_U32, format.SampleRate),
                GetTuple(AVS_TKN_AFMT.BIT_DEPTH_U32, format.BitDepth),
                GetTuple(AVS_TKN_AFMT.CHANNEL_MAP_U32, format.channelMap),
                GetTuple(AVS_TKN_AFMT.CHANNEL_CFG_U32, format.ChannelConfig),
                GetTuple(AVS_TKN_AFMT.INTERLEAVING_U32, format.Interleaving),
                GetTuple(AVS_TKN_AFMT.NUM_CHANNELS_U32, format.NumChannels),
                GetTuple(AVS_TKN_AFMT.VALID_BIT_DEPTH_U32, format.ValidBitDepth),
                GetTuple(AVS_TKN_AFMT.SAMPLE_TYPE_U32, format.SampleType),
            };

            var section = new SectionVendorTuples($"audio_format{id}_tuples");
            section.Tokens = "avs_audio_format_tokens";
            section.Tuples = new VendorTuples[] { words };

            return section;
        }

        public static IEnumerable<Section> GetAudioFormatsSections(AudioFormat[] formats)
        {
            var sections = new List<Section>();

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_MANIFEST.NUM_AFMTS_U32, (uint)formats.Length),
            };

            var tuples = new SectionVendorTuples("audio_format_hdr_tuples");
            tuples.Tokens = "avs_manifest_tokens";
            tuples.Tuples = new VendorTuples[] { words };
            sections.Add(tuples);

            for (int i = 0; i < formats.Length; i++)
                sections.Add(GetAudioFormatSection(formats[i], i));

            // create private section referencing all added entries
            var data = new SectionData("audio_format_data");
            data.Tuples = sections.Select(s => s.Identifier).ToArray();
            sections.Add(data);

            return sections;
        }

        public static Section GetModuleConfigBaseSection(ModuleConfigBase config, int id)
        {
            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_MODCFG_BASE.ID_U32, config.Id),
                GetTuple(AVS_TKN_MODCFG_BASE.CPC_U32, config.Cpc),
                GetTuple(AVS_TKN_MODCFG_BASE.IBS_U32, config.Ibs),
                GetTuple(AVS_TKN_MODCFG_BASE.OBS_U32, config.Obs),
                GetTuple(AVS_TKN_MODCFG_BASE.PAGES_U32, config.Pages),
            };

            var section = new SectionVendorTuples($"modcfg_base{id}_tuples");
            section.Tokens = "avs_modcfg_base_tokens";
            section.Tuples = new VendorTuples[] { words };

            return section;
        }

        public static IEnumerable<Section> GetModuleConfigsBaseSections(ModuleConfigBase[] configs)
        {
            var sections = new List<Section>();

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_MANIFEST.NUM_MODCFGS_BASE_U32, (uint)configs.Length),
            };

            var tuples = new SectionVendorTuples("modcfg_base_hdr_tuples");
            tuples.Tokens = "avs_manifest_tokens";
            tuples.Tuples = new VendorTuples[] { words };
            sections.Add(tuples);

            for (int i = 0; i < configs.Length; i++)
                sections.Add(GetModuleConfigBaseSection(configs[i], i));

            // create private section referencing all added entries
            var data = new SectionData("modcfg_base_data");
            data.Tuples = sections.Select(s => s.Identifier).ToArray();
            sections.Add(data);

            return sections;
        }

        public static Section GetPinFormatSection(IOPinFormat pin, string namePrefix, uint id)
        {
            var wordTuples = new List<Tuple<string, uint>>()
            {
                GetTuple(AVS_TKN_PIN_FMT.INDEX_U32, id),
                GetTuple(AVS_TKN_PIN_FMT.IOBS_U32, pin.IObs),
                GetTuple(AVS_TKN_PIN_FMT.AFMT_ID_U32, pin.AudioFormatId),
            };

            var words = new VendorTuples<uint>();
            words.Tuples = wordTuples.ToArray();

            var section = new SectionVendorTuples($"{namePrefix}pin{id}_tuples");
            section.Tokens = "avs_pin_format_tokens";
            section.Tuples = new VendorTuples[] { words };

            return section;
        }

        public static IEnumerable<Section> GetPinFormatsSections(IOPinFormat[] pins, string namePrefix)
        {
            var sections = new List<Section>();

            for (uint i = 0; i < pins.Length; i++)
                sections.Add(GetPinFormatSection(pins[i], namePrefix, i));

            return sections;
        }

        public static Section GetModuleConfigExtSection(ModuleConfigExt module, int id)
        {
            var uuids = new VendorTuples<Guid>();
            uuids.Tuples = new[]
            {
                GetTuple(AVS_TKN_MODCFG.EXT_TYPE_UUID, module.uuid),
            };

            var wordTuples = new List<Tuple<string, uint>>
            {
                GetTuple(AVS_TKN_MODCFG.EXT_ID_U32, module.Id),
            };

            var byteTuples = new List<Tuple<string, byte>>();

            // module-type specific tuples
            if (module.CprOutAudioFormatId.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.CPR_OUT_AFMT_ID_U32, module.CprOutAudioFormatId.Value));
            if (module.CprBlobFormatId.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.CPR_BLOB_FMT_ID_U32, module.CprBlobFormatId.Value));
            if (module.CprFeatureMask.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.CPR_FEATURE_MASK_U32, module.CprFeatureMask.Value));
            if (module.CprVirtualIndex != null)
                byteTuples.Add(GetTuple(AVS_TKN_MODCFG.CPR_VINDEX_U8, module.CprVirtualIndex.Value));
            if (module.cprDMAType.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.CPR_DMA_TYPE_U32, module.cprDMAType.Value));
            if (module.CprDMABufferSize != null)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.CPR_DMABUFF_SIZE_U32, module.CprDMABufferSize.Value));
            if (module.MicselOutAudioFormatId.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.MICSEL_OUT_AFMT_ID_U32, module.MicselOutAudioFormatId.Value));
            if (module.IntelWOVCpcLowPowerMode.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.INTELWOV_CPC_LP_MODE_U32, module.IntelWOVCpcLowPowerMode.Value));
            if (module.SrcOutFrequency.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.SRC_OUT_FREQ_U32, module.SrcOutFrequency.Value));
            if (module.MuxRefAudioFormatId.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.MUX_REF_AFMT_ID_U32, module.MuxRefAudioFormatId.Value));
            if (module.MuxOutAudioFormatId.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.MUX_OUT_AFMT_ID_U32, module.MuxOutAudioFormatId.Value));
            if (module.AecRefAudioFormatId.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.AEC_REF_AFMT_ID_U32, module.AecRefAudioFormatId.Value));
            if (module.AecOutAudioFormatId.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.AEC_OUT_AFMT_ID_U32, module.AecOutAudioFormatId.Value));
            if (module.AecCpcLowPowerMode.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.AEC_CPC_LP_MODE_U32, module.AecCpcLowPowerMode.Value));
            if (module.UpDownMixOutChanCfg.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.UPDOWN_MIX_OUT_CHAN_CFG_U32, module.UpDownMixOutChanCfg.Value));
            if (module.UpDownMixCoeffSelect.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.UPDOWN_MIX_COEFF_SELECT_U32, module.UpDownMixCoeffSelect.Value));

            if (module.UpDownMixCoeff != null)
            {
                if (module.UpDownMixCoeff.Length > 8)
                    throw new InvalidOperationException("Too many coefficients passed to UpDownMix");
                for (int i = (int)AVS_TKN_MODCFG.UPDOWN_MIX_COEFF_0_S32; i <= (int)AVS_TKN_MODCFG.UPDOWN_MIX_COEFF_7_S32; i++)
                {
                    int j = i - (int)AVS_TKN_MODCFG.UPDOWN_MIX_COEFF_0_S32;
                    if (j <= module.UpDownMixCoeff.Length)
                        wordTuples.Add(GetTuple(i, (uint)module.UpDownMixCoeff[j]));
                }
            }
            if (module.UpDownMixChanMap.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.UPDOWN_MIX_CHAN_MAP_U32, module.UpDownMixChanMap.Value));

            if (module.ASrcOutFrequency.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.ASRC_OUT_FREQ_U32, module.ASrcOutFrequency.Value));
            if (module.ASrcMode.HasValue)
                byteTuples.Add(GetTuple(AVS_TKN_MODCFG.ASRC_MODE_U8, module.ASrcMode.Value));
            if (module.ASrcDisableJitterBuffer.HasValue)
                byteTuples.Add(GetTuple(AVS_TKN_MODCFG.ASRC_DISABLE_JITTER_BUFFER_U8, module.ASrcDisableJitterBuffer.Value));

            if (module.InPinFormats != null)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.EXT_NUM_INPUT_PINS_U16, (uint)module.InPinFormats.Length));
            if (module.OutPinFormats != null)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.EXT_NUM_OUTPUT_PINS_U16, (uint)module.OutPinFormats.Length));

            if (module.WhmRefAudioFormatId.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.WHM_REF_AFMT_ID_U32, module.WhmRefAudioFormatId.Value));
            if (module.WhmOutAudioFormatId.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.WHM_OUT_AFMT_ID_U32, module.WhmOutAudioFormatId.Value));
            if (module.WhmBlobFormatId.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.WHM_BLOB_FMT_ID_U32, module.WhmBlobFormatId.Value));
            if (module.WhmWakeTickPeriod.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.WHM_WAKE_TICK_PERIOD_U32, module.WhmWakeTickPeriod.Value));
            if (module.WhmVirtualIndex != null)
                byteTuples.Add(GetTuple(AVS_TKN_MODCFG.WHM_VINDEX_U8, module.WhmVirtualIndex.Value));
            if (module.whmDMAType.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.WHM_DMA_TYPE_U32, module.whmDMAType.Value));
            if (module.WhmDMABufferSize != null)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.WHM_DMABUFF_SIZE_U32, module.WhmDMABufferSize.Value));
            if (module.PeakVolVolume.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.PEAKVOL_VOLUME_U32, module.PeakVolVolume.Value));
            if (module.PeakVolCurveType.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.PEAKVOL_CURVE_TYPE_U32, module.PeakVolCurveType.Value));
            if (module.PeakVolCurveDuration.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.PEAKVOL_CURVE_DURATION_U32, module.PeakVolCurveDuration.Value));

            var words = new VendorTuples<uint>();
            words.Tuples = wordTuples.ToArray();

            var bytes = new VendorTuples<byte>();
            bytes.Tuples = byteTuples.ToArray();

            var section = new SectionVendorTuples($"modcfg_ext{id}_tuples");
            section.Tokens = "avs_modcfg_ext_tokens";
            section.Tuples = new VendorTuples[] { words, uuids, bytes };

            return section;
        }

        public static IEnumerable<Section> GetModuleConfigsExtSections(ModuleConfigExt[] configs)
        {
            var sections = new List<Section>();

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_MANIFEST.NUM_MODCFGS_EXT_U32, (uint)configs.Length),
            };

            var tuples = new SectionVendorTuples("modcfg_ext_hdr_tuples");
            tuples.Tokens = "avs_manifest_tokens";
            tuples.Tuples = new VendorTuples[] { words };
            sections.Add(tuples);

            for (int i = 0; i < configs.Length; i++)
            {
                sections.Add(GetModuleConfigExtSection(configs[i], i));
                if (configs[i].InPinFormats != null)
                    sections.AddRange(GetPinFormatsSections(configs[i].InPinFormats, $"modcfg_ext{i}_in"));
                if (configs[i].OutPinFormats != null)
                    sections.AddRange(GetPinFormatsSections(configs[i].OutPinFormats, $"modcfg_ext{i}_out"));
            }

            // create private section referencing all added entries
            var data = new SectionData("modcfg_ext_data");
            data.Tuples = sections.Select(s => s.Identifier).ToArray();
            sections.Add(data);

            return sections;
        }

        public static IEnumerable<Section> GetModuleInitConfigSections(ModuleInitConfig initConfig, int id)
        {
            var wordTuples = new List<Tuple<string, uint>>
            {
                GetTuple(AVS_TKN_INIT_CONFIG.ID_U32, initConfig.Id),
                GetTuple(AVS_TKN_INIT_CONFIG.LENGTH_U32, (uint)initConfig.Data.Length),
            };

            var byteTuples = new List<Tuple<string, byte>>
            {
                GetTuple(AVS_TKN_INIT_CONFIG.PARAM_U8, initConfig.Param)
            };

            var words = new VendorTuples<uint>();
            words.Tuples = wordTuples.ToArray();

            var bytes = new VendorTuples<byte>();
            bytes.Tuples = byteTuples.ToArray();

            var vendorTuples = new SectionVendorTuples($"module_init_config{id}_hdr_tuples");
            vendorTuples.Tokens = "avs_init_config_tokens";
            vendorTuples.Tuples = new VendorTuples[] { words, bytes };

            var sections = new List<Section>();
            sections.Add(vendorTuples);

            // create private section referencing added tuples entries
            var headerData = new SectionData($"module_init_config{id}_hdr_data");
            headerData.Tuples = sections.Select(s => s.Identifier).ToArray();
            sections.Add(headerData);

            // create private section referencing added bytes entries
            var data = new SectionData($"module_init_config{id}_data");
            data.Bytes = initConfig.Data;
            sections.Add(data);

            return sections;
        }

        public static IEnumerable<Section> GetModuleInitConfigsSections(ModuleInitConfig[] initConfigs)
        {
            var sections = new List<Section>();

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_MANIFEST.NUM_INIT_CONFIGS_U32, (uint)initConfigs.Length),
            };

            var tuples = new SectionVendorTuples("module_init_config_tuples");
            tuples.Tokens = "avs_manifest_tokens";
            tuples.Tuples = new VendorTuples[] { words };
            sections.Add(tuples);

            // create private section referencing all added entries
            var data = new SectionData("module_init_config_hdr_data");
            data.Tuples = sections.Select(s => s.Identifier).ToArray();
            sections.Add(data);

            // create private sections adding tuples and bytes entries
            for (int i = 0; i < initConfigs.Length; i++)
                sections.AddRange(GetModuleInitConfigSections(initConfigs[i], i));

            return sections;
        }

        public static Section GetPipelineConfigSection(PipelineConfig config, int id)
        {
            var wordTuples = new List<Tuple<string, uint>>
            {
                GetTuple(AVS_TKN_PPLCFG.ID_U32, config.Id),
            };

            var shortTuples = new List<Tuple<string, ushort>>
            {
                GetTuple(AVS_TKN_PPLCFG.REQ_SIZE_U16, config.RequiredSize),
            };

            var byteTuples = new List<Tuple<string, byte>>();
            var boolTuples = new List<Tuple<string, bool>>();

            if (config.Trigger.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_PPLCFG.TRIGGER_U32, config.Trigger.Value));
            if (config.Attributes.HasValue)
                shortTuples.Add(GetTuple(AVS_TKN_PPLCFG.ATTRIBUTES_U16, config.Attributes.Value));
            if (config.Priority.HasValue)
                byteTuples.Add(GetTuple(AVS_TKN_PPLCFG.PRIORITY_U8, config.Priority.Value));
            if (config.LowPower.HasValue)
                boolTuples.Add(GetTuple(AVS_TKN_PPLCFG.LOW_POWER_BOOL, config.LowPower.Value));

            var words = new VendorTuples<uint>();
            words.Tuples = wordTuples.ToArray();

            var shorts = new VendorTuples<ushort>();
            shorts.Tuples = shortTuples.ToArray();

            var bytes = new VendorTuples<byte>();
            bytes.Tuples = byteTuples.ToArray();

            var bools = new VendorTuples<bool>();
            bools.Tuples = boolTuples.ToArray();

            var section = new SectionVendorTuples($"pplcfg{id}_tuples");
            section.Tokens = "avs_pplcfg_tokens";
            section.Tuples = new VendorTuples[] { words, shorts, bytes, bools };

            return section;
        }

        public static IEnumerable<Section> GetPipelineConfigsSections(PipelineConfig[] configs)
        {
            var sections = new List<Section>();

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_MANIFEST.NUM_PPLCFGS_U32, (uint)configs.Length),
            };

            var tuples = new SectionVendorTuples("pplcfg_hdr_tuples");
            tuples.Tokens = "avs_manifest_tokens";
            tuples.Tuples = new VendorTuples[] { words };
            sections.Add(tuples);

            for (int i = 0; i < configs.Length; i++)
                sections.Add(GetPipelineConfigSection(configs[i], i));

            // create private section referencing all added entries
            var data = new SectionData("pplcfg_data");
            data.Tuples = sections.Select(s => s.Identifier).ToArray();
            sections.Add(data);

            return sections;
        }

        public static Section GetBindingSection(Binding binding, int id)
        {
            var strings = new VendorTuples<string>();
            strings.Tuples = new[]
            {
                GetTuple(AVS_TKN_BINDING.TARGET_TPLG_NAME_STRING, binding.TargetTopologyName),
            };

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_BINDING.ID_U32, binding.Id),
                GetTuple(AVS_TKN_BINDING.TARGET_PATH_TMPL_ID_U32, binding.TargetPathTemplateId),
                GetTuple(AVS_TKN_BINDING.TARGET_PPL_ID_U32, binding.TargetPipelineId),
                GetTuple(AVS_TKN_BINDING.TARGET_MOD_ID_U32, binding.TargetModuleId),
                GetTuple(AVS_TKN_BINDING.MOD_ID_U32, binding.ModuleId),
            };

            var bytes = new VendorTuples<byte>();
            bytes.Tuples = new[]
            {
                GetTuple(AVS_TKN_BINDING.TARGET_MOD_PIN_U8, binding.TargetModulePin),
                GetTuple(AVS_TKN_BINDING.MOD_PIN_U8, binding.ModulePin),
                GetTuple(AVS_TKN_BINDING.IS_SINK_U8, Convert.ToByte(binding.IsSink)),
            };

            var section = new SectionVendorTuples($"binding{id}_tuples");
            section.Tokens = "avs_binding_tokens";
            section.Tuples = new VendorTuples[] { words, strings, bytes };

            return section;
        }

        public static IEnumerable<Section> GetBindingsSections(Binding[] bindings)
        {
            var sections = new List<Section>();

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_MANIFEST.NUM_BINDINGS_U32, (uint)bindings.Length),
            };

            var tuples = new SectionVendorTuples("binding_hdr_tuples");
            tuples.Tokens = "avs_manifest_tokens";
            tuples.Tuples = new VendorTuples[] { words };
            sections.Add(tuples);

            for (int i = 0; i < bindings.Length; i++)
                sections.Add(GetBindingSection(bindings[i], i));

            // create private section referencing all added entries
            var data = new SectionData("binding_data");
            data.Tuples = sections.Select(s => s.Identifier).ToArray();
            sections.Add(data);

            return sections;
        }

        public static Section GetModuleSection(Module module, string namePrefix, uint id)
        {
            var wordTuples = new List<Tuple<string, uint>>
            {
                GetTuple(AVS_TKN_MOD.ID_U32, module.Id),
                GetTuple(AVS_TKN_MOD.MODCFG_BASE_ID_U32, module.ConfigBaseId),
                GetTuple(AVS_TKN_MOD.IN_AFMT_ID_U32, module.InAudioFormatId),
                GetTuple(AVS_TKN_MOD.MODCFG_EXT_ID_U32, module.ConfigExtId),
            };

            var byteTuples = new List<Tuple<string, byte>>();

            if (module.CoreId.HasValue)
                byteTuples.Add(GetTuple(AVS_TKN_MOD.CORE_ID_U8, module.CoreId.Value));
            if (module.ProcessingDomain.HasValue)
                byteTuples.Add(GetTuple(AVS_TKN_MOD.PROC_DOMAIN_U8, module.ProcessingDomain.Value));
            if (module.KcontrolId.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MOD.KCONTROL_ID_U32, module.KcontrolId.Value));
            if (module.InitConfigIds != null)
                wordTuples.Add(GetTuple(AVS_TKN_MOD.INIT_CONFIG_NUM_IDS_U32, (uint)module.InitConfigIds.Length));

            var words = new VendorTuples<uint>();
            words.Tuples = wordTuples.ToArray();

            var bytes = new VendorTuples<byte>();
            bytes.Tuples = byteTuples.ToArray();

            var section = new SectionVendorTuples($"{namePrefix}_mod{id}_tuples");
            section.Tokens = "avs_module_tokens";
            section.Tuples = new VendorTuples[] { words, bytes };

            return section;
        }

        public static Section GetModuleInitConfigSection(Module module, string namePrefix, uint id, uint config_id)
        {
            var wordTuples = new List<Tuple<string, uint>>
            {
                GetTuple(AVS_TKN_MOD.INIT_CONFIG_ID_U32, module.InitConfigIds[config_id]),
            };

            var words = new VendorTuples<uint>();
            words.Tuples = wordTuples.ToArray();

            var section = new SectionVendorTuples($"{namePrefix}_mod{id}_config{config_id}_tuples");
            section.Tokens = "avs_module_tokens";
            section.Tuples = new VendorTuples[] { words };

            return section;
        }

        public static Section GetBindingIdSection(uint bindid, string namePrefix, uint id)
        {
            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_PPL.BINDING_ID_U32, bindid),
            };

            var section = new SectionVendorTuples($"{namePrefix}_bindid{id}_tuples");
            section.Tokens = "avs_pipeline_tokens";
            section.Tuples = new VendorTuples[] { words };

            return section;
        }

        public static IEnumerable<Section> GetPipelineSections(Pipeline ppl, string namePrefix, int id)
        {
            var sections = new List<Section>();
            string identifier = $"{namePrefix}_ppl{id}";
            int numBindings = (ppl.BindingId == null) ? 0 : ppl.BindingId.Length;

            if (ppl.Modules != null)
            {
                for (uint i = 0; i < ppl.Modules.Length; i++)
                {
                    sections.Add(GetModuleSection(ppl.Modules[i], identifier, i));
                    if (ppl.Modules[i].InitConfigIds != null)
                        for (uint j = 0; j < ppl.Modules[i].InitConfigIds.Length; j++)
                            sections.Add(GetModuleInitConfigSection(ppl.Modules[i], identifier, i, j));
                }

                for (uint i = 0; i < numBindings; i++)
                    sections.Add(GetBindingIdSection(ppl.BindingId[i], identifier, i));
            }

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_PPL.ID_U32, ppl.Id),
                GetTuple(AVS_TKN_PPL.PPLCFG_ID_U32, ppl.ConfigId),
                GetTuple(AVS_TKN_PPL.NUM_BINDING_IDS_U32, (uint)numBindings),
            };

            var tuples = new SectionVendorTuples($"{identifier}_tuples");
            tuples.Tokens = "avs_pipeline_tokens";
            tuples.Tuples = new VendorTuples[] { words };
            // ensure main section prepends child sections
            sections.Insert(0, tuples);

            return sections;
        }

        public static IEnumerable<Section> GetPathSections(Path path, string namePrefix, int id)
        {
            var sections = new List<Section>();
            string identifier = $"{namePrefix}_path{id}";

            if (path.Pipelines != null)
                for (int i = 0; i < path.Pipelines.Length; i++)
                    sections.AddRange(GetPipelineSections(path.Pipelines[i], identifier, i));

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_PATH.ID_U32, path.Id),
                GetTuple(AVS_TKN_PATH.FE_AFMT_ID_U32, path.FEAudioFormatId),
                GetTuple(AVS_TKN_PATH.BE_AFMT_ID_U32, path.BEAudioFormatId),
            };

            var tuples = new SectionVendorTuples($"{identifier}_tuples");
            tuples.Tokens = "avs_path_tokens";
            tuples.Tuples = new VendorTuples[] { words };
            // ensure main section prepends child sections
            sections.Insert(0, tuples);

            return sections;
        }

        public static IEnumerable<Section> GetPathTemplateSections(PathTemplate template, int id)
        {
            var sections = new List<Section>();
            string identifier = $"path_tmpl{id}";

            for (int i = 0; i < template.Paths.Length; i++)
                sections.AddRange(GetPathSections(template.Paths[i], identifier, i));

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_PATH_TMPL.ID_U32, template.Id),
            };

            var strings = new VendorTuples<string>();
            strings.Tuples = new[]
            {
                GetTuple(AVS_TKN_PATH_TMPL.DAI_WNAME_STRING, template.WidgetName),
            };

            var tuples = new SectionVendorTuples($"{identifier}_tuples");
            tuples.Tokens = "avs_path_template_tokens";
            tuples.Tuples = new VendorTuples[] { words, strings };
            // ensure main section prepends child sections
            sections.Insert(0, tuples);

            // create private section listing all template components
            var data = new SectionData($"{identifier}_data");
            data.Tuples = sections.Select(s => s.Identifier).ToArray();
            sections.Add(data);

            var widget = new SectionWidget(template.WidgetName);
            widget.NoPm = true;
            widget.IgnoreSuspend = template.IgnoreSuspend;
            widget.Data = new string[] { data.Identifier };

            if (template.Kcontrol == null || template.Kcontrol.Name == null)
            {
                widget.Type = TPLG_DAPM.SCHEDULER;
            }
            else
            {
                // ASoC does not create kcontrols for widgets of type SCHEDULER.
                widget.Type = TPLG_DAPM.PGA;

                List<string> mixerSettings = new List<string> { template.Kcontrol.Name };
                if (template.MuteKcontrol != null && template.MuteKcontrol.Name != null)
                    mixerSettings.Add(template.MuteKcontrol.Name);
                widget.Mixer = mixerSettings.ToArray();
                sections.AddRange(GetKcontrolMixerSections(template.Kcontrol));
                if (template.MuteKcontrol != null && template.MuteKcontrol.Name != null)
                    sections.AddRange(GetKcontrolMixerSections(template.MuteKcontrol));
            }

            sections.Add(widget);

            return sections;
        }

        public static IEnumerable<Section> GetCondpathSections(Condpath path, string namePrefix, int id)
        {
            var sections = new List<Section>();
            string identifier = $"{namePrefix}_condpath{id}";

            if (path.Pipelines != null)
                for (int i = 0; i < path.Pipelines.Length; i++)
                    sections.AddRange(GetPipelineSections(path.Pipelines[i], identifier, i));

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_CONDPATH.ID_U32, path.Id),
                GetTuple(AVS_TKN_CONDPATH.SOURCE_PATH_ID_U32, path.SourcePathId),
                GetTuple(AVS_TKN_CONDPATH.SINK_PATH_ID_U32, path.SinkPathId),
            };

            var tuples = new SectionVendorTuples($"{identifier}_tuples");
            tuples.Tokens = "avs_condpath_tokens";
            tuples.Tuples = new VendorTuples[] { words };
            // ensure main section prepends child sections
            sections.Insert(0, tuples);

            return sections;
        }

        public static IEnumerable<Section> GetCondpathTemplateSections(CondpathTemplate template, int id)
        {
            var sections = new List<Section>();
            string identifier = $"condpath_tmpl{id}";

            for (int i = 0; i < template.Condpaths.Length; i++)
                sections.AddRange(GetCondpathSections(template.Condpaths[i], identifier, i));

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_CONDPATH_TMPL.ID_U32, template.Id),
                GetTuple(AVS_TKN_CONDPATH_TMPL.SOURCE_PATH_TMPL_ID_U32, template.SourcePathTemplateId),
                GetTuple(AVS_TKN_CONDPATH_TMPL.SINK_PATH_TMPL_ID_U32, template.SinkPathTemplateId),
                GetTuple(AVS_TKN_CONDPATH_TMPL.COND_TYPE_U32, template.ConditionType),
            };

            var strings = new VendorTuples<string>();
            strings.Tuples = new[]
            {
                GetTuple(AVS_TKN_CONDPATH_TMPL.SOURCE_TPLG_NAME_STRING, template.SourceTopologyName),
                GetTuple(AVS_TKN_CONDPATH_TMPL.SINK_TPLG_NAME_STRING, template.SinkTopologyName),
            };

            var bytes = new VendorTuples<byte>();
            bytes.Tuples = new[]
            {
                GetTuple(AVS_TKN_CONDPATH_TMPL.PRIORITY_U8, template.Priority),
            };

            var bools = new VendorTuples<bool>();
            bools.Tuples = new[]
            {
                GetTuple(AVS_TKN_CONDPATH_TMPL.OVERRIDABLE_BOOL, template.Overridable),
            };

            var tuples = new SectionVendorTuples($"{identifier}_tuples");
            tuples.Tokens = "avs_condpath_template_tokens";
            tuples.Tuples = new VendorTuples[] { words, strings, bytes, bools };
            // ensure main section prepends child sections
            sections.Insert(0, tuples);

            return sections;
        }

        public static IEnumerable<Section> GetCondpathTemplatesSections(CondpathTemplate[] templates)
        {
            var sections = new List<Section>();
            int length = (templates != null) ? templates.Length : 0;

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_MANIFEST.NUM_CONDPATH_TMPLS_U32, (uint)length),
            };

            var tuples = new SectionVendorTuples("condpath_hdr_tuples");
            tuples.Tokens = "avs_manifest_tokens";
            tuples.Tuples = new VendorTuples[] { words };
            sections.Add(tuples);

            for (int i = 0; i < length; i++)
                sections.AddRange(GetCondpathTemplateSections(templates[i], i));

            // create private section referencing all added entries
            var data = new SectionData("condpath_data");
            data.Tuples = sections.Select(s => s.Identifier).ToArray();
            sections.Add(data);

            return sections;
        }

        public static SectionPCMCapabilities GetPCMCapabilitiesSection(PCMCapabilities caps, string identifier)
        {
            uint[] formats = caps.Formats.ToUInts32();
            uint[] rates = caps.Rates.ToUInts32();
            uint[] channels = caps.Channels.ToUInts32();

            var section = new SectionPCMCapabilities(identifier);
            section.Formats.UnionWith(formats.Select(f => f.ToFormat()));
            section.Rates.UnionWith(rates.Select(s => s.ToRate()));
            section.ChannelsMax = channels.Max();
            section.ChannelsMin = channels.Min();
            section.SigBits = caps.SigBits;

            return section;
        }

        public static IEnumerable<Section> GetFEDAISections(FEDAI fedai)
        {
            var sections = new List<Section>();
            string identifier;

            var pcm = new SectionPCM(fedai.Name);
            pcm.IgnoreSuspend = fedai.IgnoreSuspend;
            if (fedai.CaptureCapabilities != null)
            {
                identifier = $"{fedai.Name}-capture";
                pcm.Capture = new PCMStream("capture");
                pcm.Capture.Capabilities = identifier;
                sections.Add(GetPCMCapabilitiesSection(fedai.CaptureCapabilities, identifier));
            }

            if (fedai.PlaybackCapabilities != null)
            {
                identifier = $"{fedai.Name}-playback";
                pcm.Playback = new PCMStream("playback");
                pcm.Playback.Capabilities = identifier;
                sections.Add(GetPCMCapabilitiesSection(fedai.PlaybackCapabilities, identifier));
            }

            pcm.DAI = new FE_DAI($"{fedai.Name}-dai");
            sections.Add(pcm);

            return sections;
        }

        public static Section GetDAPMGraphSection(DAPMGraph graph)
        {
            var section = new SectionGraph(graph.Name);
            section.Lines = graph.Routes.Select(
                r => $"{r.Sink}, {r.Control}, {r.Source}").ToArray();

            return section;
        }

        public static IEnumerable<Section> GetKcontrolMixerSections(Kcontrol kctrl)
        {
            var sections = new List<Section>();

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_KCONTROL.ID_U32, kctrl.Id),
            };

            var section = new SectionVendorTuples($"kctrl_{kctrl.Name}_tokens");
            section.Tokens = "avs_kcontrol_tokens";
            section.Tuples = new VendorTuples[] { words };
            sections.Add(section);

            var data = new SectionData($"kctrl_{kctrl.Name}_data");
            data.Tuples = new[] { section.Identifier };
            sections.Add(data);

            var control = new SectionControlMixer(kctrl.Name);
            // TODO: replace hardcodes below with descriptive constants
            control.Max = kctrl.max;
            if (kctrl.Name.Contains("Volume"))
            {
                control.Ops = new Ops("ctl")
                {
                    Get = 257,
                    Put = 257,
                    Info = TPLG_CTL.VOLSW,
                };
            }
            else if (kctrl.Name.Contains("Switch"))
            {
                control.Ops = new Ops("ctl")
                {
                    Get = 258,
                    Put = 258,
                    Info = TPLG_CTL.VOLSW,
                };
            }
            control.Access = new[]
            {
                CTL_ELEM_ACCESS.READWRITE,
                CTL_ELEM_ACCESS.VOLATILE,
            };
            control.Data = data.Identifier;
            sections.Add(control);

            return sections;
        }

        public static IEnumerable<Section> GetKcontrolBytesSections(Kcontrol kctrl)
        {
            var sections = new List<Section>();

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_KCONTROL.ID_U32, kctrl.Id),
            };

            var section = new SectionVendorTuples($"kctrl_{kctrl.Name}_tokens");
            section.Tokens = "avs_kcontrol_tokens";
            section.Tuples = new VendorTuples[] { words };
            sections.Add(section);

            var data = new SectionData($"kctrl_{kctrl.Name}_data");
            data.Tuples = new[] { section.Identifier };
            sections.Add(data);

            var control = new SectionControlBytes(kctrl.Name);
            // TODO: replace hardcodes below with descriptive constants
            control.Max = 0x100000;
            control.Ops = new Ops("ctl") { Info = TPLG_CTL.BYTES };
            control.ExtOps = new Ops("ctl") { Put = 256 };
            control.Access = new[]
            {
                CTL_ELEM_ACCESS.TLV_WRITE,
                CTL_ELEM_ACCESS.TLV_CALLBACK,
            };
            control.Data = data.Identifier;
            sections.Add(control);

            return sections;
        }

        public static IEnumerable<Section> GetKcontrolSections(Kcontrol kctrl)
        {
            switch (kctrl.Type)
            {
                case KcontrolType.Mixer:
                    return GetKcontrolMixerSections(kctrl);
                case KcontrolType.Bytes:
                    return GetKcontrolBytesSections(kctrl);
                case KcontrolType.Enum:
                default:
                    return Enumerable.Empty<Section>();
            }
        }

        public static IEnumerable<Section> GetTopologySections(Topology topology)
        {
            var sections = new List<Section>();

            sections.AddRange(GetAllSectionTokens());

            // append topology header
            var strings = new VendorTuples<string>();
            strings.Tuples = new[]
            {
                GetTuple(AVS_TKN_MANIFEST.NAME_STRING, topology.Name),
            };

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_MANIFEST.VERSION_U32, topology.Version),
            };

            var tuples = new SectionVendorTuples("manifest_hdr_tuples");
            tuples.Tokens = "avs_manifest_tokens";
            tuples.Tuples = new VendorTuples[] { strings, words };
            sections.Add(tuples);

            var data = new SectionData("manifest_hdr_data");
            data.Tuples = new string[] { tuples.Identifier };
            sections.Add(data);

            // then all other components topology consists of
            sections.AddRange(GetLibrariesSections(topology.Libraries));
            sections.AddRange(GetAudioFormatsSections(topology.AudioFormats));
            sections.AddRange(GetModuleConfigsBaseSections(topology.ModuleConfigsBase));
            sections.AddRange(GetModuleConfigsExtSections(topology.ModuleConfigsExt));
            sections.AddRange(GetPipelineConfigsSections(topology.PipelineConfigs));
            sections.AddRange(GetBindingsSections(topology.Bindings));
            sections.AddRange(GetCondpathTemplatesSections(topology.CondpathTemplates));
            if (topology.ModuleInitConfigs != null)
                sections.AddRange(GetModuleInitConfigsSections(topology.ModuleInitConfigs));

            var manifest = new SectionManifest("avs_manifest");
            // Manifest should not reference any SectionData that is already
            // owned by other section e.g.: SectionWidget. Assign Data here
            // before any widgets are added by GetPathTemplateSections
            manifest.Data = sections.OfType<SectionData>().Select(s => s.Identifier).ToArray();
            sections.Add(manifest);

            for (int i = 0; i < topology.PathTemplates.Length; i++)
                sections.AddRange(GetPathTemplateSections(topology.PathTemplates[i], i));

            for (int i = 0; i < topology.FEDAIs.Length; i++)
                sections.AddRange(GetFEDAISections(topology.FEDAIs[i]));

            for (int i = 0; i < topology.Graphs.Length; i++)
                sections.Add(GetDAPMGraphSection(topology.Graphs[i]));

            for (int i = 0; i < topology.Kcontrols?.Length; i++)
                sections.AddRange(GetKcontrolSections(topology.Kcontrols[i]));

            return sections;
        }
    }
}
