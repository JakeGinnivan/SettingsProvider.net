using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace SettingsProviderNet
{
    public abstract class JsonSettingsStoreBase : ISettingsStorage
    {
        public string SerializeList(List<string> listOfItems)
        {
            var ms = new MemoryStream();
            var writer = JsonReaderWriterFactory.CreateJsonWriter(ms, Encoding.Unicode);
            new DataContractJsonSerializer(typeof(List<string>)).WriteObject(ms, listOfItems);
            writer.Flush();
            var jsonString = Encoding.Default.GetString(ms.ToArray());

            return jsonString;
        }

        public List<string> DeserializeList(string serializedList)
        {
            return (List<string>)new DataContractJsonSerializer(typeof(List<string>))
                .ReadObject(new MemoryStream(Encoding.Default.GetBytes(serializedList)));
        }

        public void Save(string key, Dictionary<string, string> settings)
        {
            var filename = key + ".settings";

            var serializer = new DataContractJsonSerializer(typeof(Dictionary<string, string>));
            var ms = new MemoryStream();
            var writer = JsonReaderWriterFactory.CreateJsonWriter(ms, Encoding.Unicode);
            serializer.WriteObject(ms, settings);
            writer.Flush();
            var jsonString = Encoding.Default.GetString(ms.ToArray());
            WriteTextFile(filename, jsonString);
        }

        protected abstract void WriteTextFile(string filename, string fileContents);

        public Dictionary<string, string> Load(string key)
        {
            var filename = key + ".settings";

            var readTextFile = ReadTextFile(filename);
            if (!string.IsNullOrEmpty(readTextFile))
            {
                var serializer = new DataContractJsonSerializer(typeof(Dictionary<string, string>));
                return (Dictionary<string, string>)serializer.ReadObject(new MemoryStream(Encoding.Default.GetBytes(readTextFile)));
            }

            return new Dictionary<string, string>();
        }

        protected abstract string ReadTextFile(string filename);
    }
}