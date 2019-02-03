using NUmcSerializer;

namespace itt
{
    public static class Constants
    {
        public const uint DMA_CLK_CONTROLS        = 1;
        public const uint DMA_TRANSMISSION_START  = 2;
        public const uint DMA_TRANSMISSION_STOP   = 3;
        public const uint SCHEDULER_CONFIG        = 18;

        public const uint DMA_BUFFER_COUNT        = 24;

        public const uint MAX_IN_QUEUE   = 8;
        public const uint MAX_OUT_QUEUE  = 8;

        public const int GAIN_MIN_INDEX  = 0;
        public const int GAIN_MAX_INDEX  = 1440;
        public const int GAIN_DB_STEP    = 10;
        public const int GAIN_DB_MIN     = -14400;
        public const int GAIN_TC_MIN     = 0;
        public const int GAIN_TC_MAX     = 5000;
        public const int GAIN_RT_MIN     = 0;
        public const int GAIN_RT_MAX     = 2;

        public const uint SKL_CTL_TLV_BYTE         = 0x100;
        public const uint SKL_CTL_TLV_PROBE        = 0x101;
        public const uint SKL_CTL_MIC_SELECT       = 0x102;
        public const uint SKL_CTL_MULTI_IO_SELECT  = 0x103;
        public const uint SKL_CTL_VOLUME           = 0x104;
        public const uint SKL_CTL_RAMP_DURATION    = 0x105;
        public const uint SKL_CTL_RAMP_TYPE        = 0x106;

        // Max size of module init params
        public const uint HDA_SST_CFG_MAX = 900;

        public const uint SKL_UUID_STR_SZ = 40;

        public const int NOPM = -1;

        // Intel internal event flags
        public const DAPM_EVENT MIX   = (DAPM_EVENT.POST_PMU | DAPM_EVENT.PRE_PMD |
                                         DAPM_EVENT.PRE_PMU | DAPM_EVENT.POST_PMD);
        public const DAPM_EVENT VMIX  = (DAPM_EVENT.PRE_PMU | DAPM_EVENT.POST_PMD);
        public const DAPM_EVENT PGAL  = (DAPM_EVENT.PRE_PMU | DAPM_EVENT.POST_PMD);
    }

    //
    // This is for sorting which widgets will be triggered first
    // according to DAPM rules.
    //
    // Following are the DAPM Rules.
    // Mixer power up than PGA Power up in case of DAPM power up sequence.
    //
    // PGA power down than Mixer power down in case of DAPM down sequence.
    // Within same type of widgets, lower number widgets get powered up first.
    //
    // Withing same type of widgets, higher numbered gets powered down first.
    //
    // Following are the rules required by the firmware
    // Pipe should be created from src pipe to sink pipe, provided all the pipe
    // has same priorities.
    // Pipes should be started from sink pipe to source pipe irrespective of
    // priority of pipe.
    //
    // Based on DAPM rules and the firmware required priority we have come
    // up with below priorities for the different mixer and PGA modules, based
    // on whether they belong to FE pipe or BE pipe, and whether they are
    // SRC or Sink pipes.
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
