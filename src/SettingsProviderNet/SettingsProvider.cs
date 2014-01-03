using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace SettingsProviderNet
{
    // ReSharper disable InconsistentNaming

    public class SettingsProvider : ISettingsProvider
    {
        readonly ISettingsStorage settingsRepository;
        readonly Dictionary<Type, object> cache = new Dictionary<Type, object>();

        public SettingsProvider(ISettingsStorage settingsRepository = null)
        {
            this.settingsRepository = settingsRepository ?? new IsolatedStorageSettingsStore();
        }

        public T GetSettings<T>(bool fresh = false) where T : new()
        {
            var type = typeof (T);
            if (!fresh && cache.ContainsKey(type))
                return (T)cache[type];

            var settingsLookup = settingsRepository.Load(GetKey<T>());
            var settings = new T();
            var settingMetadata = ReadSettingMetadata<T>();

            foreach (var setting in settingMetadata)
            {
                // Write over it using the stored value if exists
                var legacyKey = GetLegacyKey<T>(setting);
                object value;
                if (settingsLookup.ContainsKey(setting.Key)) 
                    value = ConvertValue(settingsLookup[setting.Key], setting);
                else if (settingsLookup.ContainsKey(legacyKey))
                    value = ConvertValue(settingsLookup[legacyKey], setting);
                else
                    value = GetDefaultValue(setting);

                setting.Write(settings, value);
            }

            cache[typeof(T)] = settings;

            return settings;
        }

        object GetDefaultValue(SettingDescriptor setting)
        {
            return setting.DefaultValue ?? ConvertValue(null, setting);
        }

        static string GetKey<T>()
        {
            return typeof(T).Name;
        }

        object ConvertValue(string storedValue, SettingDescriptor setting)
        {
            var propertyType = setting.Property.PropertyType;
            var isList = IsList(propertyType);
            if (isList && string.IsNullOrEmpty(storedValue)) return CreateListInstance(propertyType);
            if (string.IsNullOrEmpty(storedValue)) return GetDefault(propertyType);
            if (setting.UnderlyingType.IsEnum) return Enum.Parse(setting.UnderlyingType, storedValue);
            if (!string.IsNullOrEmpty(storedValue) && setting.UnderlyingType == typeof(string) && !storedValue.StartsWith("\""))
                storedValue = string.Format("\"{0}\"", storedValue);
            if (setting.UnderlyingType == typeof (bool))
                storedValue = storedValue.ToLower();

            return new DataContractJsonSerializer(setting.UnderlyingType)
                .ReadObject(new MemoryStream(Encoding.Default.GetBytes(storedValue)));
        }

        static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        private static object CreateListInstance(Type propertyType)
        {
            return Activator.CreateInstance(propertyType.IsClass ? propertyType : typeof(List<>).MakeGenericType(propertyType.GetGenericArguments()[0]));
        }

        private static bool IsList(Type propertyType)
        {
            return
                typeof(IList).IsAssignableFrom(propertyType) ||
                (propertyType.IsGenericType && typeof(IList<>) == propertyType.GetGenericTypeDefinition());
        }

        public void SaveSettings<T>(T settingsToSave)
        {
            cache[typeof (T)] = settingsToSave;

            var settings = new Dictionary<string, string>();
            var settingsMetadata = ReadSettingMetadata<T>();

            foreach (var setting in settingsMetadata)
            {
                var value = setting.ReadValue(settingsToSave) ?? setting.DefaultValue;
                // Give enum a default
                if (setting.UnderlyingType.IsEnum)
                {
                    if (value == null)
                        value = EnumHelper.GetValues(setting.UnderlyingType).First();

                    settings[setting.Key] = value.ToString();
                }
                else if (value != null)
                {
                    var ms = new MemoryStream();
                    var writer = JsonReaderWriterFactory.CreateJsonWriter(ms, Encoding.Unicode);
                    new DataContractJsonSerializer(setting.UnderlyingType).WriteObject(ms, value);
                    writer.Flush();
                    var jsonString = Encoding.Default.GetString(ms.ToArray());

                    settings[setting.Key] = jsonString;
                }
                else
                {
                    settings[setting.Key] =  string.Empty;
                }
            }
            settingsRepository.Save(GetKey<T>(), settings);
        }
        
        internal static string GetLegacyKey<T>(SettingDescriptor setting)
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

        public T ResetToDefaults<T>() where T : new()
        {
            settingsRepository.Save(GetKey<T>(), new Dictionary<string, string>());

            var type = typeof (T);
            if (cache.ContainsKey(type))
            {
                var cachedCopy = cache[type];
                var settingMetadata = ReadSettingMetadata<T>();

                foreach (var setting in settingMetadata)
                {
                    setting.Write(cachedCopy, GetDefaultValue(setting));
                }

                return (T)cachedCopy;
            }

            return GetSettings<T>();
        }

        public class SettingDescriptor : INotifyPropertyChanged
        {
            readonly PropertyInfo property;

            public SettingDescriptor(PropertyInfo property)
            {
                this.property = property;
                DisplayName = property.Name;
                Key = property.Name;

                ReadAttribute<DefaultValueAttribute>(d => DefaultValue = d.Value);
                ReadAttribute<DescriptionAttribute>(d => Description = d.Description);
                ReadAttribute<DisplayNameAttribute>(d => DisplayName = d.DisplayName);
                ReadAttribute<DataMemberAttribute>(d => Key = d.Name);
                ReadAttribute<KeyAttribute>(d => Key = d.Key);
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

            public string Key { get; private set; }

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

            protected virtual void OnPropertyChanged(string propertyName)
            {
                var handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    // ReSharper restore InconsistentNaming
}