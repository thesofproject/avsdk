// ExtensionMethods.cs - Collection of extension methods
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

using NUmcSerializer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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

        internal static SKL_PIPE_CONN_TYPE GetValue(this ConnType connType)
        {
            switch (connType)
            {
                case ConnType.HOST_DMA:
                case ConnType.HDMI_HOST_DMA:
                    return SKL_PIPE_CONN_TYPE.FE;

                case ConnType.LINK_DMA:
                    return SKL_PIPE_CONN_TYPE.BE;

                case ConnType.NONE:
                default:
                    return SKL_PIPE_CONN_TYPE.NONE;
            }
        }

        internal static SKL_HW_CONN_TYPE GetHwConnType(this Direction direction)
        {
            switch (direction)
            {
                case Direction.PLAYBACK:
                    return SKL_HW_CONN_TYPE.SOURCE;

                case Direction.CAPTURE:
                    return SKL_HW_CONN_TYPE.SINK;

                case Direction.INVALID:
                default:
                    return SKL_HW_CONN_TYPE.NONE;
            }
        }

        internal static SKL_MODULE_TYPE GetModuleType(this string value)
        {
            switch (value)
            {
                case ModuleNames.Mixin:
                case ModuleNames.Mixout:
                    return SKL_MODULE_TYPE.MIXER;
                case ModuleNames.Copier:
                    return SKL_MODULE_TYPE.COPIER;
                case ModuleNames.UpDownMix:
                    return SKL_MODULE_TYPE.UPDWMIX;
                case ModuleNames.Src:
                    return SKL_MODULE_TYPE.SRCINT;
                case ModuleNames.MicSel:
                    return SKL_MODULE_TYPE.MIC_SELECT;
                case ModuleNames.Kpb:
                    return SKL_MODULE_TYPE.KPB;
                case ModuleNames.Probe:
                    return SKL_MODULE_TYPE.PROBE;
                case ModuleNames.Asrc:
                    return SKL_MODULE_TYPE.ASRC;
                case ModuleNames.Gain:
                    return SKL_MODULE_TYPE.GAIN;
                default:
                    return SKL_MODULE_TYPE.ALGO;
            }
        }

        internal static bool TryUInt32(this string value, out uint result)
        {
            if (value.StartsWith("0x", StringComparison.CurrentCulture))
                return uint.TryParse(value.Substring(2), NumberStyles.HexNumber,
                              CultureInfo.CurrentCulture, out result);

            return uint.TryParse(value, out result);
        }

        internal static uint ToUInt32(this string value)
        {
            TryUInt32(value, out uint result);
            return result;
        }

        internal static uint[] ToUInts32(this string value)
        {
            var result = new List<uint>();
            var substrs = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim());

            foreach (var substr in substrs)
                if (value.TryUInt32(out uint val))
                    result.Add(val);

            return result.ToArray();
        }

        internal static byte[] ToBytes(this string value)
        {
            return ToUInts32(value).SelectMany(e => BitConverter.GetBytes(e)).ToArray();
        }

        internal static TPLG_DAPM ToDapm(this ModulePosition pos)
        {
            switch (pos)
            {
                case ModulePosition.SOURCE:
                    return TPLG_DAPM.MIXER;

                default:
                    return TPLG_DAPM.PGA;
            }
        }

        internal static string ToRate(this uint value)
        {
            string rate;
            switch (value)
            {
                case 5512:
                case 8000:
                case 11025:
                case 16000:
                case 22050:
                case 32000:
                case 44100:
                case 48000:
                case 64000:
                case 88200:
                case 96000:
                case 176400:
                case 192000:
                    rate = value.ToString();
                    break;

                case 0:
                case 24000:
                case 37800:
                    rate = "KNOT";
                    break;

                default:
                    throw new NotSupportedException(nameof(value));
            }

            return rate;
        }

        internal static PCM_FORMAT ToFormat(this uint value)
        {
            switch (value)
            {
                case 16:
                    return PCM_FORMAT.S16_LE;
                case 24:
                    return PCM_FORMAT.S24_LE;
                case 32:
                    return PCM_FORMAT.S32_LE;
                default:
                    throw new NotSupportedException(nameof(value));
            }
        }
    }
}
