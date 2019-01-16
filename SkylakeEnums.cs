namespace itt
{
    public enum SKL_MODULE_TYPE
    {
        MIXER = 0,
        COPIER,
        UPDWMIX,
        SRCINT,
        ALGO,
        BASE_OUTFMT,
        KPB,
        MIC_SELECT,
        PROBE,
        ASRC,
        GAIN,
    };

    public enum SKL_EVENT_TYPE
    {
        NONE = 0,
        MIXER,
        MUX,
        VMIXER,
        PGA
    };

    public enum SKL_BLOCK_TYPE
    {
        TUPLE,
        BINARY
    };

    public enum SKL_TKN
    {
        UUID = 1,
        U8_NUM_BLOCKS,
        U8_BLOCK_TYPE,
        U8_IN_PIN_TYPE,
        U8_OUT_PIN_TYPE,
        U8_DYN_IN_PIN,
        U8_DYN_OUT_PIN,
        U8_IN_QUEUE_COUNT,
        U8_OUT_QUEUE_COUNT,
        U8_TIME_SLOT,
        U8_CORE_ID,
        U8_MODULE_TYPE,
        U8_CONN_TYPE,
        U8_DEV_TYPE,
        U8_HW_CONN_TYPE,
        U16_MOD_INST_ID,
        U16_BLOCK_SIZE,
        U32_MAX_MCPS,
        U32_MEM_PAGES,
        U32_OBS,
        U32_IBS,
        U32_VBUS_ID,
        U32_PARAMS_FIXUP,
        U32_CONVERTER,
        U32_PIPE_ID,
        U32_PIPE_CONN_TYPE,
        U32_PIPE_PRIORITY,
        U32_PIPE_MEM_PGS,
        U32_DIR_PIN_COUNT,
        U32_FMT_CH,
        U32_FMT_FREQ,
        U32_FMT_BIT_DEPTH,
        U32_FMT_SAMPLE_SIZE,
        U32_FMT_CH_CONFIG,
        U32_FMT_INTERLEAVE,
        U32_FMT_SAMPLE_TYPE,
        U32_FMT_CH_MAP,
        U32_PIN_MOD_ID,
        U32_PIN_INST_ID,
        U32_MOD_SET_PARAMS,
        U32_MOD_PARAM_ID,
        U32_CAPS_SET_PARAMS,
        U32_CAPS_PARAMS_ID,
        U32_CAPS_SIZE,
        U32_PROC_DOMAIN,
        U32_LIB_COUNT,
        STR_LIB_NAME,
        U32_PMODE,
        U32_D0I3_CAPS,
        U32_DMA_BUF_SIZE,

        U32_PIPE_DIRECTION,
        U32_PIPE_CONFIG_ID,
        U32_NUM_CONFIGS,
        U32_PATH_MEM_PGS,

        U32_CFG_FREQ,
        U8_CFG_CHAN,
        U8_CFG_BPS,
        CFG_MOD_RES_ID,
        CFG_MOD_FMT_ID,
        U8_NUM_MOD,

        MM_U8_MOD_IDX,
        MM_U8_NUM_RES,
        MM_U8_NUM_INTF,
        MM_U32_RES_ID,
        MM_U32_CPS,
        MM_U32_DMA_SIZE,
        MM_U32_CPC,
        MM_U32_RES_PIN_ID,
        MM_U32_INTF_PIN_ID,
        MM_U32_PIN_BUF,
        MM_U32_FMT_ID,
        MM_U32_NUM_IN_FMT,
        MM_U32_NUM_OUT_FMT,

        U32_ASTATE_IDX,
        U32_ASTATE_COUNT,
        U32_ASTATE_KCPS,
        U32_ASTATE_CLK_SRC,

        U32_DMACTRL_CFG_IDX = 82,
        U32_DMACTRL_CFG_SIZE,
        U32_DMA_IDX,
        U32_DMA_TYPE,
        U32_DMA_SIZE,
        U32_DMA_MAX_SIZE,
        U32_DMA_MIN_SIZE,

        U32_SCH_TYPE,
        U32_SCH_SIZE,
        U32_SCH_SYS_TICK_MUL,
        U32_SCH_SYS_TICK_DIV,
        U32_SCH_SYS_TICK_LL_SRC,
        U32_SCH_SYS_TICK_CFG_LEN,
        U32_SCH_SYS_TICK_CFG,

        U32_FMT_CFG_IDX,
        MAX = U32_FMT_CFG_IDX,
    }
}
