using System.Collections.Generic;

namespace SettingsProviderNet
{
    public interface ISettingsStorage
    {
        string SerializeList(List<string> listOfItems);
        List<string> DeserializeList(string serializedList);
        void Save(string key, Dictionary<string, string> settings);
        Dictionary<string, string> Load(string key);
    }
}