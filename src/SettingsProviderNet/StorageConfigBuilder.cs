using System;
using System.IO;

namespace SimpleSettingsStorage
{
  /// <summary> 
  /// Necessarily you should specify AppName xor SpecifyPathToConfigFile.
  /// Optionally you might set SpecialFolder, FileName, etc.
  /// </summary>
  public class StorageConfigBuilder
  {
    public StorageConfigBuilder SpecifyPathToConfigFile(string path)
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

      _config.PathToTargetFile = path;
      return this;
    }

    public StorageConfigBuilder SetFolder(Environment.SpecialFolder specialFolder)
    {
      if (_config.PathToTargetFile != null)
        throw new InvalidOperationException("Target file already specified");

      _config.SpecialFolder = specialFolder;
      return this;
    }

    public StorageConfigBuilder SetAppName(string appName)
    {
      if (_config.PathToTargetFile != null)
        throw new InvalidOperationException("Target file already specified");

      _config.AppName = appName;
      return this;
    }

    public StorageConfigBuilder FileName(string fileName)
    {
      if (_config.PathToTargetFile != null)
        throw new InvalidOperationException("Target file already specified");

      _config.FileName = fileName;
      return this;
    }

    /// <summary>
    /// Automaticaly settings file creation if not exist
    /// </summary>
    public StorageConfigBuilder CreateIfNotExist(bool value)
    {
      _config.CreateIfNotExist = value;
      return this;
    }

    public StorageConfig Build()
    {
      if (_config.PathToTargetFile != null)
        return _config;

      if (_config.AppName == null)
        throw new InvalidOperationException($"{nameof(_config.AppName)} expected");

      if (_config.FileName == null)
        _config.FileName = DefaultFileName;

      if (_config.SpecialFolder == null)
        _config.SpecialFolder = DefaultSpecialFolder;

      return _config;
    }

    private readonly StorageConfig _config = new StorageConfig();
    private const string DefaultFileName = "Settings.json";
    private const Environment.SpecialFolder DefaultSpecialFolder = Environment.SpecialFolder.LocalApplicationData;
  }
}
