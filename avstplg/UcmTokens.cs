namespace avstplg
{
    public enum AVS_TKN_PIPELINE
    {
        ID_U32 = 501,
        REQ_SIZE_U16,
        PRIORITY_U8,
        LOW_POWER_BOOL,
    }

    public enum AVS_TKN_PIPE
    {
        OBJECT_ID_U32 = 9101,
        IMPL_ID_U32,
    }

    public enum AVS_TKN_AFMT
    {
        ID_U32 = 601,
        SAMPLE_RATE_U32,
        BIT_DEPTH_U32,
        CHANNEL_MAP_U32,
        CHANNEL_CFG_U32,
        INTERLEAVING_U32,
        NUM_CHANNELS_U32,
        VALID_BIT_DEPTH_U32,
        SAMPLE_TYPE_U32,
    }

    public enum AVS_TKN_MODCFG_BASE
    {
        ID_U32 = 701,
        CPC_U32,
        IBS_U32,
        OBS_U32,
        PAGES_U32,
        AFMT_ID_U32,
    }

    public enum AVS_TKN_MODCFG
    {
        OBJECT_ID_U32 = 801,
        TYPE_UUID,
        CORE_ID_U8 = 804,
        PROC_DOMAIN_U8,

        BASE_ID_U32 = 811,

        CPR_OUT_AFMT_ID_U32 = 821,
        CPR_FEATURE_MASK_U32,
        CPR_DMA_TYPE_U32,
        CPR_DMABUFF_SIZE_U32,

        MICSEL_OUT_AFMT_ID_U32 = 831,

        INTELWOV_CPC_LOW_POWER_MODE_U32 = 841,

        SRC_OUT_FREQ_U32 = 851,
    }

    public enum AVS_TKN_ROUTE
    {
        TARGET_TPLG_NAME_STRING = 9001,
        TARGET_PATH_OBJECT_ID_U32,
        TARGET_PPL_OBJECT_ID_U32,
        TARGET_MOD_OBJECT_ID_U32,
        TARGET_MOD_PIN_U8,
        MOD_OBJECT_ID_U32,
        MOD_PIN_U8,
        IS_SINK_U8,
    }

    public enum AVS_TKN_PATH
    {
        VARIANT_ID_U32 = 10001,
        FE_FMT_ID_U32,
        BE_FMT_ID_U32,
    }

    public enum AVS_TKN_PATH_TMPL
    {
        OBJECT_ID_U32 = 10100,
        DAI_SNAME_STRING,
    }

    public enum AVS_TKN_LIBRARY
    {
        FILE_STRING = 111000,
    }

    public enum AVS_TKN_TPLG
    {
        NUM_AUDIO_FMTS_U32 = 2001,
        NUM_BASE_CFGS_U32,
        NUM_PIPES_U32,
        NUM_LIBRARIES_U32,
        NAME_STRING,
    }
}
