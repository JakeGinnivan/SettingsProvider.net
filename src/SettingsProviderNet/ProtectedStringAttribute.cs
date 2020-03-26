using System;

namespace SimpleSettingsStorage
{
  [AttributeUsage(AttributeTargets.Property)]
  public class ProtectedStringAttribute : Attribute
  {
  }
}