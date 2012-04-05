using System;

namespace SettingsProviderNet
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DisplayNameAttribute : Attribute
    {
        public DisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }

        public string DisplayName { get; set; }
    }
}