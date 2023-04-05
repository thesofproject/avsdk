using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using avstplg;
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
        public void TestGetAllSectionTokens()
        {
            IEnumerable<Section> sections = SectionProvider.GetAllSectionTokens();

            Assert.Contains(sections, (s) => s_comparer.Equals("avs_manifest_tokens", s.Identifier));
            Assert.Contains(sections, (s) => s_comparer.Equals("avs_library_tokens", s.Identifier));
            Assert.Contains(sections, (s) => s_comparer.Equals("avs_audio_format_tokens", s.Identifier));
            Assert.Contains(sections, (s) => s_comparer.Equals("avs_modcfg_base_tokens", s.Identifier));
            Assert.Contains(sections, (s) => s_comparer.Equals("avs_modcfg_ext_tokens", s.Identifier));
            Assert.Contains(sections, (s) => s_comparer.Equals("avs_pplcfg_tokens", s.Identifier));
            Assert.Contains(sections, (s) => s_comparer.Equals("avs_binding_tokens", s.Identifier));
            Assert.Contains(sections, (s) => s_comparer.Equals("avs_pipeline_tokens", s.Identifier));
            Assert.Contains(sections, (s) => s_comparer.Equals("avs_module_tokens", s.Identifier));
            Assert.Contains(sections, (s) => s_comparer.Equals("avs_path_template_tokens", s.Identifier));
            Assert.Contains(sections, (s) => s_comparer.Equals("avs_path_tokens", s.Identifier));
            Assert.Contains(sections, (s) => s_comparer.Equals("avs_condpath_template_tokens", s.Identifier));
            Assert.Contains(sections, (s) => s_comparer.Equals("avs_condpath_tokens", s.Identifier));
            Assert.Contains(sections, (s) => s_comparer.Equals("avs_pin_format_tokens", s.Identifier));
            Assert.Contains(sections, (s) => s_comparer.Equals("avs_kcontrol_tokens", s.Identifier));
        }

        [Fact]
        public void TestGetLibrarySection()
        {
            Library lib = new Library()
            {
                Id = 0,
                FileName = "MyLibrary.bin",
            };

            int id = 13;
            Section section = SectionProvider.GetLibrarySection(lib, id);

            Assert.NotNull(section);
            Assert.Equal<string>($"library{id}_tuples", section.Identifier, s_comparer);
        }

        [Fact]
        public void TestGetLibrariesSections()
        {
            Library[] libs = new Library[]
            {
        new Library() { Id = 0, FileName = "MyLibrary.bin", },
        new Library() { Id = 1, FileName = null, },
            };

            IEnumerable<Section> sections = SectionProvider.GetLibrariesSections(libs);

            Assert.NotEmpty(sections);
            Assert.Equal<int>(libs.Length + 2, sections.Count()); // array + hdr + priv_data
        }

        [Fact]
        public void TestGetAudioFormatSection()
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

            Assert.NotNull(section);
            Assert.Equal<string>($"audio_format{id}_tuples", section.Identifier, s_comparer);
        }

        [Fact]
        public void TestGetAudioFormatsSections()
        {
            AudioFormat[] fmts = new AudioFormat[]
            {
        new AudioFormat() { Id = 567, ValidBitDepth = 8, },
        new AudioFormat() { Id = 123, BitDepth = 8, },
            };

            IEnumerable<Section> sections = SectionProvider.GetAudioFormatsSections(fmts);

            Assert.NotEmpty(sections);
            Assert.Equal<int>(fmts.Length + 2, sections.Count()); // array + hdr + priv_data
        }

        [Fact]
        public void TestGetModuleConfigBaseSection()
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

            Assert.NotNull(section);
            Assert.Equal<string>($"modcfg_base{id}_tuples", section.Identifier, s_comparer);
        }

        [Fact]
        public void TestGetModuleConfigsBaseSections()
        {
            ModuleConfigBase[] cfgs = new ModuleConfigBase[]
            {
        new ModuleConfigBase() { Id = 543, Cpc = 30000, },
        new ModuleConfigBase() { Id = 345, Pages = 12, },
            };

            IEnumerable<Section> sections = SectionProvider.GetModuleConfigsBaseSections(cfgs);

            Assert.NotEmpty(sections);
            Assert.Equal<int>(cfgs.Length + 2, sections.Count()); // array + hdr + priv_data
        }

        [Fact]
        public void TestGetPinFormatSection()
        {
            IOPinFormat pin = new IOPinFormat()
            {
                IObs = 0xc0,
                AudioFormatId = 2,
            };

            string prefix = "io";
            uint id = 0xFF;
            Section section = SectionProvider.GetPinFormatSection(pin, prefix, id);

            Assert.NotNull(section);
            Assert.Equal<string>($"{prefix}pin{id}_tuples", section.Identifier, s_comparer);
        }

        [Fact]
        public void TestGetPinFormatsSections()
        {
            IOPinFormat[] pins = new IOPinFormat[]
            {
        new IOPinFormat() { IObs = 10000, },
        new IOPinFormat() { AudioFormatId = 21, },
            };

            IEnumerable<Section> sections = SectionProvider.GetPinFormatsSections(pins, string.Empty);

            Assert.NotEmpty(sections);
            Assert.Equal<int>(pins.Length, sections.Count());
        }

        [Fact]
        public void TestGetModuleConfigExtSection()
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
            };

            int id = 5561;
            Section section = SectionProvider.GetModuleConfigExtSection(cfg, id);

            Assert.NotNull(section);
            Assert.Equal<string>($"modcfg_ext{id}_tuples", section.Identifier, s_comparer);

            cfg.UpDownMixCoeff = new int[9];
            Assert.Throws<InvalidOperationException>(
            () => SectionProvider.GetModuleConfigExtSection(cfg, 0));
        }

        [Fact]
        public void TestGetModuleConfigsExtSections()
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

            Assert.NotEmpty(sections);
            Assert.Equal<int>(cfgs.Length + 3, sections.Count()); // array + inpin + hdr + priv_data
        }

        [Fact]
        public void TestGetPipelineConfigSection()
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

            Assert.NotNull(section);
            Assert.Equal<string>($"pplcfg{id}_tuples", section.Identifier, s_comparer);
        }

        [Fact]
        public void TestGetPipelineConfigsSections()
        {
            PipelineConfig[] cfgs = new PipelineConfig[]
            {
        new PipelineConfig() { Id = 0, },
        new PipelineConfig() { RequiredSize = 4, },
            };

            IEnumerable<Section> sections = SectionProvider.GetPipelineConfigsSections(cfgs);

            Assert.NotEmpty(sections);
            Assert.Equal<int>(cfgs.Length + 2, sections.Count()); // array + hdr + priv_data
        }

        [Fact]
        public void TestGetBindingSection()
        {
            Binding binding = new Binding()
            {
                Id = 0,
                ModuleId = 0x6,
            };

            int id = 0x44;
            Section section = SectionProvider.GetBindingSection(binding, id);

            Assert.NotNull(section);
            Assert.Equal<string>($"binding{id}_tuples", section.Identifier, s_comparer);
        }

        [Fact]
        public void TestGetBindingsSections()
        {
            Binding[] bindings = new Binding[]
            {
        new Binding() { TargetPipelineId = 3, },
        new Binding() { TargetModuleId = 9, },
            };

            IEnumerable<Section> sections = SectionProvider.GetBindingsSections(bindings);

            Assert.NotEmpty(sections);
            Assert.Equal<int>(bindings.Length + 2, sections.Count()); // array + hdr + priv_data
        }

        [Fact]
        public void TestGetModuleSection()
        {
            Module mod = new Module()
            {
                Id = 0,
                CoreId = 0x2,
                ProcessingDomain = 1,
                KcontrolId = 0,
            };

            string prefix = "ppl0";
            uint id = 2273;
            Section section = SectionProvider.GetModuleSection(mod, prefix, id);

            Assert.NotNull(section);
            Assert.Equal<string>($"{prefix}_mod{id}_tuples", section.Identifier, s_comparer);
        }

        [Fact]
        public void TestGetBindingIdSection()
        {
            string prefix = "ppl0";
            uint bindid = 2;
            uint id = 17;
            Section section = SectionProvider.GetBindingIdSection(bindid, prefix, id);

            Assert.NotNull(section);
            Assert.Equal<string>($"{prefix}_bindid{id}_tuples", section.Identifier, s_comparer);
        }

        [Fact]
        public void TestGetPipelineSections()
        {
            Pipeline ppl = new Pipeline()
            {
                Modules = new Module[]
        {
            new Module { Id = 0, },
        },
                BindingId = new uint[] { 66 },
            };

            string prefix = "path0";
            int id = 0;
            IEnumerable<Section> sections = SectionProvider.GetPipelineSections(ppl, prefix, id);

            Assert.NotEmpty(sections);
            Assert.Equal<int>(3, sections.Count()); // ppl + mod + bind
            Assert.Equal<string>($"{prefix}_ppl{id}_tuples", sections.ElementAt(0).Identifier, s_comparer);
        }

        [Fact]
        public void TestGetPathSections()
        {
            Path path = new Path()
            {
                Id = 0,
                Pipelines = new Pipeline[] { new Pipeline() },
            };

            string prefix = "tmpl0";
            int id = 0xAA;
            IEnumerable<Section> sections = SectionProvider.GetPathSections(path, prefix, id);

            Assert.NotEmpty(sections);
            Assert.Equal<int>(2, sections.Count()); // path + ppl
            Assert.Equal<string>($"{prefix}_path{id}_tuples", sections.ElementAt(0).Identifier, s_comparer);
        }

        [Fact]
        public void TestGetPathTemplateSections()
        {
            PathTemplate tmpl = new PathTemplate()
            {
                Id = 0,
                Paths = new Path[] { new Path() },
            };

            int id = 88552299;
            IEnumerable<Section> sections = SectionProvider.GetPathTemplateSections(tmpl, id);

            Assert.NotEmpty(sections);
            Assert.Equal<int>(4, sections.Count()); // tmpl + path + priv_data + widget
            Assert.Equal<string>($"path_tmpl{id}_tuples", sections.ElementAt(0).Identifier, s_comparer);
        }

        [Fact]
        public void TestGetCondpathSections()
        {
            Condpath path = new Condpath()
            {
                Id = 0,
                Pipelines = new Pipeline[] { new Pipeline() },
            };

            string prefix = "tmpl0";
            int id = 0xAA;
            IEnumerable<Section> sections = SectionProvider.GetCondpathSections(path, prefix, id);

            Assert.NotEmpty(sections);
            Assert.Equal<int>(2, sections.Count()); // path + ppl
            Assert.Equal<string>($"{prefix}_condpath{id}_tuples", sections.ElementAt(0).Identifier, s_comparer);
        }

        [Fact]
        public void TestGetCondpathTemplateSections()
        {
            CondpathTemplate tmpl = new CondpathTemplate()
            {
                Id = 0,
                Condpaths = new Condpath[] { new Condpath() },
            };

            int id = 88552299;
            IEnumerable<Section> sections = SectionProvider.GetCondpathTemplateSections(tmpl, id);

            Assert.NotEmpty(sections);
            Assert.Equal<int>(2, sections.Count()); // tmpl + path
            Assert.Equal<string>($"condpath_tmpl{id}_tuples", sections.ElementAt(0).Identifier, s_comparer);
        }

        [Fact]
        public void TestGetCondpathTemplatesSections()
        {
            CondpathTemplate[] tmpls = new CondpathTemplate[]
            {
        new CondpathTemplate()
        {
            Id = 0,
            Condpaths = new Condpath[] { new Condpath() },
        },
        new CondpathTemplate()
        {
            Condpaths = new Condpath[] { new Condpath() },
        }
            };

            IEnumerable<Section> sections = SectionProvider.GetCondpathTemplatesSections(tmpls);

            Assert.NotEmpty(sections);
            Assert.Equal<int>(6, sections.Count()); // hdr + 2x (tmpl + path) + priv_data
            Assert.Equal<string>($"condpath_hdr_tuples", sections.ElementAt(0).Identifier, s_comparer);

            sections = SectionProvider.GetCondpathTemplatesSections(null);
            Assert.NotEmpty(sections);
            Assert.Equal<int>(2, sections.Count()); // hdr + priv_data
        }

        [Fact]
        public void TestGetPCMCapabilitiesSection()
        {
            PCMCapabilities caps = new PCMCapabilities()
            {
                Formats = "16,32,",
                Rates = "5512, 11025",
                Channels = "8,2",
            };

            string id = "playback";
            Section section = SectionProvider.GetPCMCapabilitiesSection(caps, id);

            Assert.NotNull(section);
            Assert.Equal<string>(id, section.Identifier, s_comparer);
        }

        [Fact]
        public void TestGetFEDAISections()
        {
            FEDAI dai = new FEDAI()
            {
                Name = null,
                CaptureCapabilities = new PCMCapabilities(),
                PlaybackCapabilities = new PCMCapabilities(),
            };

            IEnumerable<Section> sections = SectionProvider.GetFEDAISections(dai);

            Assert.NotEmpty(sections);
            Assert.Equal<int>(3, sections.Count()); // dai + cp + pb
        }

        [Fact]
        public void TestGetDAPMGraphSection()
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

            Assert.NotNull(section);
            Assert.Equal<string>(graph.Name, section.Identifier, s_comparer);
        }

        [Fact]
        public void TestGetKcontrolSections()
        {
            IEnumerable<Section> sections;
            Kcontrol kctrl = new Kcontrol()
            {
                Id = 0,
                Name = null,
            };

            kctrl.Type = KcontrolType.Mixer;
            sections = SectionProvider.GetKcontrolSections(kctrl);
            Assert.NotEmpty(sections);
            Assert.Equal<int>(3, sections.Count()); // kctrl + priv_data + kctrl_bytes
            Assert.Equal<string>($"kctrl__tuples", sections.ElementAt(0).Identifier, s_comparer);

            kctrl.Type = KcontrolType.Bytes;
            sections = SectionProvider.GetKcontrolSections(kctrl);
            Assert.NotEmpty(sections);

            kctrl.Type = KcontrolType.Enum;
            sections = SectionProvider.GetKcontrolSections(kctrl);
            Assert.Empty(sections);
        }

        [Fact]
        public void TestGetTopologySections()
        {
            Topology tplg = new Topology()
            {
                Name = null,
                Libraries = new Library[] { },
                AudioFormats = new AudioFormat[] { },
                ModuleConfigsBase = new ModuleConfigBase[] { },
                ModuleConfigsExt = new ModuleConfigExt[] { },
                PipelineConfigs = new PipelineConfig[] { },
                Bindings = new Binding[] { },
                CondpathTemplates = new CondpathTemplate[] { },
                PathTemplates = new PathTemplate[]
                {
                    new PathTemplate()
                    {
                        Paths = new Path[] { },
                        WidgetName = "stub_widget",
                    },
                },
                FEDAIs = new FEDAI[]
                {
                    new FEDAI() { Name = "stub_dai" },
                },
                Graphs = new DAPMGraph[]
                {
                    new DAPMGraph() { Name = "stub_graph" },
                },
                Kcontrols = new Kcontrol[]
                {
                    new Kcontrol() { Name = "stub_kcontrol" },
                },
            };

            IEnumerable<Section> sections = SectionProvider.GetTopologySections(tplg);
            foreach (Section elem in sections)
            {
                Console.WriteLine(elem.Identifier);
            }

            Assert.NotEmpty(sections);
            Assert.Equal<int>(39, sections.Count());
            Assert.Contains(sections, (s) => s_comparer.Equals("manifest_hdr_tuples", s.Identifier));
        }
    }
}
