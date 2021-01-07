// Constants.cs - Topology constants
//
// Copyright (c) 2018-2019 Intel Corporation
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// The source code contained or described herein and all documents
// related to the source code ("Material") are owned by Intel Corporation
// or its suppliers or licensors. Title to the Material remains with
// Intel Corporation or its suppliers and licensors. The Material contains
// trade secrets and proprietary and confidential information of Intel or
// its suppliers and licensors. The Material is protected by worldwide
// copyright and trade secret laws and treaty provisions. No part of the
// Material may be used, copied, reproduced, modified, published, uploaded,
// posted, transmitted, distributed, or disclosed in any way without Intel's
// prior express written permission.
//
// No license under any patent, copyright, trade secret or other intellectual
// property right is granted to or conferred upon you by disclosure or
// delivery of the Materials, either expressly, by implication, inducement,
// estoppel or otherwise. Any license under such intellectual property
// rights must be express and approved by Intel in writing.
//

using NUcmSerializer;

namespace itt
{
    public static class Constants
    {
        public const uint DMA_CLK_CONTROLS        = 1;
        public const uint DMA_TRANSMISSION_START  = 2;
        public const uint DMA_TRANSMISSION_STOP   = 3;
        public const uint SCHEDULER_CONFIG        = 18;

        public const uint DMA_BUFFER_COUNT        = 40;

        public const uint MAX_IN_QUEUE   = 8;
        public const uint MAX_OUT_QUEUE  = 8;

        public const int GAIN_MIN_INDEX  = 0;
        public const int GAIN_MAX_INDEX  = 1440;
        public const int GAIN_DB_STEP    = 10;
        public const int GAIN_DB_MIN     = -14400;
        public const int GAIN_TC_MIN     = 0;
        public const int GAIN_TC_MAX     = 10000000;
        public const int GAIN_RT_MIN     = 0;
        public const int GAIN_RT_MAX     = 2;

        public const uint SKL_CTL_TLV_BYTE         = 0x100;
        public const uint SKL_CTL_TLV_PROBE        = 0x101;
        public const uint SKL_CTL_MIC_SELECT       = 0x102;
        public const uint SKL_CTL_MULTI_IO_SELECT  = 0x103;
        public const uint SKL_CTL_VOLUME           = 0x104;
        public const uint SKL_CTL_RAMP_DURATION    = 0x105;
        public const uint SKL_CTL_RAMP_TYPE        = 0x106;

        public const int NOPM = -1;

        // Intel internal event flags
        public const DAPM_EVENT MIX   = (DAPM_EVENT.POST_PMU | DAPM_EVENT.PRE_PMD |
                                         DAPM_EVENT.PRE_PMU | DAPM_EVENT.POST_PMD);
        public const DAPM_EVENT VMIX  = (DAPM_EVENT.PRE_PMU | DAPM_EVENT.POST_PMD);
        public const DAPM_EVENT PGAL  = (DAPM_EVENT.PRE_PMU | DAPM_EVENT.POST_PMD);
    }

    public static class ModuleNames
    {
        public const string Mixin = "mixin";
        public const string Mixout = "mixout";
        public const string Copier = "copier";
        public const string PeakVolume = "peakvol";
        public const string Gain = "gain";
        public const string UpDownMix = "updwmix";
        public const string Src = "srcintc";
        public const string Asrc = "asrc";
        public const string Wov = "wov";
        public const string Aec = "aec";
        public const string MicSel = "micsel";
        public const string Mux = "mux";
        public const string Kpb = "kpbuff";
        public const string Probe = "probe";
        public const string Eqiir = "eqiir";
        public const string Eqfir = "eqfir";
        public const string Eqdcr = "eqdcr";
        public const string MDRC = "mdrc";
        public const string Uplink = "uplink";
        public const string Downlink = "downlink";
    }

    //
    // Constants below help select the next widget to trigger.
    //
    // Following are the DAPM rules:
    // - mixer power up than PGA power up in case of DAPM power up sequence
    // - PGA power down than Mixer power down in case of DAPM down sequence
    // - within widgets of the same type, lower numbered get powered up first
    // - within widgets of the same type, higher numbered get powered down first
    //
    // Following are the rules required by the firmware:
    // - pipes should be created from source to sink pipe, provided all of them
    //   have the same priorities
    // - pipes should be started from sink to source pipe irrespective of
    //   priority of pipe
    //
    // Given the DAPM and firmware rules, following priorities have been defined
    // for the different mixer and PGA modules, based on whether they belong to
    // FE pipe or BE pipe, and whether they are source or sink pipe.
    //
    public static class HDA_DAPM_SUBSEQ
    {
        public const uint BE_SINK_MIX       = 10u;
        public const uint BE_SINK_PGA       = 0u;
        public const uint FE_SRC_MIX        = 0u;
        public const uint FE_SRC_PGA        = 10u;

        public const uint BE_SRC_MIX        = 0u;
        public const uint BE_SRC_PGA        = 10u;
        public const uint FE_SINK_MIX       = 10u;
        public const uint FE_SINK_PGA       = 0u;

        public const uint INTERMEDIATE_MIX  = 5u;
        public const uint INTERMEDIATE_PGA  = 5u;
    }
}
