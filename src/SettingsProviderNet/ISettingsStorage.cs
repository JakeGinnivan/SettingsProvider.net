using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSettingsStorage
{
  public interface ISettingsStorage
  {
    void Configure(StorageConfig config);
    void Save(Dictionary<string, string> settings);
    bool TryLoad(out IReadOnlyDictionary<string, string> values);
    StorageConfig Config { get; }
  }
}
