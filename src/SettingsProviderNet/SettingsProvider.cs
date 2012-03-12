using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;

namespace SettingsProviderNet
{
    // ReSharper disable InconsistentNaming
    public interface ISettingsProvider
    {
        T GetSettings<T>() where T : new();
        void SaveSettings<T>(T settings);
        IEnumerable<SettingsProvider.SettingDescriptor> ReadSettingMetadata<T>();
        IEnumerable<SettingsProvider.SettingDescriptor> ReadSettingMetadata(Type settingsType);
    }

    public interface ISettingsStorage
    {
        void Save(string key, Dictionary<string, string> settings);
        Dictionary<string, string> Load(string key);
    }

    public class IsolatedStorageSettingsStore : ISettingsStorage
    {
        const IsolatedStorageScope Scope = IsolatedStorageScope.Assembly | IsolatedStorageScope.User | IsolatedStorageScope.Roaming;

        public void Save(string key, Dictionary<string, string> settings)
        {
            var serializer = new DataContractJsonSerializer(typeof(Dictionary<string, string>));

            using (var isoStore = IsolatedStorageFile.GetStore(Scope, null, null))
            using (var stream = new IsolatedStorageFileStream(key, FileMode.Create, isoStore))
                serializer.WriteObject(stream, settings);
        }

        public Dictionary<string, string> Load(string key)
        {
            var serializer = new DataContractJsonSerializer(typeof(Dictionary<string, string>));
            using (var isoStore = IsolatedStorageFile.GetStore(Scope, null, null))
            {
                if (isoStore.FileExists(key))
                {
                    using (var stream = new IsolatedStorageFileStream(key, FileMode.Open, isoStore))
                        return (Dictionary<string, string>)serializer.ReadObject(stream);
                }
            }

            return new Dictionary<string, string>();
        }
    }

    public class SettingsProvider : ISettingsProvider
    {
        const string NotConvertableMessage = "Settings provider only supports types that Convert.ChangeType supports. See http://msdn.microsoft.com/en-us/library/dtb69x08.aspx";
        readonly ISettingsStorage settingsRepository;

        public SettingsProvider(ISettingsStorage settingsRepository = null)
        {
            this.settingsRepository = settingsRepository ?? new IsolatedStorageSettingsStore();
        }

        public T GetSettings<T>() where T : new()
        {
            var settingsLookup = settingsRepository.Load(FileKey<T>());
            var settings = new T();
            var settingMetadata = ReadSettingMetadata<T>();

            foreach (var setting in settingMetadata)
            {
                // Initialize with default values
                setting.Write(settings, setting.DefaultValue);

                // Write over it using the stored value if exists
                var key = GetKey<T>(setting);
                if (settingsLookup.ContainsKey(key))
                {
                    setting.Write(settings, ConvertValue(settingsLookup[key], setting));
                }
            }

            return settings;
        }

        static string FileKey<T>()
        {
            return typeof(T).Name + ".settings";
        }

        static object ConvertValue(string storedValue, SettingDescriptor setting)
        {
            if (storedValue == null) return null;
            if (setting.UnderlyingType == typeof(string)) return storedValue;
            if (setting.UnderlyingType != typeof(string) && string.IsNullOrEmpty(storedValue)) return null;
            if (setting.UnderlyingType.IsEnum) return Enum.Parse(setting.UnderlyingType, storedValue, false);

            object converted;
            try
            {
                converted = Convert.ChangeType(storedValue, setting.UnderlyingType, CultureInfo.InvariantCulture);
            }
            catch (InvalidCastException ex)
            {
                throw new NotSupportedException(NotConvertableMessage, ex);
            }
            catch (FormatException ex)
            {
                throw new NotSupportedException(NotConvertableMessage, ex);
            }

            return converted;
        }

        public void SaveSettings<T>(T settingsToSave)
        {
            var settings = new Dictionary<string, string>();
            var settingsMetadata = ReadSettingMetadata<T>();

            foreach (var setting in settingsMetadata)
            {
                var value = setting.ReadValue(settingsToSave) ?? setting.DefaultValue;
                if (value == null && setting.UnderlyingType.IsEnum)
                    value = Enum.GetValues(setting.UnderlyingType).Cast<object>().First();
                settings[GetKey<T>(setting)] = Convert.ToString(value ?? string.Empty, CultureInfo.InvariantCulture);
            }
            settingsRepository.Save(FileKey<T>(), settings);
        }

        internal static string GetKey<T>(SettingDescriptor setting)
        {
            var settingsType = typeof(T);

            return string.Format("{0}.{1}", settingsType.FullName, setting.Property.Name);
        }

        public IEnumerable<SettingDescriptor> ReadSettingMetadata<T>()
        {
            return ReadSettingMetadata(typeof(T));
        }

        public IEnumerable<SettingDescriptor> ReadSettingMetadata(Type settingsType)
        {
            return settingsType.GetProperties()
                .Where(x => x.CanRead && x.CanWrite)
                .Select(x => new SettingDescriptor(x))
                .ToArray();
        }

        public class SettingDescriptor : INotifyPropertyChanged
        {
            readonly PropertyInfo property;

            public SettingDescriptor(PropertyInfo property)
            {
                this.property = property;
                DisplayName = property.Name;

                ReadAttribute<DefaultValueAttribute>(d => DefaultValue = d.Value);
                ReadAttribute<DescriptionAttribute>(d => Description = d.Description);
                ReadAttribute<DisplayNameAttribute>(d => DisplayName = d.DisplayName);
            }

            void ReadAttribute<TAttribute>(Action<TAttribute> callback)
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

            public void Write(object settings, object value)
            {
                property.SetValue(settings, value, null);
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

            public object ReadValue(object settings)
            {
                return property.GetValue(settings, null);
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }
    }
    // ReSharper restore InconsistentNaming
}