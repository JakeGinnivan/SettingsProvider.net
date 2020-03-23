using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace SettingsProviderNet.Storages
{
  public class JsonSettingsStorage : ISettingsStorage2
  {
    static JsonSettingsStorage()
    {
      _serializerOptions = new JsonSerializerOptions();
      _serializerOptions.WriteIndented = true;
    }

    public void Configure(StorageOptions config)
    {
      if (Config != null)
        throw new InvalidOperationException("already specified");

      Config = config ?? throw new ArgumentNullException();
      _pathToConfigFile = Config.GetPath();
    }

    public bool TryLoad(out IReadOnlyDictionary<string, string> config)
    {
      if (Config == null)
        throw new InvalidOperationException("unconfigured");

      if (!File.Exists(_pathToConfigFile))
      {
        config = _empty;
        return false;
      }

      try
      {
        var json = File.ReadAllText(_pathToConfigFile);
        config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        return true;
      }
      catch (Exception ex)
      {
        throw new IOException("Could not read", ex);
      }
    }

    public void Save(Dictionary<string, string> settings)
    {
      if (Config == null)
        throw new InvalidOperationException("unconfigured");

      try
      {
        var dirs = Path.GetDirectoryName(_pathToConfigFile);
        Directory.CreateDirectory(dirs);

        var json = JsonSerializer.Serialize(settings, _serializerOptions);

        // write via a temporary file to avoid problems with a damaged settings 
        //file when the process terminates during file writing
        var tempPath = Path.GetTempFileName();
        File.WriteAllText(tempPath, json);
        if (File.Exists(_pathToConfigFile))
          File.Replace(tempPath, _pathToConfigFile, _pathToConfigFile + ".bak");
        else
          File.Move(tempPath, _pathToConfigFile);
      }
      catch (Exception ex)
      {
        throw new IOException("Could not save", ex);
      }
    }

    public StorageOptions Config { get; private set; }

    private string _pathToConfigFile;
    private static readonly JsonSerializerOptions _serializerOptions;
    private static readonly Dictionary<string, string> _empty = new Dictionary<string, string>(0);
  }
}
