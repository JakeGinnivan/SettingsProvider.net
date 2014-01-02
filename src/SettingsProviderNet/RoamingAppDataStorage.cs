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
            File.WriteAllText(GetSettingsFilename(filename), fileContents);
        }

        protected override string ReadTextFile(string filename)
        {
            return File.ReadAllText(GetSettingsFilename(filename));
        }

        private string GetSettingsFilename(string filename)
        {
            var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var settingsFilename = Path.Combine(folderPath, folderName, filename);
            return settingsFilename;
        }
    }
}