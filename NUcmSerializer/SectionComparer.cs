//
// Copyright (c) 2019, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// This program is free software; you can redistribute it and/or modify it
// under the terms and conditions of the MIT License.
//
// This program is distributed in the hope it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE.
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
