using System.Collections.Generic;

namespace SettingsProviderNet
{
    public interface ISettingsStorage
    {
        void Save(string key, Dictionary<string, string> settings);
        Dictionary<string, string> Load(string key);
    }
}