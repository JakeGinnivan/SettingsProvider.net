using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace SettingsProviderNet
{
    public class IsolatedStorageSettingsStore : ISettingsStorage
    {
        readonly IsolatedStorageSettings storage;

        public IsolatedStorageSettingsStore()
        {
            storage = IsolatedStorageSettings.ApplicationSettings;
        }

        public string SerializeList(List<string> listOfItems)
        {
            var stringBuilder = new StringBuilder();
            var xmlWriter = XmlWriter.Create(stringBuilder);
            new DataContractSerializer(typeof(List<string>)).WriteObject(xmlWriter, listOfItems);
            xmlWriter.Flush();
            return stringBuilder.ToString();
        }

        public List<string> DeserializeList(string serializedList)
        {
            return (List<string>)new DataContractSerializer(typeof(List<string>))
                .ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(serializedList)));
        }

        public void Save(string key, Dictionary<string, string> settings)
        {
            var stringBuilder = new StringBuilder();
            var xmlWriter = XmlWriter.Create(stringBuilder);
            new DataContractSerializer(typeof(Dictionary<string, string>)).WriteObject(xmlWriter, settings);
            xmlWriter.Flush();

            if (storage.Contains(key))
                storage[key] = stringBuilder.ToString();
            else
                storage.Add(key, stringBuilder.ToString());
        }

        public Dictionary<string, string> Load(string key)
        {
            if (storage.Contains(key))
            {
                var serializer = new DataContractSerializer(typeof(Dictionary<string, string>));
                var xmlReader = XmlReader.Create(new StringReader((string)storage[key]));
                return (Dictionary<string, string>)serializer.ReadObject(xmlReader);
            }

            return new Dictionary<string, string>();
        }
    }
}