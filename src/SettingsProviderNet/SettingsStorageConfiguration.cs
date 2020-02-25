using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SettingsProviderNet
{
  public class SettingsStorageConfiguration
  {
    internal SettingsStorageConfiguration() { }

    public Environment.SpecialFolder? SpecialFolder { get; internal set; }

    public string AppName { get; internal set; }

    public string FileName { get; internal set; }

    public string TargetFileName { get; internal set; }
  }

  public class SettingsStorageConfigurationBuilder
  {
    public SettingsStorageConfigurationBuilder SpecifyFile(string path)
    {
      if (string.IsNullOrEmpty(path))
        throw new ArgumentException(nameof(path));

      if (_config.SpecialFolder != null)
        throw new InvalidOperationException("target special folder already specified");

      try
      {
        var dir = Path.GetDirectoryName(path);
        var name = Path.GetFileNameWithoutExtension(path);
        var ext = Path.GetExtension(path);
      }
      catch (Exception ex)
      {
        throw new ArgumentException(ex.Message);
      }

      _config.TargetFileName = path;
      return this;
    }

    public SettingsStorageConfigurationBuilder SetFolder(Environment.SpecialFolder specialFolder)
    {
      if (_config.TargetFileName != null)
        throw new InvalidOperationException("Target file already specified");

      _config.SpecialFolder = specialFolder;
      return this;
    }

    public SettingsStorageConfigurationBuilder SetAppName(string appName)
    {
      if (_config.TargetFileName != null)
        throw new InvalidOperationException("Target file already specified");

      _config.AppName = appName;
      return this;
    }

    public SettingsStorageConfigurationBuilder FileName(string fileName)
    {
      if (_config.TargetFileName != null)
        throw new InvalidOperationException("Target file already specified");

      _config.FileName = fileName;
      return this;
    }

    public SettingsStorageConfiguration Build()
    {
      if (_config.TargetFileName != null)
        return _config;

      if (_config.AppName == null)
        throw new InvalidOperationException($"{nameof(_config.AppName)} expected");

      if (_config.FileName == null)
        _config.FileName = DefaultFileName;

      if (_config.SpecialFolder == null)
        _config.SpecialFolder = DefaultSpecialFolder;

      return _config;
    }

    private readonly SettingsStorageConfiguration _config = new SettingsStorageConfiguration();
    private const string DefaultFileName = "Settings.settings";
    private const Environment.SpecialFolder DefaultSpecialFolder = Environment.SpecialFolder.LocalApplicationData;
  }
}
