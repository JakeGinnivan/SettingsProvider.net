using System;
using System.ComponentModel;
using System.Reflection;

namespace SettingsProviderNet
{
    public interface ISettingDescriptor
    {
        void ReadAttribute<TAttribute>(Action<TAttribute> callback);
        PropertyInfo Property { get; }
        object DefaultValue { get; }
        string Description { get; }
        string DisplayName { get; }
        string Key { get; }

        bool IsProtected { get; }

        /// <summary>
        /// If the property type is nullable, returns the type. i.e int? returns int
        /// </summary>
        Type UnderlyingType { get; }

        void Write(object settings, object value);
        object ReadValue(object settings);
        event PropertyChangedEventHandler PropertyChanged;
    }
}