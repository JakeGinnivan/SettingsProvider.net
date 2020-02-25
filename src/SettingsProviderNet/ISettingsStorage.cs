using System.Collections.Generic;

namespace SettingsProviderNet
{
  public interface ISettingsStorage
  {
    void Save(string key, Dictionary<string, string> settings);
    IReadOnlyDictionary<string, string> Load(string key);
  }
}