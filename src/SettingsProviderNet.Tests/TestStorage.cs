using System.Collections.Generic;

namespace SettingsProviderNet.Tests
{
    public class TestStorage : JsonSettingsStoreBase
    {
        private readonly Dictionary<string, string> files = new Dictionary<string, string>();

        public Dictionary<string, string> Files
        {
            get { return files; }
        }

        protected override void WriteTextFile(string filename, string fileContents)
        {
            if (!Files.ContainsKey(filename))
                Files.Add(filename, fileContents);
            else
                Files[filename] = fileContents;
        }

        protected override string ReadTextFile(string filename)
        {
            return Files.ContainsKey(filename) ? Files[filename] : null;
        }
    }
}