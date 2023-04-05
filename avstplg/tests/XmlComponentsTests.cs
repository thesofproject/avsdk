using System;
using System.Globalization;
using avstplg;
using Xunit;

namespace avstplg.tests
{
    public class XmlComponentsTests
    {
        private static StringComparer s_comparer;

        static XmlComponentsTests()
        {
            s_comparer = StringComparer.Create(CultureInfo.CurrentCulture, false);
        }

        [Fact]
        public void TestLibrary()
        {
            Library lib = new Library()
            {
                Id = 0,
                FileName = "MyLibrary.bin",
            };

            Assert.Equal<uint>(0u, lib.Id);
            Assert.Equal<string>("MyLibrary.bin", lib.FileName, s_comparer);
        }

        [Fact]
        public void TestAudioFormat()
        {
            AudioFormat fmt = new AudioFormat()
            {
                Id = 0,
                SampleRate = 48000,
                BitDepth = 32,
                ChannelConfig = 0,
                Interleaving = 0,
                NumChannels = 2,
                ValidBitDepth = 24,
                SampleType = 0,
            };

            Assert.Equal<uint>(0, fmt.Id);
            Assert.Equal<uint>(48000, fmt.SampleRate);
            Assert.Equal<uint>(32, fmt.BitDepth);
            Assert.Equal<uint>(0, fmt.ChannelConfig);
            Assert.Equal<uint>(0, fmt.Interleaving);
            Assert.Equal<uint>(2, fmt.NumChannels);
            Assert.Equal<uint>(24, fmt.ValidBitDepth);
            Assert.Equal<uint>(0, fmt.SampleType);

            fmt.ChannelMap = "0xFFFFFF10";
            Assert.Equal<string>("0xFFFFFF10", fmt.ChannelMap, s_comparer);
            Assert.NotEqual<string>("0x10", fmt.ChannelMap, s_comparer);

            fmt.ChannelMap = string.Empty;
            Assert.NotEqual<string>("0x0", fmt.ChannelMap, s_comparer);
            Assert.Equal<string>("0x00000000", fmt.ChannelMap, s_comparer);

            fmt.ChannelMap = null;
            Assert.NotEqual<string>("0x0", fmt.ChannelMap, s_comparer);
            Assert.Equal<string>("0x00000000", fmt.ChannelMap, s_comparer);
        }

        [Fact]
        public void TestModuleConfigBase()
        {
            ModuleConfigBase cfg = new ModuleConfigBase()
            {
                Id = 0,
                Cpc = 100000,
                Ibs = 0x180,
                Obs = 0x300,
                Pages = 1,
            };

            Assert.Equal<uint>(0, cfg.Id);
            Assert.Equal<uint>(100000, cfg.Cpc);
            Assert.Equal<uint>(0x180, cfg.Ibs);
            Assert.Equal<uint>(0x300, cfg.Obs);
            Assert.Equal<uint>(1, cfg.Pages);
        }

        [Fact]
        public void TestIOPinFormat()
        {
            IOPinFormat pin = new IOPinFormat()
            {
                IObs = 768,
                AudioFormatId = 8,
            };

            Assert.Equal<uint>(768, pin.IObs);
            Assert.Equal<uint>(8, pin.AudioFormatId);
        }

        [Fact]
        public void TestModuleConfigExt()
        {
            ModuleConfigExt cfg = new ModuleConfigExt()
            {
                Id = 0,
                CprOutAudioFormatId = 0,
                CprBlobFormatId = 0,
                CprFeatureMask = 0,
                CprVirtualIndex = 0,
                CprDMABufferSize = 0,
                MicselOutAudioFormatId = 0,
                IntelWOVCpcLowPowerMode = 0,
                SrcOutFrequency = 0,
                MuxRefAudioFormatId = 0,
                MuxOutAudioFormatId = 0,
                AecRefAudioFormatId = 0,
                AecOutAudioFormatId = 0,
                AecCpcLowPowerMode = 0,
                UpDownMixOutChanCfg = 0,
                UpDownMixCoeffSelect = 0,
                UpDownMixCoeff = new int[] { },
                UpDownMixChanMap = 0,
                ASrcOutFrequency = 0,
                ASrcMode = 0,
                ASrcDisableJitterBuffer = 0,
                InPinFormats = new IOPinFormat[] { },
                OutPinFormats = null,
                WhmRefAudioFormatId = 0,
                WhmOutAudioFormatId = 0,
                WhmBlobFormatId = 0,
                WhmWakeTickPeriod = 0,
                WhmVirtualIndex = 0,
                WhmDMABufferSize = 0,
            };

            Assert.Equal<uint>(0, cfg.Id);
            Assert.Equal<uint>(0, cfg.CprOutAudioFormatId.Value);
            Assert.Equal<uint>(0, cfg.CprBlobFormatId.Value);
            Assert.Equal<uint>(0, cfg.CprFeatureMask.Value);
            Assert.Equal<byte>(0, cfg.CprVirtualIndex.Value);
            Assert.Equal<uint>(0, cfg.CprDMABufferSize.Value);
            Assert.Equal<uint>(0, cfg.MicselOutAudioFormatId.Value);
            Assert.Equal<uint>(0, cfg.IntelWOVCpcLowPowerMode.Value);
            Assert.Equal<uint>(0, cfg.SrcOutFrequency.Value);
            Assert.Equal<uint>(0, cfg.MuxRefAudioFormatId.Value);
            Assert.Equal<uint>(0, cfg.MuxOutAudioFormatId.Value);
            Assert.Equal<uint>(0, cfg.AecRefAudioFormatId.Value);
            Assert.Equal<uint>(0, cfg.AecOutAudioFormatId.Value);
            Assert.Equal<uint>(0, cfg.AecCpcLowPowerMode.Value);
            Assert.Equal<uint>(0, cfg.UpDownMixOutChanCfg.Value);
            Assert.Equal<uint>(0, cfg.UpDownMixCoeffSelect.Value);
            Assert.Empty(cfg.UpDownMixCoeff);
            Assert.Equal<uint>(0, cfg.UpDownMixChanMap.Value);
            Assert.Equal<uint>(0, cfg.ASrcOutFrequency.Value);
            Assert.Equal<byte>(0, cfg.ASrcMode.Value);
            Assert.Equal<byte>(0, cfg.ASrcDisableJitterBuffer.Value);
            Assert.NotNull(cfg.InPinFormats);
            Assert.Null(cfg.OutPinFormats);
            Assert.Equal<uint>(0, cfg.WhmRefAudioFormatId.Value);
            Assert.Equal<uint>(0, cfg.WhmOutAudioFormatId.Value);
            Assert.Equal<uint>(0, cfg.WhmBlobFormatId.Value);
            Assert.Equal<uint>(0, cfg.WhmWakeTickPeriod.Value);
            Assert.Equal<byte>(0, cfg.WhmVirtualIndex.Value);
            Assert.Equal<uint>(0, cfg.WhmDMABufferSize.Value);

            cfg.UUID = Guid.Empty.ToString();
            Assert.Equal<string>(Guid.Empty.ToString(), cfg.UUID, s_comparer);
            cfg.UUID = null; // exception squelched
            Assert.Equal<string>(Guid.Empty.ToString(), cfg.UUID, s_comparer);

            Assert.Null(cfg.CprDMAType);
            cfg.CprDMAType = "0xa"; // shall be capitalized
            Assert.NotEqual<string>("0xa", cfg.CprDMAType, s_comparer);
            Assert.Equal<string>("0x0000000A", cfg.CprDMAType, s_comparer);
            cfg.CprDMAType = null; // value shall not change
            Assert.NotNull(cfg.CprDMAType);
            Assert.Equal<string>("0x0000000A", cfg.CprDMAType, s_comparer);

            Assert.Null(cfg.WhmDMAType);
            cfg.WhmDMAType = "0xa"; // shall be capitalized
            Assert.NotEqual<string>("0xa", cfg.WhmDMAType, s_comparer);
            Assert.Equal<string>("0x0000000A", cfg.WhmDMAType, s_comparer);
            cfg.WhmDMAType = null; // value shall not change
            Assert.NotNull(cfg.WhmDMAType);
            Assert.Equal<string>("0x0000000A", cfg.WhmDMAType, s_comparer);
        }

        [Fact]
        public void TestPipelineConfig()
        {
            PipelineConfig cfg = new PipelineConfig()
            {
                Id = 0,
                RequiredSize = 8,
                Priority = 0,
                LowPower = false,
                Attributes = 0,
                Trigger = 0,
            };

            Assert.Equal<uint>(0, cfg.Id);
            Assert.Equal<ushort>(8, cfg.RequiredSize);
            Assert.Equal<byte>(0, cfg.Priority.Value);
            Assert.False(cfg.LowPower);
            Assert.Equal<ushort>(0, cfg.Attributes.Value);
            Assert.Equal<uint>(0, cfg.Trigger.Value);
        }

        [Fact]
        public void TestBinding()
        {
            Binding binding = new Binding()
            {
                Id = 0,
                TargetTopologyName = "avs_hdaudio",
                TargetPathTemplateId = 2,
                TargetPipelineId = 9,
                TargetModuleId = 0xb,
                TargetModulePin = 0,
                ModuleId = 6,
                ModulePin = 3,
                IsSink = true,
            };

            Assert.Equal<uint>(0, binding.Id);
            Assert.Equal<string>("avs_hdaudio", binding.TargetTopologyName, s_comparer);
            Assert.Equal<uint>(2, binding.TargetPathTemplateId);
            Assert.Equal<uint>(9, binding.TargetPipelineId);
            Assert.Equal<uint>(0xb, binding.TargetModuleId);
            Assert.Equal<byte>(0, binding.TargetModulePin);
            Assert.Equal<uint>(6, binding.ModuleId);
            Assert.Equal<byte>(3, binding.ModulePin);
            Assert.True(binding.IsSink);
        }

        [Fact]
        public void TestModule()
        {
            Module mod = new Module()
            {
                Id = 0,
                ConfigBaseId = 7777,
                InAudioFormatId = 1,
                CoreId = 2,
                ProcessingDomain = 1,
                ConfigExtId = 0x34,
                KcontrolId = 0,
            };

            Assert.Equal<uint>(0, mod.Id);
            Assert.Equal<uint>(7777, mod.ConfigBaseId);
            Assert.Equal<uint>(1, mod.InAudioFormatId);
            Assert.Equal<byte>(2, mod.CoreId.Value);
            Assert.Equal<byte>(1, mod.ProcessingDomain.Value);
            Assert.Equal<uint>(0x34, mod.ConfigExtId);
            Assert.Equal<uint>(0, mod.KcontrolId.Value);
        }

        [Fact]
        public void TestPipeline()
        {
            Pipeline ppl = new Pipeline()
            {
                Id = 0,
                ConfigId = 659308,
                Modules = new Module[] { },
                BindingId = new uint[] { 543 },
            };

            Assert.Equal<uint>(0, ppl.Id);
            Assert.Equal<uint>(659308, ppl.ConfigId);
            Assert.Empty(ppl.Modules);
            Assert.Single(ppl.BindingId);
        }

        [Fact]
        public void TestPath()
        {
            Path path = new Path()
            {
                Id = 0,
                FEAudioFormatId = 2,
                BEAudioFormatId = 5,
                Pipelines = null,
            };

            Assert.Equal<uint>(0, path.Id);
            Assert.Equal<uint>(2, path.FEAudioFormatId);
            Assert.Equal<uint>(5, path.BEAudioFormatId);
            Assert.Null(path.Pipelines);
        }

        [Fact]
        public void TestPathTemplate()
        {
            PathTemplate tmpl = new PathTemplate()
            {
                Id = 0,
                WidgetName = "ssp0_fe",
                IgnoreSuspend = true,
                Paths = new Path[] { },
            };

            Assert.Equal<uint>(0, tmpl.Id);
            Assert.Equal<string>("ssp0_fe", tmpl.WidgetName, s_comparer);
            Assert.True(tmpl.IgnoreSuspend);
            Assert.Empty(tmpl.Paths);
        }

        [Fact]
        public void TestCondpath()
        {
            Condpath path = new Condpath()
            {
                Id = 0,
                SourcePathId = 2,
                SinkPathId = 5,
                Pipelines = null,
            };

            Assert.Equal<uint>(0, path.Id);
            Assert.Equal<uint>(2, path.SourcePathId);
            Assert.Equal<uint>(5, path.SinkPathId);
            Assert.Null(path.Pipelines);
        }

        [Fact]
        public void TestCondpathTemplate()
        {
            CondpathTemplate tmpl = new CondpathTemplate()
            {
                Id = 0,
                SourceTopologyName = "rt274",
                SourcePathTemplateId = 3,
                SinkTopologyName = "dmic",
                SinkPathTemplateId = 6,
                ConditionType = 1,
                Overridable = false,
                Priority = 0,
                Condpaths = new Condpath[] { },
            };

            Assert.Equal<uint>(0, tmpl.Id);
            Assert.Equal<string>("rt274", tmpl.SourceTopologyName, s_comparer);
            Assert.Equal<uint>(3, tmpl.SourcePathTemplateId);
            Assert.Equal<string>("dmic", tmpl.SinkTopologyName, s_comparer);
            Assert.Equal<uint>(6, tmpl.SinkPathTemplateId);
            Assert.Equal<uint>(1, tmpl.ConditionType);
            Assert.False(tmpl.Overridable);
            Assert.Equal<byte>(0, tmpl.Priority);
            Assert.Empty(tmpl.Condpaths);
        }

        [Fact]
        public void TestPCMCapabilities()
        {
            PCMCapabilities caps = new PCMCapabilities()
            {
                Formats = "S16_LE, S24_LE",
                Rates = "44100, 48000",
                Channels = null,
            };

            Assert.Equal<string>("S16_LE, S24_LE", caps.Formats, s_comparer);
            Assert.Equal<string>("44100, 48000", caps.Rates, s_comparer);
            Assert.Null(caps.Channels);
        }

        [Fact]
        public void TestFEDAI()
        {
            FEDAI dai = new FEDAI()
            {
                Name = "System Playback",
                IgnoreSuspend = false,
                CaptureCapabilities = null,
                PlaybackCapabilities = new PCMCapabilities(),
            };

            Assert.Equal<string>("System Playback", dai.Name, s_comparer);
            Assert.False(dai.IgnoreSuspend);
            Assert.Null(dai.CaptureCapabilities);
            Assert.NotNull(dai.PlaybackCapabilities);
        }

        [Fact]
        public void TestDAPMRoute()
        {
            DAPMRoute route = new DAPMRoute()
            {
                Sink = "sink0",
                Control = null,
                Source = "source1",
            };

            Assert.Equal<string>("sink0", route.Sink, s_comparer);
            Assert.Null(route.Control);
            Assert.Equal<string>("source1", route.Source, s_comparer);
        }

        [Fact]
        public void TestDAPMGraph()
        {
            DAPMGraph graph = new DAPMGraph()
            {
                Name = "my_graph",
                Routes = new DAPMRoute[] { },
            };

            Assert.Equal<string>("my_graph", graph.Name, s_comparer);
            Assert.Empty(graph.Routes);
        }

        [Fact]
        public void TestKcontrol()
        {
            Kcontrol kctrl = new Kcontrol()
            {
                Id = 0,
                Name = "my_kctrl",
            };

            Assert.Equal<uint>(0, kctrl.Id);
            Assert.Equal<string>("my_kctrl", kctrl.Name, s_comparer);
        }

        [Fact]
        public void TestTopology()
        {
            Topology tplg = new Topology()
            {
                Name = "avs_hdaudio",
                Version = 1,
                Libraries = new Library[] { },
                AudioFormats = null,
                ModuleConfigsBase = null,
                ModuleConfigsExt = new ModuleConfigExt[] { },
                PipelineConfigs = new PipelineConfig[] { },
                Bindings = new Binding[] { },
                PathTemplates = null,
                CondpathTemplates = new CondpathTemplate[] { },
                FEDAIs = null,
                Graphs = null,
                Kcontrols = null,
            };

            Assert.Equal<string>("avs_hdaudio", tplg.Name, s_comparer);
            Assert.Equal<uint>(1, tplg.Version);
            Assert.NotNull(tplg.Libraries);
            Assert.Null(tplg.AudioFormats);
            Assert.Null(tplg.ModuleConfigsBase);
            Assert.NotNull(tplg.ModuleConfigsExt);
            Assert.NotNull(tplg.PipelineConfigs);
            Assert.NotNull(tplg.Bindings);
            Assert.Null(tplg.PathTemplates);
            Assert.NotNull(tplg.CondpathTemplates);
            Assert.Null(tplg.FEDAIs);
            Assert.Null(tplg.Graphs);
            Assert.Null(tplg.Kcontrols);
        }
    }
}

