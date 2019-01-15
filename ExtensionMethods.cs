using System;

namespace itt
{
    internal static class ExtensionMethods
    {
        internal static string GetName(this SKL_TKN token)
        {
            Type type = token.GetType();
            return string.Format("{0}_{1}", type.Name, Enum.GetName(type, token));
        }

        internal static uint GetValue(this Port port)
        {
            switch (port)
            {
                case Port.SSP0:
                case Port.SDW0:
                case Port.PDM:
                    return 0;

                case Port.SSP1:
                case Port.SDW1:
                    return 1;

                case Port.SSP2:
                case Port.SDW2:
                    return 2;

                case Port.SSP3:
                case Port.SDW3:
                    return 3;

                case Port.SSP4:
                    return 4;

                case Port.SSP5:
                    return 5;

                case Port.SSP6:
                    return 6;

                case Port.NONE:
                default:
                    return (uint)Port.INVALID;
            }
        }

        internal static byte GetValue(this ConnType connType)
        {
            switch (connType)
            {
                case ConnType.HOST_DMA:
                case ConnType.HDMI_HOST_DMA:
                    return 1; // SKL_PIPE_CONN_TYPE_FE

                case ConnType.LINK_DMA:
                    return 2; // SKL_PIPE_CONN_TYPE_BE

                default:
                case ConnType.NONE:
                    return 0; // SKL_PIPE_CONN_TYPE_NONE
            }
        }

        internal static SKL_MODULE_TYPE GetModuleType(this string value)
        {
            switch (value)
            {
                case "mixin":
                case "mixout":
                    return SKL_MODULE_TYPE.MIXER;
                case "copier":
                    return SKL_MODULE_TYPE.COPIER;
                case "updownmixer":
                case "updwmix":
                    return SKL_MODULE_TYPE.UPDWMIX;
                case "srcintc":
                    return SKL_MODULE_TYPE.SRCINT;
                case "mic_select":
                    return SKL_MODULE_TYPE.MIC_SELECT;
                case "kpb":
                    return SKL_MODULE_TYPE.KPB;
                case "probe":
                    return SKL_MODULE_TYPE.PROBE;
                case "asrc":
                    return SKL_MODULE_TYPE.ASRC;
                case "gain":
                    return SKL_MODULE_TYPE.GAIN;
                default:
                    return SKL_MODULE_TYPE.ALGO;
            }
        }
    }
}
