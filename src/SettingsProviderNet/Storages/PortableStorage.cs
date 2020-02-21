using System.IO;
using System.Reflection;

namespace SettingsProviderNet
{
  public class PortableStorage : JsonSettingsStoreBase
  {
    public PortableStorage()
    {
    }

    protected override void WriteTextFile(string filename, string fileContents)
    {
      var settingsFilename = Path.Combine(GetProgramFolder(), filename);
      File.WriteAllText(settingsFilename, fileContents);
    }

    protected override string ReadTextFile(string filename)
    {
      var settingsFilename = Path.Combine(GetProgramFolder(), filename);
      return File.Exists(settingsFilename) ? File.ReadAllText(settingsFilename) : null;
    }

    private string GetProgramFolder()
    {
      return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
    }
  }
}
