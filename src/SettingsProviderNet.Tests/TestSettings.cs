using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SettingsProviderNet.Tests
{
  public class TestSettings
  {
    [DefaultValue("foo")]
    public string TestProp1 { get; set; }

    public MyEnum SomeEnum { get; set; }

    public int TestProp2 { get; set; }

    public DateTime? FirstRun { get; set; }

    public ComplexType NoSetter { get { return new ComplexType(); } }

    public List<string> List { get; set; }

    public List<int> List2 { get; set; }

    public IList<Guid> IdList { get; set; }

    [Key("OriginalName")]
    public string RenamedProperty { get; set; }

    public ComplexType Complex { get; set; }

    public bool Boolean { get; set; }

    [ProtectedString]
    public string ProtectedString { get; set; }

    [DefaultValue("test")]
    [ProtectedString]
    public string ProtectedStringWithDefault { get; set; }
  }
}