using System;
using System.Collections.Generic;
using System.Text;

namespace SettingsProviderNet
{
  public interface ISettingsStorage2
  {
    void Configure();
    void Save(Dictionary<string, string> settings);
    IReadOnlyDictionary<string, string> Load();
  }
}
