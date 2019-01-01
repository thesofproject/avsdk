using System;

namespace NUmcSerializer
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class UmcNamedTagAttribute : Attribute
    {
        public string Name { get; }

        public UmcNamedTagAttribute(string name)
        {
            Name = name;
        }
    }

    // exclusive with all other attributes, default for all properties
    public class UmcElementAttribute : UmcNamedTagAttribute
    {
        public UmcElementAttribute(string name)
            : base(name)
        {
        }

        public UmcElementAttribute()
            : this(null)
        {
        }
    }

    // default for all arrays
    public class UmcArrayAttribute : UmcNamedTagAttribute
    {
        public bool Inline { get; set; }

        public UmcArrayAttribute(string name, bool inline)
            : base(name)
        {
            Inline = inline;
        }

        public UmcArrayAttribute()
            : this(null, false)
        {
        }

        public UmcArrayAttribute(string name)
            : this(name, false)
        {
        }

        public UmcArrayAttribute(bool inline)
            : this(null, inline)
        {
        }
    }

    // default for all sections
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property)]
    public class UmcSectionAttribute : UmcNamedTagAttribute
    {
        public UmcSectionAttribute(string name)
            : base(name)
        {
        }

        public UmcSectionAttribute()
            : this(null)
        {
        }
    }

    // only one per Type (section), exclusive with all other attributes
    [AttributeUsage(AttributeTargets.Property)]
    public class UmcIdentifierAttribute : Attribute
    {
    }

    // mutually exclusive with identifier
    [AttributeUsage(AttributeTargets.Property)]
    public class UmcExclusiveAttribute : Attribute
    {
        public string Namespace { get; set; }

        public UmcExclusiveAttribute(string nameSpace)
        {
            Namespace = nameSpace;
        }

        public UmcExclusiveAttribute()
            : this("")
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class UmcIgnoreAttribute : Attribute
    {
    }
}
