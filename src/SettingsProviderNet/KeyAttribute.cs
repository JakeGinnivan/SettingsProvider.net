using System;

namespace SettingsProviderNet
{
    public class KeyAttribute : Attribute
    {
        public KeyAttribute(string key)
        {
            Key = key;
        }

        public string Key { get; private set; }
    }
}