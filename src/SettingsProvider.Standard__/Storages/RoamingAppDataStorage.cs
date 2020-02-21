using System;
using System.IO;

namespace SettingsProviderNet
{
  public class RoamingAppDataStorage : JsonSettingsStoreBase
  {
    private readonly string folderName;

    public RoamingAppDataStorage(string folderName)
    {
      this.folderName = folderName;
    }

    protected override void WriteTextFile(string filename, string fileContents)
    {
      var settingsFolder = GetSettingsFolder();
      if (!Directory.Exists(settingsFolder))
        Directory.CreateDirectory(settingsFolder);
      File.WriteAllText(Path.Combine(settingsFolder, filename), fileContents);
    }

    protected override string ReadTextFile(string filename)
    {
      var settingsFilename = Path.Combine(GetSettingsFolder(), filename);
      return File.Exists(settingsFilename) ? File.ReadAllText(settingsFilename) : null;
    }

    private string GetSettingsFolder()
    {
      var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
      var settingsFilename = Path.Combine(folderPath, folderName);
      return settingsFilename;
    }
  }
}