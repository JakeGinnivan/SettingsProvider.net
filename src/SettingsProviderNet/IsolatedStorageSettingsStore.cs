using System.IO;
using System.IO.IsolatedStorage;

namespace SettingsProviderNet
{
    public class IsolatedStorageSettingsStore : JsonSettingsStoreBase
    {
        const IsolatedStorageScope Scope = IsolatedStorageScope.User | IsolatedStorageScope.Roaming;

        protected override void WriteTextFile(string filename, string fileContents)
        {
            using (var isoStore = IsolatedStorageFile.GetStore(Scope, null, null))
            {
                using (var stream = new IsolatedStorageFileStream(filename, FileMode.Create, isoStore))
                    new StreamWriter(stream).Write(fileContents);
            }
        }

        protected override string ReadTextFile(string filename)
        {
            using (var isoStore = IsolatedStorageFile.GetStore(Scope, null, null))
            {
                if (isoStore.FileExists(filename))
                {
                    using (var stream = new IsolatedStorageFileStream(filename, FileMode.Open, isoStore))
                        return new StreamReader(stream).ReadToEnd();
                }
            }

            return null;
        }
    }
}