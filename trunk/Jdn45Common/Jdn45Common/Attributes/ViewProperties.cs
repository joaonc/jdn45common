using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;

///
/// These classes define attributes not found in System.ComponentModel.
/// 
/// 
/// Below is a list of attribures that can be used from System.ComponentModel:
/// Note that the class names end with 'Attribute'
/// 
/// Attributes useful for DataGridView:
///   Browsable(false)               - Don't show
///   ReadOnly(true)                 - Make read only (unable to edit)
///   DisplayName("my display name") - Set the display name (not localizable)
///                                    Use DisplayNameLocalized in Jdn2Common.Attributes instead
///
/// Attributes useful for PropertyGrid:
///   Category        - This attribute places your property in the appropriate category in a node on the property grid.
///   Description     - This attribute places a description of your property at the bottom of the property grid
///   Browsable       - This is used to determine whether or not the property is shown or hidden in the property grid
///   ReadOnly        - Use this attribute to make your property read only inside the property grid
///   DefaultValue    - Specifies the default value of the property shown in the property grid
///   DefaultProperty - If placed above a property, this property gets the focus when the property grid is first launched.
///                     Unlike the other attributes, this attribute goes above the class. 
/// 
/// 
/// Attributes for Xml serialization from System.Xml.Serialization:
///   XmlIgnore - Don't serialize
///   
namespace Jdn45Common.Attributes
{
    /// <summary>
    /// Whether the field is a Compounded object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
    public class IsCompoundAttribute : BoolAttribute
    {
        public IsCompoundAttribute() : base() { }
        public IsCompoundAttribute(bool isCompound) : base(isCompound) { }
    }

    /// <summary>
    /// Marks it as the primary key.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
    public class PrimaryKeyAttribute : BoolAttribute
    {
        public PrimaryKeyAttribute() : base() { }
        public PrimaryKeyAttribute(bool isPrimaryKey) : base(isPrimaryKey) { }
    }
    
    /// <summary>
    /// Make a Foreign Key.
    /// The FK needs to be a property.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyAttribute : System.Attribute
    {
        private PropertyInfo foreignKey;

        public ForeignKeyAttribute(string className)
        {
            SetForeignKey(Type.GetType(className), "Id");
        }

        public ForeignKeyAttribute(Type type)
        {
            SetForeignKey(type, "Id");
        }

        public ForeignKeyAttribute(string className, string propertyName)
        {
            SetForeignKey(Type.GetType(className), propertyName);
        }

        public ForeignKeyAttribute(Type type, string propertyName)
        {
            SetForeignKey(type, propertyName);
        }

        private void SetForeignKey(Type type, string propertyName)
        {
            PropertyInfo pi = type.GetProperty(propertyName);

            if (pi == null)
            {
                throw new Exception(string.Format("Property {0} not found for class {1}",
                    propertyName, type.Name));
            }

            // Check that the Foreign Key is Primary Key (has the PrimaryKey attribute) or Id (has the IsId attribute)
            object[] isIdAttribute = pi.GetCustomAttributes(typeof(IdentityAttribute), true);
            bool isId = true;
            if (isIdAttribute == null || isIdAttribute.Length == 0)
            {
                isId = false;
            }
            else if (isIdAttribute.Length > 1)
            {
                throw new Exception(string.Format("{0} attribute defined more than once.", typeof(IdentityAttribute).Name));
            }
            else if (!((IdentityAttribute)isIdAttribute[0]).IsSet)
            {
                isId = false;
            }

            object[] isPkAttribute = pi.GetCustomAttributes(typeof(PrimaryKeyAttribute), true);
            bool isPk = true;
            if (isPkAttribute == null || isPkAttribute.Length == 0)
            {
                isPk = false;
            }
            else if (isPkAttribute.Length > 1)
            {
                throw new Exception(string.Format("{0} attribute defined more than once.", typeof(PrimaryKeyAttribute).Name));
            }
            else if (!((PrimaryKeyAttribute)isPkAttribute[0]).IsSet)
            {
                isPk = false;
            }

            if (!(isPk || isId))
            {
                throw new Exception(string.Format("Property {0} in {1} does not have the {2} attribute set.",
                    pi.Name, pi.DeclaringType.Name, typeof(IdentityAttribute).Name));
            }

            foreignKey = pi;
        }

        public PropertyInfo ForeignKey { get { return foreignKey; } }
    }

    /// <summary>
    /// Suggests the index.
    /// Does not guarantee it as primary keys and other criteria may override it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
    public class IndexAttribute : IntAttribute
    {
        public IndexAttribute() : base() { }
        public IndexAttribute(int index) : base(index) { }
    }

    /// <summary>
    /// Marks the field as mandatory.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
    public class MandatoryAttribute : BoolAttribute
    {
        public MandatoryAttribute() : base() { }
        public MandatoryAttribute(bool isMandatory) : base(isMandatory) { }
    }

    /// <summary>
    /// Marks the field as password.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
    public class PasswordAttribute : BoolAttribute
    {
        public PasswordAttribute() : base() { }
        public PasswordAttribute(bool isPassword) : base(isPassword) { }
    }

    /// <summary>
    /// Localized Display Name of a field.
    /// The name of the resource that contains the string that will show in the display.
    /// This is needed to localize properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
    public class DisplayNameLocalizedAttribute : DisplayNameAttribute
    {
        public DisplayNameLocalizedAttribute(Type resourceManagerProvider, string resourceKey)
            : base(Util.LookupResource(resourceManagerProvider, resourceKey))
        {
        }

        public DisplayNameLocalizedAttribute(Type resourceManagerProvider, string resourceKey, string prepend, string append)
            : base(Util.LookupResource(resourceManagerProvider, resourceKey))
        {
            this.DisplayNameValue = prepend + this.DisplayNameValue + append;
        }
    }
    
    /// <summary>
    /// Localized Category in a Property Grid.
    /// The name of the resource that contains the string that will show in the category.
    /// This is needed to localize categories.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class CategoryLocalizedAttribute : CategoryAttribute
    {
        public CategoryLocalizedAttribute(Type resourceManagerProvider, string resourceKey)
            : base(Util.LookupResource(resourceManagerProvider, resourceKey))
        {
        }

        public string GetLocalizedString()
        {
            return Category;  // Already localized
        }
    }

    /// <summary>
    /// Localized Description of a field.
    /// The name of the resource that contains the string that will show in the display.
    /// This is needed to localize properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
    public class DescriptionLocalizedAttribute : DescriptionAttribute
    {
        public DescriptionLocalizedAttribute(Type resourceManagerProvider, string resourceKey)
            : base(Util.LookupResource(resourceManagerProvider, resourceKey))
        {
        }

        public DescriptionLocalizedAttribute(Type resourceManagerProvider, string resourceKey, string prepend, string append)
            : base(Util.LookupResource(resourceManagerProvider, resourceKey))
        {
            this.DescriptionValue = prepend + this.DescriptionValue + append;
        }
    }

    /// <summary>
    /// Localized name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
    public class NameAttribute : LocalizedStringAttribute
    {
        public NameAttribute(Type resourceManagerProvider, string resourceKey)
            : base(resourceManagerProvider, resourceKey)
        {
        }

        public NameAttribute(Type resourceManagerProvider, string resourceKey, string prepend, string append)
            : base(resourceManagerProvider, resourceKey, prepend, append)
        {
        }
    }

    /// <summary>
    /// Whether it's active.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
    public class IsActiveAttribute : BoolAttribute
    {
        public IsActiveAttribute() : base() { }
        public IsActiveAttribute(bool isActive) : base(isActive) { }
    }

    /// <summary>
    /// Whether it's new.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
    public class IsNewAttribute : BoolAttribute
    {
        public IsNewAttribute() : base() { }
        public IsNewAttribute(bool isNew) : base(isNew) { }
    }

    /// <summary>
    /// Whether it's current or the current one.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
    public class IsCurrentAttribute : BoolAttribute
    {
        public IsCurrentAttribute() : base() { }
        public IsCurrentAttribute(bool isCurrent) : base(isCurrent) { }
    }

    /// <summary>
    /// Whether the field is an Identity, which means the values are auto-generated.
    /// Reflects the IDENTITY keyword in SQL.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
    public class IdentityAttribute : BoolAttribute
    {
        public IdentityAttribute() : base() { }
        public IdentityAttribute(bool isIdentity) : base(isIdentity) { }
    }

    /// <summary>
    /// Whether cannot be null.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
    public class NotNullAttribute : BoolAttribute
    {
        public NotNullAttribute() : base() { }
        public NotNullAttribute(bool isCurrent) : base(isCurrent) { }
    }

    /// <summary>
    /// Whether the method that is marked with NotNullAttribute can have the values considered undefined.
    /// This is due to literal values (ex. int) do not have null. So .Net can't tell if, for example,
    /// an int with value 0 represents DbNull or it's the actual value 0.
    /// To see which values are considered undefined, see BaseDAO.IsDefined(...).
    /// This attribute is only meaningful with the NotNullAttribute set to true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
    public class CanBeUndefineddAttribute : BoolAttribute
    {
        public CanBeUndefineddAttribute() : base() { }
        public CanBeUndefineddAttribute(bool isCurrent) : base(isCurrent) { }
    }
}
