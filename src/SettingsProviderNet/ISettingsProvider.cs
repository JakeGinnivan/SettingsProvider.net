using System;
using System.Collections.Generic;

namespace SettingsProviderNet
{
    public interface ISettingsProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="freshCopy">If true, does not fetch from cache (useful for isolated editing)</param>
        /// <returns></returns>
        T GetSettings<T>(bool freshCopy = false) where T : new();
        void SaveSettings<T>(T settings);
        IEnumerable<ISettingDescriptor> ReadSettingMetadata<T>();
        IEnumerable<ISettingDescriptor> ReadSettingMetadata(Type settingsType);
        T ResetToDefaults<T>() where T : new();
    }
}