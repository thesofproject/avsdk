using NUmcSerializer;

namespace itt
{
    public static class Constants
    {
        public const uint DMA_CLK_CONTROLS        = 1;
        public const uint DMA_TRANSMISSION_START  = 2;
        public const uint DMA_TRANSMISSION_STOP   = 3;

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
}
