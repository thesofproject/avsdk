﻿//
// Copyright (c) 2019, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: MIT
//

using System.Collections.Generic;

namespace NUcmSerializer
{
    public class SectionComparer : EqualityComparer<Section>
    {
        public override bool Equals(Section x, Section y)
        {
            if (x == y)
                return true;
            if (x == null)
                return false;
            if (y == null)
                return false;

            return x.GetType() == y.GetType() &&
                (bool)x.Identifier?.Equals(y.Identifier);
        }

        public override int GetHashCode(Section obj)
        {
            return obj.GetType().GetHashCode() ^ (int)obj.Identifier?.GetHashCode();
        }
    }
}
