using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SettingsProviderNet
{
  public class StorageOptions
  {
    internal StorageOptions() { }

    public static StorageConfigBuilder Create() => new StorageConfigBuilder();

    public bool CreateIfNotExist { get; internal set; }

    public Environment.SpecialFolder? SpecialFolder { get; internal set; }

    public string AppName { get; internal set; }

    public string FileName { get; internal set; }

    public string PathToTargetFile { get; internal set; }

    public string GetPath()
    {
      return PathToTargetFile ?? Path.Combine(Environment.GetFolderPath(SpecialFolder.Value), AppName, FileName);
    }
  }
}
