//
// Copyright (c) 2020-2022, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUcmSerializer;

namespace avstplg
{
    internal static class ExtensionMethods
    {
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
            IEnumerable<string> substrs = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim());

            foreach (string substr in substrs)
            {
                if (substr.StartsWith("0x", StringComparison.CurrentCulture) &&
                    uint.TryParse(substr.Substring(2), NumberStyles.HexNumber,
                                        CultureInfo.CurrentCulture, out uint val))
                    result.Add(val);
                else if (uint.TryParse(substr, out val))
                    result.Add(val);
            }

            return result.ToArray();
        }

        internal static PCM_RATE ToRate(this uint value)
        {
            switch (value)
            {
                case 5512:
                    return PCM_RATE._5512;
                case 8000:
                    return PCM_RATE._8000;
                case 11025:
                    return PCM_RATE._11025;
                case 16000:
                    return PCM_RATE._16000;
                case 22050:
                    return PCM_RATE._22050;
                case 32000:
                    return PCM_RATE._32000;
                case 44100:
                    return PCM_RATE._44100;
                case 48000:
                    return PCM_RATE._48000;
                case 64000:
                    return PCM_RATE._64000;
                case 88200:
                    return PCM_RATE._88200;
                case 96000:
                    return PCM_RATE._96000;
                case 176400:
                    return PCM_RATE._176400;
                case 192000:
                    return PCM_RATE._192000;
                case 0:
                case 24000:
                case 37800:
                    return PCM_RATE.KNOT;

                default:
                    throw new NotSupportedException(nameof(value));
            }
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
