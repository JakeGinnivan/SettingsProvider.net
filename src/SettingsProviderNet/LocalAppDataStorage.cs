using System;
using System.IO;

namespace SettingsProviderNet
{
    public class LocalAppDataStorage : JsonSettingsStoreBase
    {
        private readonly string folderName;

        public LocalAppDataStorage(string folderName)
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
            var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var settingsFilename = Path.Combine(folderPath, folderName);
            return settingsFilename;
        }
    }
}