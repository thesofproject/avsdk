//
// Copyright (c) 2026, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.Globalization;
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
        public void Library()
        {
            Library lib = new Library()
            {
                Id = 0,
                FileName = "MyLibrary.bin",
            };

            Assert.Equal(0u, lib.Id);
            Assert.Equal("MyLibrary.bin", lib.FileName, s_comparer);
        }

        [Fact]
        public void AudioFormat()
        {
            AudioFormat fmt = new AudioFormat()
            {
                Id = 0,
                SampleRate = 48000,
                BitDepth = 32,
                ChannelMap = "0xFFFFFF10",
                ChannelConfig = 0,
                Interleaving = 0,
                NumChannels = 2,
                ValidBitDepth = 24,
                SampleType = 0,
            };

            Assert.Equal(0u, fmt.Id);
            Assert.Equal(48000u, fmt.SampleRate);
            Assert.Equal(32u, fmt.BitDepth);
            Assert.Equal("0xFFFFFF10", fmt.ChannelMap, s_comparer);
            Assert.Equal(0u, fmt.ChannelConfig);
            Assert.Equal(0u, fmt.Interleaving);
            Assert.Equal(2u, fmt.NumChannels);
            Assert.Equal(24u, fmt.ValidBitDepth);
            Assert.Equal(0u, fmt.SampleType);

            fmt.ChannelMap = string.Empty;
            Assert.Equal("0x00000000", fmt.ChannelMap, s_comparer);

            fmt.ChannelMap = null;
            Assert.Equal("0x00000000", fmt.ChannelMap, s_comparer);
        }

        [Fact]
        public void ModuleConfigBase()
        {
            ModuleConfigBase cfg = new ModuleConfigBase()
            {
                Id = 0,
                Cpc = 100000,
                Ibs = 0x180,
                Obs = 0x300,
                Pages = 1,
            };

            Assert.Equal(0u, cfg.Id);
            Assert.Equal(100000u, cfg.Cpc);
            Assert.Equal(0x180u, cfg.Ibs);
            Assert.Equal(0x300u, cfg.Obs);
            Assert.Equal(1u, cfg.Pages);
        }

        [Fact]
        public void IOPinFormat()
        {
            IOPinFormat pin = new IOPinFormat()
            {
                IObs = 768,
                AudioFormatId = 8,
            };

            Assert.Equal(768u, pin.IObs);
            Assert.Equal(8u, pin.AudioFormatId);
        }

        [Fact]
        public void ModuleConfigExt()
        {
            ModuleConfigExt cfg = new ModuleConfigExt()
            {
                Id = 0,
                UUID = Guid.Empty.ToString(),
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
                PeakVolVolume = 0,
                PeakVolCurveType = 0,
                PeakVolCurveDuration = 0,
            };

            Assert.Equal(0u, cfg.Id);
            Assert.Equal(Guid.Empty.ToString(), cfg.UUID, s_comparer);
            Assert.Equal(0u, cfg.CprOutAudioFormatId.Value);
            Assert.Equal(0u, cfg.CprBlobFormatId.Value);
            Assert.Equal(0u, cfg.CprFeatureMask.Value);
            Assert.Equal<byte>(0, cfg.CprVirtualIndex.Value);
            Assert.Null(cfg.CprDMAType);    // tests negative branch in the getter
            Assert.Equal(0u, cfg.CprDMABufferSize.Value);
            Assert.Equal(0u, cfg.MicselOutAudioFormatId.Value);
            Assert.Equal(0u, cfg.IntelWOVCpcLowPowerMode.Value);
            Assert.Equal(0u, cfg.SrcOutFrequency.Value);
            Assert.Equal(0u, cfg.MuxRefAudioFormatId.Value);
            Assert.Equal(0u, cfg.MuxOutAudioFormatId.Value);
            Assert.Equal(0u, cfg.AecRefAudioFormatId.Value);
            Assert.Equal(0u, cfg.AecOutAudioFormatId.Value);
            Assert.Equal(0u, cfg.AecCpcLowPowerMode.Value);
            Assert.Equal(0u, cfg.UpDownMixOutChanCfg.Value);
            Assert.Equal(0u, cfg.UpDownMixCoeffSelect.Value);
            Assert.Empty(cfg.UpDownMixCoeff);
            Assert.Equal(0u, cfg.UpDownMixChanMap.Value);
            Assert.Equal(0u, cfg.ASrcOutFrequency.Value);
            Assert.Equal<byte>(0, cfg.ASrcMode.Value);
            Assert.Equal<byte>(0, cfg.ASrcDisableJitterBuffer.Value);
            Assert.NotNull(cfg.InPinFormats);
            Assert.Null(cfg.OutPinFormats);
            Assert.Equal(0u, cfg.WhmRefAudioFormatId.Value);
            Assert.Equal(0u, cfg.WhmOutAudioFormatId.Value);
            Assert.Equal(0u, cfg.WhmBlobFormatId.Value);
            Assert.Equal(0u, cfg.WhmWakeTickPeriod.Value);
            Assert.Equal<byte>(0, cfg.WhmVirtualIndex.Value);
            Assert.Null(cfg.WhmDMAType);    // tests negative branch in the getter
            Assert.Equal(0u, cfg.WhmDMABufferSize.Value);
            Assert.Equal(0u, cfg.PeakVolVolume.Value);
            Assert.Equal(0u, cfg.PeakVolCurveType.Value);
            Assert.Equal(0u, cfg.PeakVolCurveDuration.Value);

            cfg.UUID = null;                // exception squelched
            Assert.Equal(Guid.Empty.ToString(), cfg.UUID, s_comparer);

            cfg.CprDMAType = "0xa";
            cfg.WhmDMAType = "0xa";
            Assert.Equal("0x0000000A", cfg.CprDMAType, s_comparer);
            Assert.Equal("0x0000000A", cfg.WhmDMAType, s_comparer);

            cfg.CprDMAType = null;          // value shall not change
            cfg.WhmDMAType = null;
            Assert.Equal("0x0000000A", cfg.CprDMAType, s_comparer);
            Assert.Equal("0x0000000A", cfg.WhmDMAType, s_comparer);
        }

        [Fact]
        public void PipelineConfig()
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

            Assert.Equal(0u, cfg.Id);
            Assert.Equal<ushort>(8, cfg.RequiredSize);
            Assert.Equal<byte>(0, cfg.Priority.Value);
            Assert.False(cfg.LowPower);
            Assert.Equal<ushort>(0, cfg.Attributes.Value);
            Assert.Equal(0u, cfg.Trigger.Value);
        }

        [Fact]
        public void Binding()
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

            Assert.Equal(0u, binding.Id);
            Assert.Equal("avs_hdaudio", binding.TargetTopologyName, s_comparer);
            Assert.Equal(2u, binding.TargetPathTemplateId);
            Assert.Equal(9u, binding.TargetPipelineId);
            Assert.Equal(0xBu, binding.TargetModuleId);
            Assert.Equal<byte>(0, binding.TargetModulePin);
            Assert.Equal(6u, binding.ModuleId);
            Assert.Equal<byte>(3, binding.ModulePin);
            Assert.True(binding.IsSink);
        }

        [Fact]
        public void Module()
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
                InitConfigIds = new uint[] { 1, 2, 3 },
                NHLTConfigId = 7,
            };

            Assert.Equal(0u, mod.Id);
            Assert.Equal(7777u, mod.ConfigBaseId);
            Assert.Equal(1u, mod.InAudioFormatId);
            Assert.Equal<byte>(2, mod.CoreId.Value);
            Assert.Equal<byte>(1, mod.ProcessingDomain.Value);
            Assert.Equal(0x34u, mod.ConfigExtId);
            Assert.Equal(0u, mod.KcontrolId.Value);
            Assert.Equal(3, mod.InitConfigIds.Length);
            Assert.Equal(7u, mod.NHLTConfigId.Value);
        }

        [Fact]
        public void ModuleInitConfig()
        {
            ModuleInitConfig cfg = new ModuleInitConfig()
            {
                Id = 3,
                Param = 0x42,
                Data = new byte[] { 0x01, 0x02, 0x03, 0x04 },
            };

            Assert.Equal(3u, cfg.Id);
            Assert.Equal<byte>(0x42, cfg.Param);
            Assert.Equal(4, cfg.Data.Length);
        }

        [Fact]
        public void NHLTConfig()
        {
            NHLTConfig cfg = new NHLTConfig()
            {
                Id = 5,
                Data = new byte[] { 0xAA, 0xBB },
            };

            Assert.Equal(5u, cfg.Id);
            Assert.Equal(2, cfg.Data.Length);
        }

        [Fact]
        public void Pipeline()
        {
            Pipeline ppl = new Pipeline()
            {
                Id = 0,
                ConfigId = 659308,
                Modules = new Module[] { },
                BindingId = new uint[] { 543 },
            };

            Assert.Equal(0u, ppl.Id);
            Assert.Equal(659308u, ppl.ConfigId);
            Assert.Empty(ppl.Modules);
            Assert.Single(ppl.BindingId);
        }

        [Fact]
        public void Path()
        {
            Path path = new Path()
            {
                Id = 0,
                FEAudioFormatId = 2,
                BEAudioFormatId = 5,
                Pipelines = null,
            };

            Assert.Equal(0u, path.Id);
            Assert.Equal(2u, path.FEAudioFormatId);
            Assert.Equal(5u, path.BEAudioFormatId);
            Assert.Null(path.Pipelines);
        }

        [Fact]
        public void PathTemplate()
        {
            Kcontrol vol = new Kcontrol() { Name = "Master Volume" };
            Kcontrol mute = new Kcontrol() { Name = "Master Switch" };
            PathTemplate tmpl = new PathTemplate()
            {
                Id = 0,
                WidgetName = "ssp0_fe",
                IgnoreSuspend = true,
                Paths = new Path[] { },
                Kcontrol = vol,
                MuteKcontrol = mute,
            };

            Assert.Equal(0u, tmpl.Id);
            Assert.Equal("ssp0_fe", tmpl.WidgetName, s_comparer);
            Assert.True(tmpl.IgnoreSuspend);
            Assert.Empty(tmpl.Paths);
            Assert.Same(vol, tmpl.Kcontrol);
            Assert.Same(mute, tmpl.MuteKcontrol);
        }

        [Fact]
        public void Condpath()
        {
            Condpath path = new Condpath()
            {
                Id = 0,
                SourcePathId = 2,
                SinkPathId = 5,
                Pipelines = null,
            };

            Assert.Equal(0u, path.Id);
            Assert.Equal(2u, path.SourcePathId);
            Assert.Equal(5u, path.SinkPathId);
            Assert.Null(path.Pipelines);
        }

        [Fact]
        public void CondpathTemplate()
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

            Assert.Equal(0u, tmpl.Id);
            Assert.Equal("rt274", tmpl.SourceTopologyName, s_comparer);
            Assert.Equal(3u, tmpl.SourcePathTemplateId);
            Assert.Equal("dmic", tmpl.SinkTopologyName, s_comparer);
            Assert.Equal(6u, tmpl.SinkPathTemplateId);
            Assert.Equal(1u, tmpl.ConditionType);
            Assert.False(tmpl.Overridable);
            Assert.Equal<byte>(0, tmpl.Priority);
            Assert.Empty(tmpl.Condpaths);
        }

        [Fact]
        public void PCMCapabilities()
        {
            PCMCapabilities caps = new PCMCapabilities()
            {
                Formats = "S16_LE, S24_LE",
                Rates = "44100, 48000",
                Channels = null,
                SigBits = 24,
            };

            Assert.Equal("S16_LE, S24_LE", caps.Formats, s_comparer);
            Assert.Equal("44100, 48000", caps.Rates, s_comparer);
            Assert.Null(caps.Channels);
            Assert.Equal(24u, caps.SigBits);
        }

        [Fact]
        public void FEDAI()
        {
            FEDAI dai = new FEDAI()
            {
                Id = 9,
                Name = "System Playback",
                IgnoreSuspend = false,
                CaptureCapabilities = null,
                PlaybackCapabilities = new PCMCapabilities(),
            };

            Assert.Equal(9u, dai.Id);
            Assert.Equal("System Playback", dai.Name, s_comparer);
            Assert.False(dai.IgnoreSuspend);
            Assert.Null(dai.CaptureCapabilities);
            Assert.NotNull(dai.PlaybackCapabilities);
        }

        [Fact]
        public void DAPMRoute()
        {
            DAPMRoute route = new DAPMRoute()
            {
                Sink = "sink0",
                Control = null,
                Source = "source1",
            };

            Assert.Equal("sink0", route.Sink, s_comparer);
            Assert.Null(route.Control);
            Assert.Equal("source1", route.Source, s_comparer);
        }

        [Fact]
        public void DAPMGraph()
        {
            DAPMGraph graph = new DAPMGraph()
            {
                Name = "my_graph",
                Routes = new DAPMRoute[] { },
            };

            Assert.Equal("my_graph", graph.Name, s_comparer);
            Assert.Empty(graph.Routes);
        }

        [Fact]
        public void Kcontrol()
        {
            Kcontrol ctl = new Kcontrol()
            {
                Id = 0,
                Name = "my_kctrl",
                Type = KcontrolType.Mixer,
                Invert = true,
                NumChannels = 2,
            };

            Assert.Equal(0u, ctl.Id);
            Assert.Equal("my_kctrl", ctl.Name, s_comparer);
            Assert.Equal(KcontrolType.Mixer, ctl.Type);
            Assert.Null(ctl.Max);   // tests negative branch in the getter
            Assert.True(ctl.Invert);
            Assert.Equal(2, ctl.NumChannels);

            ctl.Max = "0xa";
            Assert.Equal("0x0000000A", ctl.Max, s_comparer);

            ctl.Max = null;         // value shall not change
            Assert.Equal("0x0000000A", ctl.Max, s_comparer);
        }

        [Fact]
        public void Topology()
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

            Assert.Equal("avs_hdaudio", tplg.Name, s_comparer);
            Assert.Equal(1u, tplg.Version);
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

