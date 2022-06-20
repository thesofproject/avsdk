namespace avstplg
{
    public enum AVS_TKN_MANIFEST
    {
        NAME_STRING = 1,
        VERSION_U32,
        NUM_LIBRARIES_U32,
        NUM_AFMTS_U32,
        NUM_MODCFGS_BASE_U32,
        NUM_MODCFGS_EXT_U32,
        NUM_PPLCFGS_U32,
        NUM_BINDINGS_U32,
        NUM_CONDPATH_TMPLS_U32,
    }

    public enum AVS_TKN_LIBRARY
    {
        ID_U32 = 101,
        FILENAME_STRING,
    }

    public enum AVS_TKN_AFMT
    {
        ID_U32 = 201,
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
        ID_U32 = 301,
        CPC_U32,
        IBS_U32,
        OBS_U32,
        PAGES_U32,
    }

    public enum AVS_TKN_MODCFG
    {
        EXT_ID_U32 = 401,
        EXT_TYPE_UUID,

        CPR_OUT_AFMT_ID_U32,
        CPR_FEATURE_MASK_U32,
        CPR_DMA_TYPE_U32,
        CPR_DMABUFF_SIZE_U32,
        CPR_VINDEX_U8,
        CPR_BLOB_FMT_ID_U32,

        MICSEL_OUT_AFMT_ID_U32,

        INTELWOV_CPC_LP_MODE_U32,

        SRC_OUT_FREQ_U32,

        MUX_REF_AFMT_ID_U32,
        MUX_OUT_AFMT_ID_U32,

        AEC_REF_AFMT_ID_U32,
        AEC_OUT_AFMT_ID_U32,
        AEC_CPC_LP_MODE_U32,

        ASRC_OUT_FREQ_U32,
        ASRC_MODE_U8,
        ASRC_DISABLE_JITTER_BUFFER_U8,

        UPDOWN_MIX_OUT_CHAN_CFG_U32,
        UPDOWN_MIX_COEFF_SELECT_U32,
        UPDOWN_MIX_COEFF_0_S32,
        UPDOWN_MIX_COEFF_1_S32,
        UPDOWN_MIX_COEFF_2_S32,
        UPDOWN_MIX_COEFF_3_S32,
        UPDOWN_MIX_COEFF_4_S32,
        UPDOWN_MIX_COEFF_5_S32,
        UPDOWN_MIX_COEFF_6_S32,
        UPDOWN_MIX_COEFF_7_S32,
        UPDOWN_MIX_CHAN_MAP_U32,

        EXT_NUM_INPUT_PINS_U16,
        EXT_NUM_OUTPUT_PINS_U16,

        WHM_REF_AFMT_ID_U32,
        WHM_OUT_AFMT_ID_U32,
        WHM_WAKE_TICK_PERIOD_U32,
        WHM_DMA_TYPE_U32,
        WHM_DMABUFF_SIZE_U32,
        WHM_VINDEX_U8,
        WHM_BLOB_FMT_ID_U32,
    }

    public enum AVS_TKN_PPLCFG
    {
        ID_U32 = 1401,
        REQ_SIZE_U16,
        PRIORITY_U8,
        LOW_POWER_BOOL,
        ATTRIBUTES_U16,
        TRIGGER_U32,
    }

    public enum AVS_TKN_BINDING
    {
        ID_U32 = 1501,
        TARGET_TPLG_NAME_STRING,
        TARGET_PATH_TMPL_ID_U32,
        TARGET_PPL_ID_U32,
        TARGET_MOD_ID_U32,
        TARGET_MOD_PIN_U8,
        MOD_ID_U32,
        MOD_PIN_U8,
        IS_SINK_U8,
    }

    public enum AVS_TKN_PPL
    {
        ID_U32 = 1601,
        PPLCFG_ID_U32,
        NUM_BINDING_IDS_U32,
        BINDING_ID_U32,
    }

    public enum AVS_TKN_MOD
    {
        ID_U32 = 1701,
        MODCFG_BASE_ID_U32,
        IN_AFMT_ID_U32,
        CORE_ID_U8,
        PROC_DOMAIN_U8,
        MODCFG_EXT_ID_U32,
        KCONTROL_ID_U32,
    }

    public enum AVS_TKN_PATH_TMPL
    {
        ID_U32 = 1801,
        DAI_WNAME_STRING,
    }

    public enum AVS_TKN_PATH
    {
        ID_U32 = 1901,
        FE_AFMT_ID_U32,
        BE_AFMT_ID_U32,
    }

    public enum AVS_TKN_CONDPATH_TMPL
    {
        ID_U32 = AVS_TKN_PATH_TMPL.ID_U32,
        SOURCE_TPLG_NAME_STRING = 2002,
        SOURCE_PATH_TMPL_ID_U32,
        SINK_TPLG_NAME_STRING,
        SINK_PATH_TMPL_ID_U32,
        COND_TYPE_U32,
        OVERRIDABLE_BOOL,
        PRIORITY_U8,
    }

    public enum AVS_TKN_CONDPATH
    {
        ID_U32 = AVS_TKN_PATH.ID_U32,
        SOURCE_PATH_ID_U32 = 2102,
        SINK_PATH_ID_U32,
    }

    public enum AVS_TKN_PIN_FMT
    {
        INDEX_U32 = 2201,
        IOBS_U32,
        AFMT_ID_U32,
    }

    public enum AVS_TKN_KCONTROL
    {
        ID_U32 = 2301,
    }
}
