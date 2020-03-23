using SettingsProviderNet.Storages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace SettingsProviderNet
{
  // ReSharper disable InconsistentNaming

  public class SettingsProvider : ISettingsProvider
  {
    public SettingsProvider(ISettingsStorage2 storage, string secretKey = null)
    {
      this._storage = storage;
      this._secretKey = secretKey;
    }

    public virtual T GetSettings<T>(bool fresh = false) where T : new()
    {
      var type = typeof(T);
      if (!fresh && _cache.ContainsKey(type))
        return (T)_cache[type];

      bool isFileNotExist = _storage.TryLoad(out var settingsLookup);
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

      _cache[typeof(T)] = settings;

      if (isFileNotExist && _storage.Config.CreateIfNotExist)
        SaveSettings<T>(settings);

      return settings;
    }

    object GetDefaultValue(ISettingDescriptor setting)
    {
      var value = setting.DefaultValue ?? ConvertValue(null, setting);

      if (setting.IsProtected && value != null)
        value = ProtectedDataUtils.Encrypt((string)value, _secretKey ?? typeof(SettingDescriptor).FullName);

      return value;
    }

    object ConvertValue(string storedValue, ISettingDescriptor setting)
    {
      var propertyType = setting.Property.PropertyType;
      var isList = IsList(propertyType);
      if (isList && string.IsNullOrEmpty(storedValue)) return CreateListInstance(propertyType);
      if (string.IsNullOrEmpty(storedValue)) return GetDefault(propertyType);
      if (setting.UnderlyingType.IsEnum) return Enum.Parse(setting.UnderlyingType, storedValue);
      if (!string.IsNullOrEmpty(storedValue) && setting.UnderlyingType == typeof(string) && !storedValue.StartsWith("\""))
        storedValue = string.Format("\"{0}\"", storedValue);
      if (setting.UnderlyingType == typeof(bool))
        storedValue = storedValue.ToLower();

      return new DataContractJsonSerializer(setting.UnderlyingType)
          .ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(storedValue)));
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

    public virtual void SaveSettings<T>(T settingsToSave)
    {
      _cache[typeof(T)] = settingsToSave;

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
          var writer = JsonReaderWriterFactory.CreateJsonWriter(ms, Encoding.UTF8);
          new DataContractJsonSerializer(setting.UnderlyingType).WriteObject(ms, value);
          writer.Flush();
          var jsonString = Encoding.UTF8.GetString(ms.ToArray());

          settings[setting.Key] = jsonString;
        }
        else
        {
          settings[setting.Key] = string.Empty;
        }
      }
      _storage.Save(settings);
    }

    internal static string GetLegacyKey<T>(ISettingDescriptor setting)
    {
      var settingsType = typeof(T);

      return string.Format("{0}.{1}", settingsType.FullName, setting.Property.Name);
    }

    public virtual IEnumerable<ISettingDescriptor> ReadSettingMetadata<T>()
    {
      return ReadSettingMetadata(typeof(T));
    }

    public virtual IEnumerable<ISettingDescriptor> ReadSettingMetadata(Type settingsType)
    {
      return settingsType.GetProperties()
          .Where(x => x.CanRead && x.CanWrite)
          .Select(x => new SettingDescriptor(x, _secretKey))
          .ToArray();
    }

    public virtual T ResetToDefaults<T>() where T : new()
    {
      _storage.Save(new Dictionary<string, string>());

      var type = typeof(T);
      if (_cache.ContainsKey(type))
      {
        var cachedCopy = _cache[type];
        var settingMetadata = ReadSettingMetadata<T>();

        foreach (var setting in settingMetadata)
        {
          setting.Write(cachedCopy, GetDefaultValue(setting));
        }

        return (T)cachedCopy;
      }

      return GetSettings<T>();
    }

    readonly ISettingsStorage2 _storage;
    readonly Dictionary<Type, object> _cache = new Dictionary<Type, object>();
    readonly string _secretKey;
  }
}