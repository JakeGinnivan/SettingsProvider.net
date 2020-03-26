using System;

namespace SimpleSettingsStorage
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