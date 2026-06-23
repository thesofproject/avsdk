//
// Copyright (c) 2026, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUcmSerializer;
using Xunit;

namespace avstplg.tests
{
    public class SectionProviderTests
    {
        private static StringComparer s_comparer;

        static SectionProviderTests()
        {
            s_comparer = StringComparer.Create(CultureInfo.CurrentCulture, false);
        }

        [Fact]
        public void GetAllSectionTokens()
        {
            IEnumerable<Section> sections = SectionProvider.GetAllSectionTokens();

            Assert.Single(sections, (s) => s_comparer.Equals("avs_manifest_tokens", s.Identifier));
            Assert.Single(sections, (s) => s_comparer.Equals("avs_library_tokens", s.Identifier));
            Assert.Single(sections, (s) => s_comparer.Equals("avs_audio_format_tokens", s.Identifier));
            Assert.Single(sections, (s) => s_comparer.Equals("avs_modcfg_base_tokens", s.Identifier));
            Assert.Single(sections, (s) => s_comparer.Equals("avs_modcfg_ext_tokens", s.Identifier));
            Assert.Single(sections, (s) => s_comparer.Equals("avs_pplcfg_tokens", s.Identifier));
            Assert.Single(sections, (s) => s_comparer.Equals("avs_binding_tokens", s.Identifier));
            Assert.Single(sections, (s) => s_comparer.Equals("avs_pipeline_tokens", s.Identifier));
            Assert.Single(sections, (s) => s_comparer.Equals("avs_module_tokens", s.Identifier));
            Assert.Single(sections, (s) => s_comparer.Equals("avs_path_template_tokens", s.Identifier));
            Assert.Single(sections, (s) => s_comparer.Equals("avs_path_tokens", s.Identifier));
            Assert.Single(sections, (s) => s_comparer.Equals("avs_condpath_template_tokens", s.Identifier));
            Assert.Single(sections, (s) => s_comparer.Equals("avs_condpath_tokens", s.Identifier));
            Assert.Single(sections, (s) => s_comparer.Equals("avs_pin_format_tokens", s.Identifier));
            Assert.Single(sections, (s) => s_comparer.Equals("avs_kcontrol_tokens", s.Identifier));
        }

        [Fact]
        public void GetLibrarySection()
        {
            Library lib = new Library()
            {
                Id = 0,
                FileName = "MyLibrary.bin",
            };

            int id = 13;
            Section section = SectionProvider.GetLibrarySection(lib, id);

            Assert.Equal($"library{id}_tuples", section.Identifier, s_comparer);
        }

        [Fact]
        public void GetLibrariesSections()
        {
            Library[] libs = new Library[]
            {
                new Library() { Id = 0, FileName = "MyLibrary.bin", },
                new Library() { Id = 1, FileName = null, },
            };

            IEnumerable<Section> sections = SectionProvider.GetLibrariesSections(libs);

            Assert.Equal(libs.Length + 2, sections.Count()); // lib(s) + hdr + priv_data
        }

        [Fact]
        public void GetAudioFormatSection()
        {
            AudioFormat fmt = new AudioFormat()
            {
                Id = 0,
                SampleRate = 48000,
                BitDepth = 32,
                ValidBitDepth = 24,
                NumChannels = 2,
            };

            int id = 5;
            Section section = SectionProvider.GetAudioFormatSection(fmt, id);

            Assert.Equal($"audio_format{id}_tuples", section.Identifier, s_comparer);
        }

        [Fact]
        public void GetAudioFormatsSections()
        {
            AudioFormat[] fmts = new AudioFormat[]
            {
                new AudioFormat() { Id = 567, ValidBitDepth = 8, },
                new AudioFormat() { Id = 123, BitDepth = 8, },
            };

            IEnumerable<Section> sections = SectionProvider.GetAudioFormatsSections(fmts);

            Assert.Equal(fmts.Length + 2, sections.Count()); // fmt(s) + hdr + priv_data
        }

        [Fact]
        public void GetModuleConfigBaseSection()
        {
            ModuleConfigBase cfg = new ModuleConfigBase()
            {
                Id = 0,
                Cpc = 12000,
                Ibs = 0xc0,
                Obs = 0xc0,
            };

            int id = 987;
            Section section = SectionProvider.GetModuleConfigBaseSection(cfg, id);

            Assert.Equal($"modcfg_base{id}_tuples", section.Identifier, s_comparer);
        }

        [Fact]
        public void GetModuleConfigsBaseSections()
        {
            ModuleConfigBase[] cfgs = new ModuleConfigBase[]
            {
                new ModuleConfigBase() { Id = 543, Cpc = 30000, },
                new ModuleConfigBase() { Id = 345, Pages = 12, },
            };

            IEnumerable<Section> sections = SectionProvider.GetModuleConfigsBaseSections(cfgs);

            Assert.Equal(cfgs.Length + 2, sections.Count()); // cfg(s) + hdr + priv_data
        }

        [Fact]
        public void GetPinFormatSection()
        {
            IOPinFormat pin = new IOPinFormat()
            {
                IObs = 0xc0,
                AudioFormatId = 2,
            };

            string prefix = "io";
            uint id = 0xFF;
            Section section = SectionProvider.GetPinFormatSection(pin, prefix, id);

            Assert.Equal($"{prefix}pin{id}_tuples", section.Identifier, s_comparer);
        }

        [Fact]
        public void GetPinFormatsSections()
        {
            IOPinFormat[] pins = new IOPinFormat[]
            {
                new IOPinFormat() { IObs = 10000, },
                new IOPinFormat() { AudioFormatId = 21, },
            };

            IEnumerable<Section> sections = SectionProvider.GetPinFormatsSections(pins, string.Empty);

            Assert.Equal(pins.Length, sections.Count());
        }

        [Fact]
        public void GetModuleConfigExtSection()
        {
            ModuleConfigExt cfg = new ModuleConfigExt()
            {
                Id = 0,
                CprOutAudioFormatId = 0,
                CprBlobFormatId = 0x1,
                CprFeatureMask = 0,
                CprVirtualIndex = 0x10,
                CprDMAType = "0xD",
                CprDMABufferSize = 0x300,
                MicselOutAudioFormatId = 0,
                IntelWOVCpcLowPowerMode = 0xc0,
                SrcOutFrequency = 44100,
                MuxRefAudioFormatId = 5,
                MuxOutAudioFormatId = 1,
                AecRefAudioFormatId = 5,
                AecOutAudioFormatId = 1,
                AecCpcLowPowerMode = 0x180,
                UpDownMixOutChanCfg = 0,
                UpDownMixCoeffSelect = 1,
                UpDownMixCoeff = new int[] { (int)AVS_TKN_MODCFG.UPDOWN_MIX_COEFF_5_S32 },
                UpDownMixChanMap = 90,
                ASrcOutFrequency = 16000,
                ASrcMode = 1,
                ASrcDisableJitterBuffer = 0,
                InPinFormats = new IOPinFormat[] { },
                OutPinFormats = new IOPinFormat[]
                {
                    new IOPinFormat() { IObs = 10000, },
                    new IOPinFormat() { AudioFormatId = 21, },
                },
                WhmRefAudioFormatId = 5,
                WhmOutAudioFormatId = 1,
                WhmBlobFormatId = 0x1,
                WhmWakeTickPeriod = 1,
                WhmVirtualIndex = 0x20,
                WhmDMAType = "0xC",
                WhmDMABufferSize = 0x600,
                PeakVolVolume = 0x7FFFFFFF,
                PeakVolCurveType = 1,
                PeakVolCurveDuration = 200,
            };

            int id = 5561;
            Section section = SectionProvider.GetModuleConfigExtSection(cfg, id);

            Assert.Equal($"modcfg_ext{id}_tuples", section.Identifier, s_comparer);

            // Test negative branches for InPinFormats and OutPinFormats
            Assert.NotNull(SectionProvider.GetModuleConfigExtSection(new ModuleConfigExt(), 0));
        }

        [Fact]
        public void GetModuleConfigsExtSections()
        {
            ModuleConfigExt[] cfgs = new ModuleConfigExt[]
            {
                new ModuleConfigExt()
                {
                    Id = 0,
                    InPinFormats = new IOPinFormat[]
                    {
                        new IOPinFormat() { IObs = 10000, },
                    },
                    OutPinFormats = new IOPinFormat[] { },
                }
            };

            IEnumerable<Section> sections = SectionProvider.GetModuleConfigsExtSections(cfgs);

            Assert.Equal(cfgs.Length + 3, sections.Count()); // cfg(s) + inpin + hdr + priv_data
        }

        [Fact]
        public void GetModuleInitConfigsSections()
        {
            ModuleInitConfig[] cfgs = new ModuleInitConfig[]
            {
                new ModuleInitConfig()
                {
                    Id = 0,
                    Param = 0x10,
                    Data = new byte[] { 0x01, 0x02, 0x03, 0x04 },
                },
                new ModuleInitConfig()
                {
                    Id = 1,
                    Param = 0x20,
                    Data = new byte[] { 0xAA, 0xBB },
                },
            };

            IEnumerable<Section> sections = SectionProvider.GetModuleInitConfigsSections(cfgs);

            // hdr + priv_data + cfg(s) * (hdr_tuples + hdr_data + data)
            Assert.Equal(2 + cfgs.Length * 3, sections.Count());
            Assert.Equal("module_init_config_tuples", sections.ElementAt(0).Identifier, s_comparer);
        }

        [Fact]
        public void GetNHLTConfigsSections()
        {
            NHLTConfig[] cfgs = new NHLTConfig[]
            {
                new NHLTConfig()
                {
                    Id = 0,
                    Data = new byte[] { 0x01, 0x02, 0x03, 0x04 },
                },
                new NHLTConfig()
                {
                    Id = 1,
                    Data = new byte[] { 0xAA, 0xBB, 0xCC },
                },
            };

            IEnumerable<Section> sections = SectionProvider.GetNHLTConfigsSections(cfgs);

            // hdr + priv_data + cfg(s) * (hdr_tuples + hdr_data + data)
            Assert.Equal(2 + cfgs.Length * 3, sections.Count());
            Assert.Equal("NHLT_config_tuples", sections.ElementAt(0).Identifier, s_comparer);
        }

        [Fact]
        public void GetPipelineConfigSection()
        {
            PipelineConfig cfg = new PipelineConfig()
            {
                Trigger = 0,
                Attributes = 0,
                Priority = 2,
                LowPower = true,
            };

            int id = 0xDE;
            Section section = SectionProvider.GetPipelineConfigSection(cfg, id);

            Assert.Equal($"pplcfg{id}_tuples", section.Identifier, s_comparer);
        }

        [Fact]
        public void GetPipelineConfigsSections()
        {
            PipelineConfig[] cfgs = new PipelineConfig[]
            {
                new PipelineConfig() { Id = 0, },
                new PipelineConfig() { RequiredSize = 4, },
            };

            IEnumerable<Section> sections = SectionProvider.GetPipelineConfigsSections(cfgs);

            Assert.Equal(cfgs.Length + 2, sections.Count()); // cfg(s) + hdr + priv_data
        }

        [Fact]
        public void GetBindingSection()
        {
            Binding binding = new Binding()
            {
                Id = 0,
                ModuleId = 0x6,
            };

            int id = 0x44;
            Section section = SectionProvider.GetBindingSection(binding, id);

            Assert.Equal($"binding{id}_tuples", section.Identifier, s_comparer);
        }

        [Fact]
        public void GetBindingsSections()
        {
            Binding[] bindings = new Binding[]
            {
                new Binding() { TargetPipelineId = 3, },
                new Binding() { TargetModuleId = 9, },
            };

            IEnumerable<Section> sections = SectionProvider.GetBindingsSections(bindings);

            Assert.Equal(bindings.Length + 2, sections.Count()); // binding(s) + hdr + priv_data
        }

        [Fact]
        public void GetModuleSection()
        {
            Module mod = new Module()
            {
                Id = 0,
                CoreId = 0x2,
                ProcessingDomain = 1,
                KcontrolId = 0,
                InitConfigIds = new uint[] { 1, 2 },
                NHLTConfigId = 4,
            };

            string prefix = "ppl0";
            uint id = 2273;
            Section section = SectionProvider.GetModuleSection(mod, prefix, id);

            Assert.Equal($"{prefix}_mod{id}_tuples", section.Identifier, s_comparer);

            // Test negative branches for InitConfigIds
            Assert.NotNull(SectionProvider.GetModuleSection(new Module(), "", 0));
        }

        [Fact]
        public void GetBindingIdSection()
        {
            string prefix = "ppl0";
            uint id = 17;
            Section section = SectionProvider.GetBindingIdSection(2, prefix, id);

            Assert.Equal($"{prefix}_bindid{id}_tuples", section.Identifier, s_comparer);
        }

        [Fact]
        public void GetPipelineSections()
        {
            Pipeline ppl = new Pipeline()
            {
                Modules = new Module[]
                {
                    new Module { Id = 0, InitConfigIds = new uint[] { 7 }, },
                },
                BindingId = new uint[] { 66 },
            };

            string prefix = "path0";
            int id = 0;
            IEnumerable<Section> sections = SectionProvider.GetPipelineSections(ppl, prefix, id);

            Assert.Equal(4, sections.Count()); // ppl + mod + mod_init_config + bind
            Assert.Equal($"{prefix}_ppl{id}_tuples", sections.ElementAt(0).Identifier, s_comparer);
        }

        [Fact]
        public void GetPathSections()
        {
            Path path = new Path()
            {
                Id = 0,
                Pipelines = new Pipeline[] { new Pipeline() },
            };

            string prefix = "tmpl0";
            int id = 0xAA;
            IEnumerable<Section> sections = SectionProvider.GetPathSections(path, prefix, id);

            Assert.Equal(2, sections.Count()); // path + ppl
            Assert.Equal($"{prefix}_path{id}_tuples", sections.ElementAt(0).Identifier, s_comparer);
        }

        [Fact]
        public void GetPathTemplateSections()
        {
            PathTemplate tmpl = new PathTemplate()
            {
                Id = 0,
                Paths = new Path[] { new Path() },
            };

            int id = 88552299;
            IEnumerable<Section> sections = SectionProvider.GetPathTemplateSections(tmpl, id);

            Assert.Equal($"path_tmpl{id}_tuples", sections.ElementAt(0).Identifier, s_comparer);
            Assert.Single(sections, (s) => s is SectionWidget);
            Assert.Equal(4, sections.Count());  // tmpl + path + priv_data + widget

            // tests the Kcontrol.Name == null branch
            tmpl.Kcontrol = new Kcontrol();
            sections = SectionProvider.GetPathTemplateSections(tmpl, 0);
            Assert.Equal(4, sections.Count());

            // tests the MuteKcontrol == null branch
            tmpl.Kcontrol = new Kcontrol() { Name = "vol" };
            sections = SectionProvider.GetPathTemplateSections(tmpl, 1);
            Assert.Equal(7, sections.Count());  // 4 + vendor_ctl + priv_data + alsa_ctl

            // tests the MuteKcontrol.Name == null branch
            tmpl.Kcontrol = new Kcontrol() { Name = "vol" };
            tmpl.MuteKcontrol = new Kcontrol();
            sections = SectionProvider.GetPathTemplateSections(tmpl, 1);
            Assert.Equal(7, sections.Count());  // 4 + vendor_ctl + priv_data + alsa_ctl

            tmpl.Kcontrol = new Kcontrol() { Name = "vol" };
            tmpl.MuteKcontrol = new Kcontrol() { Name = "mute" };
            sections = SectionProvider.GetPathTemplateSections(tmpl, 0);
            Assert.Equal(10, sections.Count()); // 4 + 2x (vendor_ctl + priv_data + alsa_ctl)
        }

        [Fact]
        public void GetCondpathSections()
        {
            Condpath path = new Condpath()
            {
                Id = 0,
                Pipelines = new Pipeline[] { new Pipeline() },
            };

            string prefix = "tmpl0";
            int id = 0xAA;
            IEnumerable<Section> sections = SectionProvider.GetCondpathSections(path, prefix, id);

            Assert.Equal(2, sections.Count()); // path + ppl
            Assert.Equal($"{prefix}_condpath{id}_tuples", sections.ElementAt(0).Identifier, s_comparer);
        }

        [Fact]
        public void GetCondpathTemplateSections()
        {
            CondpathTemplate tmpl = new CondpathTemplate()
            {
                Id = 0,
                Condpaths = new Condpath[] { new Condpath() },
            };

            int id = 88552299;
            IEnumerable<Section> sections = SectionProvider.GetCondpathTemplateSections(tmpl, id);

            Assert.Equal(2, sections.Count()); // tmpl + path
            Assert.Equal($"condpath_tmpl{id}_tuples", sections.ElementAt(0).Identifier, s_comparer);
        }

        [Fact]
        public void GetCondpathTemplatesSections()
        {
            CondpathTemplate[] tmpls = new CondpathTemplate[]
            {
                new CondpathTemplate() { Condpaths = new Condpath[] { new Condpath() }, },
                new CondpathTemplate() { Condpaths = new Condpath[] { new Condpath() }, },
            };

            IEnumerable<Section> sections = SectionProvider.GetCondpathTemplatesSections(tmpls);

            Assert.Equal(6, sections.Count()); // hdr + 2x (tmpl + path) + priv_data
            Assert.Equal($"condpath_hdr_tuples", sections.ElementAt(0).Identifier, s_comparer);

            sections = SectionProvider.GetCondpathTemplatesSections(null);
            Assert.Equal(2, sections.Count()); // hdr + priv_data
        }

        [Fact]
        public void GetPCMCapabilitiesSection()
        {
            PCMCapabilities caps = new PCMCapabilities()
            {
                Formats = "16,32,",
                Rates = "5512, 11025",
                Channels = "8,2",
            };

            string id = "playback";
            Section section = SectionProvider.GetPCMCapabilitiesSection(caps, id);

            Assert.Equal(id, section.Identifier, s_comparer);
        }

        [Fact]
        public void GetFEDAISections()
        {
            FEDAI dai = new FEDAI()
            {
                Name = null,
                CaptureCapabilities = new PCMCapabilities(),
                PlaybackCapabilities = new PCMCapabilities(),
            };

            IEnumerable<Section> sections = SectionProvider.GetFEDAISections(dai);

            Assert.Equal(3, sections.Count()); // dai + cp + pb
        }

        [Fact]
        public void GetDAPMGraphSection()
        {
            DAPMGraph graph = new DAPMGraph()
            {
                Name = "the_graph",
                Routes = new DAPMRoute[]
                {
                     new DAPMRoute() { Sink = "media0", Source = "codec0" },
                },
            };

            Section section = SectionProvider.GetDAPMGraphSection(graph);

            Assert.Equal(graph.Name, section.Identifier, s_comparer);
        }

        [Fact]
        public void GetKcontrolMixerSections()
        {
            Kcontrol ctl = new Kcontrol()
            {
                Id = 1,
                Name = null,
                Type = KcontrolType.Mixer,
                NumChannels = 2,
            };

            IEnumerable<Section> sections = SectionProvider.GetKcontrolMixerSections(ctl);

            Assert.Equal(3, sections.Count()); // vendor_ctl + priv_data + alsa_ctl
            Assert.Equal($"kctrl__tuples", sections.ElementAt(0).Identifier, s_comparer);

            ctl.Name = "Master Volume";
            sections = SectionProvider.GetKcontrolMixerSections(ctl);
            Assert.Single(sections, (s) => s_comparer.Equals("kctrl_Master Volume_tuples", s.Identifier));

            ctl.Name = "Master Switch";
            sections = SectionProvider.GetKcontrolMixerSections(ctl);
            Assert.Single(sections, (s) => s_comparer.Equals("kctrl_Master Switch_tuples", s.Identifier));
        }

        [Fact]
        public void GetKcontrolBytesSections()
        {
            Kcontrol ctl = new Kcontrol()
            {
                Name = "null",
                Type = KcontrolType.Bytes,
            };

            IEnumerable<Section> sections = SectionProvider.GetKcontrolBytesSections(ctl);

            Assert.Equal(3, sections.Count()); // vendor_ctl + priv_data + alsa_ctl
            Assert.Equal($"kctrl_null_tuples", sections.ElementAt(0).Identifier, s_comparer);
        }

        [Fact]
        public void GetKcontrolSections()
        {
            Kcontrol ctl = new Kcontrol();
            IEnumerable<Section> sections = SectionProvider.GetKcontrolSections(ctl);

            Assert.Empty(sections);

            ctl.Type = KcontrolType.Mixer;
            sections = SectionProvider.GetKcontrolSections(ctl);
            Assert.NotEmpty(sections);

            ctl.Type = KcontrolType.Bytes;
            sections = SectionProvider.GetKcontrolSections(ctl);
            Assert.NotEmpty(sections);

            ctl.Type = KcontrolType.Enum;
            sections = SectionProvider.GetKcontrolSections(ctl);
            Assert.Empty(sections);
        }

        [Fact]
        public void GetTopologySections()
        {
            Topology tplg = new Topology()
            {
                Name                = null,
                Libraries           = new Library[] { },
                AudioFormats        = new AudioFormat[] { },
                ModuleConfigsBase   = new ModuleConfigBase[] { },
                ModuleConfigsExt    = new ModuleConfigExt[] { },
                ModuleInitConfigs   = new ModuleInitConfig[]
                {
                    new ModuleInitConfig() { Param = 1, Data = new byte[] { 0 } },
                },
                NHLTConfigs         = new NHLTConfig[]
                {
                    new NHLTConfig() { Data = new byte[] { 3, 4 } },
                },
                PipelineConfigs     = new PipelineConfig[] { },
                Bindings            = new Binding[] { },
                CondpathTemplates   = new CondpathTemplate[] { },
                PathTemplates       = new PathTemplate[]
                {
                    new PathTemplate()
                    {
                        Paths = new Path[] { },
                        WidgetName = "stub_widget",
                    },
                },
                FEDAIs      = new FEDAI[] { new FEDAI() { Name = "stub_dai" }, },
                Graphs      = new DAPMGraph[] { new DAPMGraph() { Name = "stub_graph" }, },
                Kcontrols   = new Kcontrol[] { new Kcontrol() { Name = "stub_kcontrol" }, },
            };

            IEnumerable<Section> sections = SectionProvider.GetTopologySections(tplg);

            Assert.Equal(49, sections.Count());
            Assert.Single(sections, (s) => s_comparer.Equals("manifest_hdr_tuples", s.Identifier));
            Assert.Single(sections, (s) => s_comparer.Equals("module_init_config_tuples", s.Identifier));
            Assert.Single(sections, (s) => s_comparer.Equals("NHLT_config_tuples", s.Identifier));

            tplg.ModuleInitConfigs = null; // tests the mock-init-config branch
            sections = SectionProvider.GetTopologySections(tplg);

            Assert.Single(sections, (s) => s_comparer.Equals("module_init_config_tuples", s.Identifier));
            Assert.Single(sections, (s) => s_comparer.Equals("NHLT_config_tuples", s.Identifier));
        }
    }
}
