using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace SettingsProviderNet
{
    public class SettingDescriptor : INotifyPropertyChanged, ISettingDescriptor
    {
        readonly PropertyInfo property;
        readonly string secretKey;

        public SettingDescriptor(PropertyInfo property, string secretKey = null)
        {
            this.property = property;
            this.secretKey = secretKey ?? GetType().FullName;
            DisplayName = property.Name;
            Key = property.Name;

            ReadAttribute<DefaultValueAttribute>(d => DefaultValue = d.Value);
            ReadAttribute<DescriptionAttribute>(d => Description = d.Description);
            ReadAttribute<DisplayNameAttribute>(d => DisplayName = d.DisplayName);
            ReadAttribute<DataMemberAttribute>(d => Key = d.Name);
            ReadAttribute<KeyAttribute>(d => Key = d.Key);
            ReadAttribute<ProtectedStringAttribute>(d => IsProtected = true);
        }

        public void ReadAttribute<TAttribute>(Action<TAttribute> callback)
        {
            var instances = property.GetCustomAttributes(typeof(TAttribute), true).OfType<TAttribute>();
            foreach (var instance in instances)
            {
                callback(instance);
            }
        }

        public PropertyInfo Property
        {
            get { return property; }
        }

        public object DefaultValue { get; private set; }

        public string Description { get; private set; }
        
        public string DisplayName { get; private set; }

        public string Key { get; private set; }

        public bool IsProtected { get; private set; }

        public virtual void Write(object settings, object value)
        {
            property.SetValue(settings, IsProtected ? Decrypt(value) : value, null);
        }

        protected virtual object Encrypt(object value)
        {
            var str = value as string;
            return String.IsNullOrEmpty(str) ? value : ProtectedDataUtils.Encrypt(str, secretKey);
        }

        protected virtual object Decrypt(object value)
        {
            var str = value as string;
            return String.IsNullOrEmpty(str) ? value : ProtectedDataUtils.Decrypt(str, secretKey);
        }

        /// <summary>
        /// If the property type is nullable, returns the type. i.e int? returns int
        /// </summary>
        public Type UnderlyingType
        {
            get
            {
                if (Property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    return property.PropertyType.GetGenericArguments()[0];
                return property.PropertyType;
            }
        }

        public virtual object ReadValue(object settings)
        {
            var value = property.GetValue(settings, null);
            return IsProtected ? Encrypt(value) : value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}