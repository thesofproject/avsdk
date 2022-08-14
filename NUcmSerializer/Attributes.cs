//
// Copyright (c) 2018, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: MIT
//

using System;

namespace NUcmSerializer
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class UcmNamedTagAttribute : Attribute
    {
        public string Name { get; }

        public UcmNamedTagAttribute(string name)
        {
            Name = name;
        }
    }

    // exclusive with all other attributes, default for all properties
    public class UcmElementAttribute : UcmNamedTagAttribute
    {
        public UcmElementAttribute(string name)
            : base(name)
        {
        }

        public UcmElementAttribute()
            : this(null)
        {
        }
    }

    // default for all arrays
    public class UcmArrayAttribute : UcmNamedTagAttribute
    {
        public bool Inline { get; set; }
        public bool TagElements { get; set; }

        public UcmArrayAttribute(string name, bool inline)
            : base(name)
        {
            Inline = inline;
            TagElements = true;
        }

        public UcmArrayAttribute()
            : this(null, false)
        {
        }

        public UcmArrayAttribute(string name)
            : this(name, false)
        {
        }

        public UcmArrayAttribute(bool inline)
            : this(null, inline)
        {
        }
    }

    // default for all sections
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property)]
    public class UcmSectionAttribute : UcmNamedTagAttribute
    {
        public UcmSectionAttribute(string name)
            : base(name)
        {
        }

        public UcmSectionAttribute()
            : this(null)
        {
        }
    }

    // only one per Type (section), exclusive with all other attributes
    [AttributeUsage(AttributeTargets.Property)]
    public class UcmIdentifierAttribute : Attribute
    {
    }

    // mutually exclusive with identifier
    [AttributeUsage(AttributeTargets.Property)]
    public class UcmExclusiveAttribute : Attribute
    {
        public string Namespace { get; set; }

        public UcmExclusiveAttribute(string nameSpace)
        {
            Namespace = nameSpace;
        }

        public UcmExclusiveAttribute()
            : this("")
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class UcmIgnoreAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class UcmEnumAttribute : Attribute
    {
        public string Name { get; set; }

        public UcmEnumAttribute(string name)
        {
            Name = name;
        }

        public UcmEnumAttribute()
            : this(null)
        {
        }
    }
}
