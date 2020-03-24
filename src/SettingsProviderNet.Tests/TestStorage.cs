using System.Collections.Generic;

namespace SettingsProviderNet.Tests
{
  public class TestStorage : ISettingsStorage
  {
    private Dictionary<string, string> _settings = new Dictionary<string, string>();

    public Dictionary<string, string> Files
    {
      get { return _settings; }
    }

    public StorageOptions Config { get; private set; }

    public void Configure(StorageOptions config)
    {
      Config = config;
    }

    public void Save(Dictionary<string, string> settings)
    {
      _settings = new Dictionary<string, string>(settings);
    }

    public bool TryLoad(out IReadOnlyDictionary<string, string> values)
    {
      if (_settings == null)
      {
        values = new Dictionary<string, string>();
        return false;
      }

      values = _settings;
      return true;
    }
  }
}