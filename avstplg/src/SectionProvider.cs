using NUcmSerializer;
using System;
using System.Collections.Generic;
using System.Linq;

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
            var result = new List<Section>();

            result.Add(GetSectionTokens<AVS_TKN_PIPELINE>("avs_pipeline_impl_tokens"));
            result.Add(GetSectionTokens<AVS_TKN_PIPE>("avs_pipeline_tokens"));
            result.Add(GetSectionTokens<AVS_TKN_AFMT>("avs_audio_format_tokens"));
            result.Add(GetSectionTokens<AVS_TKN_MODCFG_BASE>("avs_mod_cfg_base_tokens"));
            result.Add(GetSectionTokens<AVS_TKN_MODCFG>("avs_mod_cfg_tokens"));
            result.Add(GetSectionTokens<AVS_TKN_ROUTE>("avs_route_tokens"));
            result.Add(GetSectionTokens<AVS_TKN_PATH>("avs_path_tokens"));
            result.Add(GetSectionTokens<AVS_TKN_PATH_TMPL>("avs_path_template_tokens"));
            result.Add(GetSectionTokens<AVS_TKN_CONDPATH>("avs_condpath_tokens"));
            result.Add(GetSectionTokens<AVS_TKN_CONDPATH_TMPL>("avs_condpath_template_tokens"));
            result.Add(GetSectionTokens<AVS_TKN_LIBRARY>("avs_library_tokens"));
            result.Add(GetSectionTokens<AVS_TKN_TPLG>("avs_tplg_core_tokens"));

            return result;
        }

        public static Section GetLibrarySection(Library library, int id)
        {
            var strings = new VendorTuples<string>();
            strings.Tuples = new[]
            {
                GetTuple(AVS_TKN_LIBRARY.FILE_STRING, library.FileName),
                GetTuple(AVS_TKN_LIBRARY.FW_NAME_STRING, library.FwName),
            };

            var section = new SectionVendorTuples($"library{id}_tuples");
            section.Tokens = "avs_library_tokens";
            section.Tuples = new VendorTuples[] { strings };

            return section;
        }

        public static IEnumerable<Section> GetLibrariesSections(Library[] libraries)
        {
            var result = new List<Section>();

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_TPLG.NUM_LIBRARIES_U32, (uint)libraries.Length),
            };

            var tuples = new SectionVendorTuples("library_hdr_tuples");
            tuples.Tokens = "avs_tplg_core_tokens";
            tuples.Tuples = new[] { words };
            result.Add(tuples);

            for (int i = 0; i < libraries.Length; i++)
                result.Add(GetLibrarySection(libraries[i], i));

            // create private section referencing all added entries
            var data = new SectionData("library_data");
            data.Tuples = result.Select(s => s.Identifier).ToArray();
            result.Add(data);

            return result;
        }

        public static Section GetAudioFormatSection(AudioFormat format, int id)
        {
            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_AFMT.ID_U32, (uint)format.Id),
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
            var result = new List<Section>();

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_TPLG.NUM_AUDIO_FMTS_U32, (uint)formats.Length),
            };

            var tuples = new SectionVendorTuples("audio_format_hdr_tuples");
            tuples.Tokens = "avs_tplg_core_tokens";
            tuples.Tuples = new[] { words };
            result.Add(tuples);

            for (int i = 0; i < formats.Length; i++)
                result.Add(GetAudioFormatSection(formats[i], i));

            // create private section referencing all added entries
            var data = new SectionData("audio_format_data");
            data.Tuples = result.Select(s => s.Identifier).ToArray();
            result.Add(data);

            return result;
        }

        public static Section GetModuleConfigBaseSection(ModuleConfigBase config, int id)
        {
            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_MODCFG_BASE.ID_U32, (uint)config.Id),
                GetTuple(AVS_TKN_MODCFG_BASE.CPC_U32, config.Cpc),
                GetTuple(AVS_TKN_MODCFG_BASE.IBS_U32, config.Ibs),
                GetTuple(AVS_TKN_MODCFG_BASE.OBS_U32, config.Obs),
                GetTuple(AVS_TKN_MODCFG_BASE.PAGES_U32, config.Pages),
                GetTuple(AVS_TKN_MODCFG_BASE.AFMT_ID_U32, config.AudioFormatId),
            };

            var section = new SectionVendorTuples($"mod_cfg_base{id}_tuples");
            section.Tokens = "avs_mod_cfg_base_tokens";
            section.Tuples = new VendorTuples[] { words };

            return section;
        }

        public static IEnumerable<Section> GetModuleConfigsBaseSections(ModuleConfigBase[] configs)
        {
            var result = new List<Section>();

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_TPLG.NUM_BASE_CFGS_U32, (uint)configs.Length),
            };

            var tuples = new SectionVendorTuples("mod_cfg_base_hdr_tuples");
            tuples.Tokens = "avs_tplg_core_tokens";
            tuples.Tuples = new[] { words };
            result.Add(tuples);

            for (int i = 0; i < configs.Length; i++)
                result.Add(GetModuleConfigBaseSection(configs[i], i));

            // create private section referencing all added entries
            var data = new SectionData("mod_cfg_base_data");
            data.Tuples = result.Select(s => s.Identifier).ToArray();
            result.Add(data);

            return result;
        }

        public static Section GetModuleSection(Module module, string namePrefix, int id)
        {
            var uuids = new VendorTuples<Guid>();
            uuids.Tuples = new[]
            {
                GetTuple(AVS_TKN_MODCFG.TYPE_UUID, module.uuid),
            };

            var wordTuples = new List<Tuple<string, uint>>
            {
                GetTuple(AVS_TKN_MODCFG.OBJECT_ID_U32, (uint)module.ObjectId),
                GetTuple(AVS_TKN_MODCFG.BASE_ID_U32, module.ModuleConfigBaseId),
            };

            var byteTuples = new List<Tuple<string, byte>>
            {
                GetTuple(AVS_TKN_MODCFG.CORE_ID_U8, module.CoreId),
                GetTuple(AVS_TKN_MODCFG.PROC_DOMAIN_U8, module.ProcessingDomain),
            };

            // module-type specific tuples
            if (module.CprOutAudioFormatId.HasValue)
            wordTuples.Add(GetTuple(AVS_TKN_MODCFG.CPR_OUT_AFMT_ID_U32, module.CprOutAudioFormatId.Value));
            if (module.CprBlobFormatId.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.CPR_BLOB_FMT_ID_U32, module.CprBlobFormatId.Value));
            if (module.CprFeatureMask.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.CPR_FEATURE_MASK_U32, module.CprFeatureMask.Value));
            if (module.CprVirtualIndex != null)
                byteTuples.Add(GetTuple(AVS_TKN_MODCFG.CPR_VINDEX_U8, module.CprVirtualIndex.Value));
            if (module.CprDMAType != null)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.CPR_DMA_TYPE_U32, module.CprDMAType.Value));
            if (module.CprDMABufferSize != null)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.CPR_DMABUFF_SIZE_U32, module.CprDMABufferSize.Value));
            if (module.MicselOutAudioFormatId.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.MICSEL_OUT_AFMT_ID_U32, module.MicselOutAudioFormatId.Value));
            if (module.IntelWOVCpcLowPowerMode.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.INTELWOV_CPC_LOW_POWER_MODE_U32, module.IntelWOVCpcLowPowerMode.Value));
            if (module.SrcOutFrequency.HasValue)
                wordTuples.Add(GetTuple(AVS_TKN_MODCFG.SRC_OUT_FREQ_U32, module.SrcOutFrequency.Value));

            var words = new VendorTuples<uint>();
            words.Tuples = wordTuples.ToArray();

            var bytes = new VendorTuples<byte>();
            bytes.Tuples = byteTuples.ToArray();

            var section = new SectionVendorTuples($"{namePrefix}_mod{id}_tuples");
            section.Tokens = "avs_mod_cfg_tokens";
            section.Tuples = new VendorTuples[] { uuids, words, bytes };

            return section;
        }

        public static IEnumerable<Section> GetPipelineSections(Pipeline pipeline, int id)
        {
            var result = new List<Section>();
            string identifier = $"pipeline{id}";

            for (int i = 0; i < pipeline.Modules.Length; i++)
                result.Add(GetModuleSection(pipeline.Modules[i], identifier, i));

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_PIPELINE.ID_U32, (uint)pipeline.Id),
                GetTuple(AVS_TKN_PIPELINE.TRIGGER_U32, (uint)pipeline.Trigger),
            };

            var shorts = new VendorTuples<ushort>();
            shorts.Tuples = new[]
            {
                GetTuple(AVS_TKN_PIPELINE.REQ_SIZE_U16, pipeline.RequiredSize),
            };

            var bytes = new VendorTuples<byte>();
            bytes.Tuples = new[]
            {
                GetTuple(AVS_TKN_PIPELINE.PRIORITY_U8, pipeline.Priority),
            };

            var bools = new VendorTuples<bool>();
            bools.Tuples = new[]
            {
                GetTuple(AVS_TKN_PIPELINE.LOW_POWER_BOOL, pipeline.LowPower),
            };

            var section = new SectionVendorTuples($"{identifier}_tuples");
            section.Tokens = "avs_pipeline_impl_tokens";
            section.Tuples = new VendorTuples[] { words, shorts, bytes, bools };
            // ensure main section prepends child sections
            result.Insert(0, section);

            return result;
        }

        public static IEnumerable<Section> GetPipelinesSections(Pipeline[] pipelines)
        {
            var result = new List<Section>();

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_TPLG.NUM_PIPES_U32, (uint)pipelines.Length),
            };

            var tuples = new SectionVendorTuples("pipeline_hdr_tuples");
            tuples.Tokens = "avs_tplg_core_tokens";
            tuples.Tuples = new[] { words };
            result.Add(tuples);

            for (int i = 0; i < pipelines.Length; i++)
                result.AddRange(GetPipelineSections(pipelines[i], i));

            // create private section referencing all added entries
            var data = new SectionData("pipeline_data");
            data.Tuples = result.Select(s => s.Identifier).ToArray();
            result.Add(data);

            return result;
        }

        public static Section GetBindingSection(Binding binding, string namePrefix, int id)
        {
            var strings = new VendorTuples<string>();
            strings.Tuples = new[]
            {
                GetTuple(AVS_TKN_ROUTE.TARGET_TPLG_NAME_STRING, binding.TargetTopologyName),
            };

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_ROUTE.TARGET_PATH_OBJECT_ID_U32, binding.TargetPathObjId),
                GetTuple(AVS_TKN_ROUTE.TARGET_PPL_OBJECT_ID_U32, binding.TargetPipeObjId),
                GetTuple(AVS_TKN_ROUTE.TARGET_MOD_OBJECT_ID_U32, binding.TargetModuleObjId),
                GetTuple(AVS_TKN_ROUTE.MOD_OBJECT_ID_U32, binding.ModuleObjId),
            };

            var bytes = new VendorTuples<byte>();
            bytes.Tuples = new[]
            {
                GetTuple(AVS_TKN_ROUTE.TARGET_MOD_PIN_U8, binding.TargetModulePin),
                GetTuple(AVS_TKN_ROUTE.MOD_PIN_U8, binding.ModulePin),
                GetTuple(AVS_TKN_ROUTE.IS_SINK_U8, Convert.ToByte(binding.IsSink)),
            };

            var section = new SectionVendorTuples($"{namePrefix}_route{id}_tuples");
            section.Tokens = "avs_route_tokens";
            section.Tuples = new VendorTuples[] { strings, words, bytes };

            return section;
        }

        public static IEnumerable<Section> GetRouteSections(Route route, string namePrefix, int id)
        {
            var result = new List<Section>();
            string identifier = $"{namePrefix}_pipe{id}";

            if (route.Bindings != null)
                for (int i = 0; i < route.Bindings.Length; i++)
                    result.Add(GetBindingSection(route.Bindings[i], identifier, i));

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_PIPE.OBJECT_ID_U32, (uint)route.ObjectId),
                GetTuple(AVS_TKN_PIPE.IMPL_ID_U32, route.ImplementingPipelineId),
            };

            var section = new SectionVendorTuples($"{identifier}_tuples");
            section.Tokens = "avs_pipeline_tokens";
            section.Tuples = new VendorTuples[] { words };
            // ensure main section prepends child sections
            result.Insert(0, section);

            return result;
        }

        public static IEnumerable<Section> GetPathSections(Path path, string namePrefix, int id)
        {
            var result = new List<Section>();
            string identifier = $"{namePrefix}_path{id}";

            if (path.Routes != null)
                for (int i = 0; i < path.Routes.Length; i++)
                    result.AddRange(GetRouteSections(path.Routes[i], identifier, i));

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_PATH.VARIANT_ID_U32, (uint)path.VariantId),
                GetTuple(AVS_TKN_PATH.FE_FMT_ID_U32, path.feAudioFormatId),
                GetTuple(AVS_TKN_PATH.BE_FMT_ID_U32, path.beAudioFormatId),
            };

            var section = new SectionVendorTuples($"{identifier}_tuples");
            section.Tokens = "avs_path_tokens";
            section.Tuples = new VendorTuples[] { words };
            // ensure main section prepends child sections
            result.Insert(0, section);

            return result;
        }

        public static IEnumerable<Section> GetPathTemplateSections(PathTemplate template, int id)
        {
            var result = new List<Section>();
            string identifier = $"path_tmpl{id}";

            for (int i = 0; i < template.Paths.Length; i++)
                result.AddRange(GetPathSections(template.Paths[i], identifier, i));

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_PATH_TMPL.OBJECT_ID_U32, (uint)template.ObjectId),
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
            result.Insert(0, tuples);

            // create private section listing all template components
            var data = new SectionData($"{identifier}_data");
            data.Tuples = result.Select(s => s.Identifier).ToArray();
            result.Add(data);

            var widget = new SectionWidget(template.WidgetName);
            widget.Type = TPLG_DAPM.SCHEDULER;
            widget.NoPm = true;
            widget.Data = new[] { data.Identifier };
            result.Add(widget);

            return result;
        }

        public static IEnumerable<Section> GetCondpathSections(Condpath path, string namePrefix, int id)
        {
            var result = new List<Section>();
            string identifier = $"{namePrefix}_condpath{id}";

            if (path.Routes != null)
                for (int i = 0; i < path.Routes.Length; i++)
                    result.AddRange(GetRouteSections(path.Routes[i], identifier, i));

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_CONDPATH.VARIANT_ID_U32, (uint)path.VariantId),
                GetTuple(AVS_TKN_CONDPATH.SOURCE_VARIANT_ID_U32, path.SourceVariantId),
                GetTuple(AVS_TKN_CONDPATH.SINK_VARIANT_ID_U32, path.SinkVariantId),
            };

            var section = new SectionVendorTuples($"{identifier}_tuples");
            section.Tokens = "avs_condpath_tokens";
            section.Tuples = new VendorTuples[] { words };
            // ensure main section prepends child sections
            result.Insert(0, section);

            return result;
        }

        public static IEnumerable<Section> GetCondpathTemplateSections(CondpathTemplate template, int id)
        {
            var result = new List<Section>();
            string identifier = $"condpath_tmpl{id}";

            for (int i = 0; i < template.Condpaths.Length; i++)
                result.AddRange(GetCondpathSections(template.Condpaths[i], identifier, i));

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_CONDPATH_TMPL.OBJECT_ID_U32, (uint)template.ObjectId),
                GetTuple(AVS_TKN_CONDPATH_TMPL.SOURCE_PATH_OBJECT_ID_U32, template.SourcePathObjId),
                GetTuple(AVS_TKN_CONDPATH_TMPL.SINK_PATH_OBJECT_ID_U32, template.SinkPathObjId),
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
                GetTuple(AVS_TKN_CONDPATH_TMPL.OVERRIDDABLE_BOOL, template.Overriddable),
            };

            var tuples = new SectionVendorTuples($"{identifier}_tuples");
            tuples.Tokens = "avs_condpath_template_tokens";
            tuples.Tuples = new VendorTuples[] { words, strings, bytes, bools };
            // ensure main section prepends child sections
            result.Insert(0, tuples);

            // create private section listing all template components
            var data = new SectionData($"{identifier}_data");
            data.Tuples = result.Select(s => s.Identifier).ToArray();
            result.Add(data);

            return result;
        }

        public static IEnumerable<Section> GetCondpathTemplatesSections(CondpathTemplate[] templates)
        {
            var result = new List<Section>();
            int length = (templates != null) ? templates.Length : 0;

            var words = new VendorTuples<uint>();
            words.Tuples = new[]
            {
                GetTuple(AVS_TKN_TPLG.NUM_CONDPATH_TMPLS_U32, (uint)length),
            };

            var tuples = new SectionVendorTuples("condpath_hdr_tuples");
            tuples.Tokens = "avs_tplg_core_tokens";
            tuples.Tuples = new[] { words };
            result.Add(tuples);

            for (int i = 0; i < length; i++)
                result.AddRange(GetCondpathTemplateSections(templates[i], i));

            // create private section referencing all added entries
            var data = new SectionData("condpath_data");
            data.Tuples = result.Select(s => s.Identifier).ToArray();
            result.Add(data);

            return result;
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

            return section;
        }

        public static IEnumerable<Section> GetFEDAISections(FEDAI fedai)
        {
            var result = new List<Section>();
            string identifier;

            var section = new SectionPCM(fedai.Name);
            if (fedai.CaptureCapabilities != null)
            {
                identifier = $"{fedai.Name}-capture";
                section.Capture = new PCMStream("capture");
                section.Capture.Capabilities = identifier;
                result.Add(GetPCMCapabilitiesSection(fedai.CaptureCapabilities, identifier));
            }

            if (fedai.PlaybackCapabilities != null)
            {
                identifier = $"{fedai.Name}-playback";
                section.Playback = new PCMStream("playback");
                section.Playback.Capabilities = identifier;
                result.Add(GetPCMCapabilitiesSection(fedai.PlaybackCapabilities, identifier));
            }

            section.DAI = new FE_DAI($"{fedai.Name}-dai");
            result.Add(section);

            return result;
        }

        public static Section GetDAPMGraphSection(DAPMGraph graph)
        {
            var section = new SectionGraph(graph.Name);
            section.Lines = graph.Routes.Select(
                r => $"{r.Sink}, {r.Control}, {r.Source}").ToArray();

            return section;
        }

        public static IEnumerable<Section> GetTopologySections(Topology topology)
        {
            var result = new List<Section>();

            result.AddRange(GetAllSectionTokens());

            // append topology header
            var strings = new VendorTuples<string>();
            strings.Tuples = new[]
            {
                GetTuple(AVS_TKN_TPLG.NAME_STRING, topology.Name),
            };

            var tuples = new SectionVendorTuples("tplg_hdr_tuples");
            tuples.Tokens = "avs_tplg_core_tokens";
            tuples.Tuples = new[] { strings };
            result.Add(tuples);

            var data = new SectionData("tplg_hdr_data");
            data.Tuples = new[] { tuples.Identifier };
            result.Add(data);

            // then all other components topology consists of
            result.AddRange(GetLibrariesSections(topology.Libraries));
            result.AddRange(GetAudioFormatsSections(topology.AudioFormats));
            result.AddRange(GetModuleConfigsBaseSections(topology.ModuleConfigsBase));
            result.AddRange(GetPipelinesSections(topology.Pipelines));
            result.AddRange(GetCondpathTemplatesSections(topology.CondpathTemplates));

            var manifest = new SectionManifest("avs_manifest");
            // Manifest should not reference any SectionData that is already
            // owned by other section e.g.: SectionWidget. Assign Data here
            // before any widgets are added by GetPathTemplateSections
            manifest.Data = result.OfType<SectionData>().Select(s => s.Identifier).ToArray();
            result.Add(manifest);

            for (int i = 0; i < topology.PathTemplates.Length; i++)
                result.AddRange(GetPathTemplateSections(topology.PathTemplates[i], i));

            for (int i = 0; i < topology.FEDAIs.Length; i++)
                result.AddRange(GetFEDAISections(topology.FEDAIs[i]));

            for (int i = 0; i < topology.Graphs.Length; i++)
                result.Add(GetDAPMGraphSection(topology.Graphs[i]));

            return result;
        }
    }
}
