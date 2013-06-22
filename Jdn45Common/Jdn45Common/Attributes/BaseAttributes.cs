using System;
using System.Collections.Generic;
using System.Text;

namespace Jdn45Common.Attributes
{
    /// <summary>
    /// Base attribute class of Boolean type.
    /// Inherit to use in different scenarios.
    /// </summary>
    public abstract class BoolAttribute : Attribute
    {
        private bool isSet;

        public BoolAttribute()
        {
            this.isSet = true;
        }

        public BoolAttribute(bool isSet)
        {
            this.isSet = isSet;
        }

        public bool IsSet { get { return isSet; } }
    }

    /// <summary>
    /// Base attribute class of integer type.
    /// Inherit to use in different scenarios.
    /// </summary>
    public abstract class IntAttribute : Attribute
    {
        private int value;

        public IntAttribute()
        {
            this.value = 0;
        }

        public IntAttribute(int value)
        {
            this.value = value;
        }

        public int Value { get { return value; } }
    }

    /// <summary>
    /// Localized string.
    /// Inherit to use in different scenarios.
    /// </summary>
    public abstract class LocalizedStringAttribute : Attribute
    {
        string str;

        public LocalizedStringAttribute(Type resourceManagerProvider, string resourceKey)
        {
            str = Util.LookupResource(resourceManagerProvider, resourceKey);
        }

        public LocalizedStringAttribute(Type resourceManagerProvider, string resourceKey, string prepend, string append)
        {
            str = string.Format("{0}{1}{2}",
                string.IsNullOrEmpty(prepend) ? "" : prepend,
                Util.LookupResource(resourceManagerProvider, resourceKey),
                string.IsNullOrEmpty(append) ? "" : append);
        }

        public string Value { get { return str; } }
    }
}
