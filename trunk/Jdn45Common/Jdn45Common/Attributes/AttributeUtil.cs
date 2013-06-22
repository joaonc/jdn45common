using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace Jdn45Common.Attributes
{
    /// <summary>
    /// Helps with the usage of attributes, especially those in this namespace.
    /// </summary>
    public static class AttributeUtil
    {
        /// <summary>
        /// Gets a list of member fields that are mandatory (have the Mandatory attribute set to true).
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<MemberInfo> GetMandatoryMembers(object obj)
        {
            List<MemberInfo> mandatoryMembers = new List<MemberInfo>();

            foreach (MemberInfo memberInfo in obj.GetType().GetMembers(BindingFlags.Public))
            {
                foreach (Attribute attribute in memberInfo.GetCustomAttributes(typeof(MandatoryAttribute), true))
                {
                    if (((MandatoryAttribute)attribute).IsSet)
                    {
                        mandatoryMembers.Add(memberInfo);
                    }
                }
            }

            return mandatoryMembers;
        }

        /// <summary>
        /// Gets the description attribute for the enum value.
        /// See also GetByDescription(...) on how to get the Enum from the description.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes =
                  (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }

        /// <summary>
        /// Gets the enum from the description attribute.
        /// Returns null if not found.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static object GetEnumByDescription(Type type, string description)
        {
            if (!type.IsEnum)
            {
                throw new Exception("Type needs to be enum.\nCurrently " + type.Name);
            }

            foreach (string name in Enum.GetNames(type))
            {
                Enum obj = (Enum)Enum.Parse(type, name);
                if (description.Equals(GetDescription(obj)))
                {
                    return obj;
                }
            }

            return null;
        }

        public static bool GetBoolAttributeValue(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            BoolAttribute[] attributes =
                  (BoolAttribute[])fi.GetCustomAttributes(typeof(BoolAttribute), true);

            return (attributes.Length > 0) ? attributes[0].IsSet : false;
        }

        /// <summary>
        /// Gets the Enum value that has the given bool attribute set to true or null if none is found.
        /// The attribute needs to derive from BoolAttribute.
        /// There can only be one value in the Enum with this attribute set to true.
        /// Use GetEnumsByBoolAttribute(...) to get all the Enum values.
        /// <typeparam name="T">An Enum.</typeparam>
        /// <param name="attributeType">A type that derives from BoolAttribute.</param>
        /// <param name="value">The value to search for.</param>
        /// <returns></returns>
        public static T GetEnumByBoolAttribute<T>(Type attributeType)
        {
            return GetEnumByBoolAttribute<T>(attributeType, true);
        }

        /// <summary>
        /// Gets the Enum value that has the given bool attribute set to the given value or null if none is found.
        /// The attribute needs to derive from BoolAttribute.
        /// There can only be one value in the Enum with this attribute set to true.
        /// Use GetEnumsByBoolAttribute(...) to get all the Enum values.
        /// </summary>
        /// <typeparam name="T">An Enum.</typeparam>
        /// <param name="attributeType">A type that derives from BoolAttribute.</param>
        /// <param name="value">The value to search for.</param>
        /// <returns></returns>
        public static T GetEnumByBoolAttribute<T>(Type attributeType, bool value)
        {
            List<T> objList = GetEnumsByBoolAttribute<T>(attributeType, value);

            if (objList.Count == 0)
            {
                return default(T);
            }
            else if (objList.Count > 1)
            {
                throw new Exception(string.Format("More than one enum {0} has the {1} attribute set to {2}.",
                    typeof(T).Name, attributeType.Name, value));
            }

            return objList[0];
        }

        /// <summary>
        /// Gets the all the Enum values that have the given bool attribute set to value.
        /// The attribute needs to derive from BoolAttribute.
        /// There can only be one value in the Enum with this attribute set to true.
        /// </summary>
        /// <typeparam name="T">An Enum.</typeparam>
        /// <param name="attributeType">A type that derives from BoolAttribute.</param>
        /// <param name="value">The value to search for.</param>
        /// <returns></returns>
        public static List<T> GetEnumsByBoolAttribute<T>(Type attributeType, bool value)
        {
            if (!attributeType.IsSubclassOf(typeof(BoolAttribute)))
            {
                throw new Exception("Type needs to be inherited from BoolAttribute.\nCurrently " + attributeType.BaseType);
            }

            Type type = typeof(T);

            if (!type.IsEnum)
            {
                throw new Exception("Type needs to be enum.\nCurrently " + type.Name);
            }

            List<T> objList = new List<T>();
            foreach (string name in Enum.GetNames(type))
            {
                FieldInfo fi = type.GetField(name);
                BoolAttribute[] attributes =
                      (BoolAttribute[])fi.GetCustomAttributes(typeof(BoolAttribute), true);

                bool isSet = (attributes.Length > 0) ? attributes[0].IsSet : false;

                if (isSet == value)
                {
                    objList.Add((T)Enum.Parse(type, name));
                }
            }

            return objList;
        }

        /// <summary>
        /// Gets the Properties that have a the given BooleanAttribute (or derived) attribute set to true.
        /// </summary>
        /// <param name="boolAttributeType"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetPropertiesByAttributeBool<T>(Type boolAttributeType)
        {
            return GetPropertiesByAttributeBool<T>(boolAttributeType, true);
        }

        /// <summary>
        /// Gets the Properties that have a the given BooleanAttribute (or derived) attribute with the given value.
        /// </summary>
        /// <param name="boolAttributeType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetPropertiesByAttributeBool<T>(Type boolAttributeType, bool value)
        {
            if (!boolAttributeType.IsSubclassOf(typeof(BoolAttribute)))
            {
                throw new Exception(string.Format("Attribute type needs to derive from BoolAttribute. Currently {0}", boolAttributeType.Name));
            }

            List<PropertyInfo> propertyInfos = new List<PropertyInfo>();

            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                if (propertyInfo.PropertyType.IsPublic)  // using BindingFlags.Public in GetProperties() returns empty
                {
                    foreach (Attribute attribute in propertyInfo.GetCustomAttributes(boolAttributeType, true))
                    {
                        if (((BoolAttribute)attribute).IsSet == value)
                        {
                            propertyInfos.Add(propertyInfo);
                        }
                    }
                }
            }

            return propertyInfos.ToArray();
        }

        /// <summary>
        /// Returns all the properties of class T with the given attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetProperties<T>(Type attributeType)
        {
            return GetProperties(typeof(T), attributeType);
        }

        /// <summary>
        /// Returns all the properties of class objType with the given attribute.
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetProperties(Type objType, Type attributeType)
        {
            if (!attributeType.IsSubclassOf(typeof(Attribute)))
            {
                throw new Exception(string.Format("Attribute type needs to derive from Attribute. Currently {0}", attributeType.Name));
            }

            List<PropertyInfo> propertyInfos = new List<PropertyInfo>();

            foreach (PropertyInfo propertyInfo in objType.GetProperties())
            {
                if (propertyInfo.PropertyType.IsPublic)  // using BindingFlags.Public in GetProperties() returns empty
                {
                    object[] attrs = propertyInfo.GetCustomAttributes(attributeType, true);
                    if (attrs != null && attrs.Length > 0)
                    {
                        propertyInfos.Add(propertyInfo);
                    }
                }
            }

            return propertyInfos.ToArray();
        }
    }
}
