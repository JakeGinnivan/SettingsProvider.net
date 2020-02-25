using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Xunit;

namespace SettingsProviderNet.Tests
{
  public class SettingsProviderTests
  {
    readonly SettingsProvider settingsRetreiver;
    readonly SettingsProvider settingsSaver;
    readonly ISettingsStorage store;

    public SettingsProviderTests()
    {
      //store = new TestStorage();
      store = new RoamingAppDataStorage("SettingsProviderNet");
      settingsRetreiver = new SettingsProvider(store);
      settingsSaver = new SettingsProvider(store);
    }

    [Fact]
    public void settings_provider_can_save_and_persist_int()
    {
      // arrange
      settingsSaver.SaveSettings(new TestSettings { TestProp2 = 123 });

      // act
      var settings = settingsRetreiver.GetSettings<TestSettings>();

      // assert
      Assert.Equal(123, settings.TestProp2);
    }

    [Fact]
    public void settings_provider_can_save_and_persist_bool()
    {
      // arrange
      settingsSaver.SaveSettings(new TestSettings { Boolean = true });

      // act
      var settings = settingsRetreiver.GetSettings<TestSettings>();

      // assert
      Assert.True(settings.Boolean);
    }

    [Fact]
    public void settings_provider_can_save_and_retreive_string()
    {
      // arrange
      settingsSaver.SaveSettings(new TestSettings { TestProp1 = "bar" });

      // act
      var settings = settingsRetreiver.GetSettings<TestSettings>();

      // assert
      Assert.Equal("bar", settings.TestProp1);
    }

    [Fact]
    public void settings_provider_can_save_and_retreive_japanese_string()
    {
      // arrange
      settingsSaver.SaveSettings(new TestSettings { TestProp1 = "居酒屋" });

      // act
      var settings = settingsRetreiver.GetSettings<TestSettings>();

      // assert
      Assert.Equal("居酒屋", settings.TestProp1);
    }

    [Fact]
    public void settings_provider_can_save_and_retreive_nullable_datetime()
    {
      // arrange
      var firstRun = DateTime.Now;
      settingsSaver.SaveSettings(new TestSettings { FirstRun = firstRun });

      // act
      var settings = settingsRetreiver.GetSettings<TestSettings>();

      // assert
      Assert.True(settings.FirstRun.HasValue);
      Debug.Assert(settings.FirstRun != null, "settings.FirstRun != null"); //R# annotations broken
      Assert.Equal(firstRun.ToString(CultureInfo.InvariantCulture), settings.FirstRun.Value.ToString(CultureInfo.InvariantCulture));
    }

    [Fact]
    public void settings_provider_can_save_and_retreive_null_nullable_datetime()
    {
      // arrange
      settingsSaver.SaveSettings(new TestSettings { FirstRun = null });

      // act
      var settings = settingsRetreiver.GetSettings<TestSettings>();

      // assert
      Assert.Null(settings.FirstRun);
    }

    [Fact]
    public void settings_provider_can_save_and_retreive_list()
    {
      // arrange
      settingsSaver.SaveSettings(new TestSettings { List2 = new List<int> { 123 } });

      // act
      var settings = settingsRetreiver.GetSettings<TestSettings>();

      // assert
      Assert.Equal(123, settings.List2.Single());
    }

    [Fact]
    public void settings_provider_can_save_and_retreive_list_with_japanese_characters()
    {
      // arrange
      settingsSaver.SaveSettings(new TestSettings { List = new List<string> { "居酒屋" } });

      // act
      var settings = settingsRetreiver.GetSettings<TestSettings>();

      // assert
      Assert.Equal("居酒屋", settings.List.Single());
    }

    [Fact]
    public void settings_provider_loads_default_values()
    {
      // arrange

      // act
      var settings = settingsRetreiver.GetSettings<TestSettings>();

      // assert
      Assert.Equal("foo", settings.TestProp1);
    }

    [Fact]
    public void settings_provider_ignores_properties_with_no_setters()
    {
      // arrange

      // act
      var settings = settingsRetreiver.GetSettings<TestSettings>();

      // assert
      Assert.NotNull(settings.NoSetter);
    }

    [Fact]
    public void settings_provider_can_save_and_retreive_enum()
    {
      // arrange
      settingsSaver.SaveSettings(new TestSettings { SomeEnum = MyEnum.Value2 });

      // act
      var settings = settingsRetreiver.GetSettings<TestSettings>();

      // assert
      Assert.Equal(MyEnum.Value2, settings.SomeEnum);
    }

    [Fact]
    public void settings_provider_can_reset_to_defaults()
    {
      // arrange
      settingsSaver.SaveSettings(new TestSettings { TestProp1 = "bar" });

      // act
      settingsRetreiver.ResetToDefaults<TestSettings>();
      var settings = settingsRetreiver.GetSettings<TestSettings>();

      // assert
      Assert.Equal("foo", settings.TestProp1);
    }

    [Fact]
    public void settings_provider_recycles_same_instance_on_reset()
    {
      // arrange
      var instance = settingsRetreiver.GetSettings<TestSettings>();
      settingsSaver.SaveSettings(new TestSettings { TestProp1 = "bar" });

      // act
      var settings = settingsRetreiver.ResetToDefaults<TestSettings>();
      var settings2 = settingsRetreiver.GetSettings<TestSettings>();

      // assert
      Assert.Same(instance, settings);
      Assert.Same(instance, settings2);
    }

    [Fact]
    public void settings_provider_returns_fresh_instance_when_requested()
    {
      // arrange
      var instance = settingsRetreiver.GetSettings<TestSettings>();

      // act
      var instance2 = settingsRetreiver.GetSettings<TestSettings>(true);

      // assert
      Assert.NotSame(instance, instance2);
    }

    [Fact]
    public void settings_provider_defaults_to_empty_ilist()
    {
      // arrange
      var settings = settingsRetreiver.GetSettings<TestSettings>();

      // act

      // assert
      Assert.NotNull(settings.IdList);
      Assert.IsType<List<Guid>>(settings.IdList);
      Assert.Empty(settings.IdList);
    }

    [Fact]
    public void settings_provider_defaults_to_empty_list()
    {
      // arrange
      var settings = settingsRetreiver.GetSettings<TestSettings>();

      // act

      // assert
      Assert.NotNull(settings.List2);
      Assert.IsType<List<int>>(settings.List2);
      Assert.Empty(settings.List2);
    }

    [Fact]
    public void CanSpecifyKey()
    {
      store.Save("TestSettings", new Dictionary<string, string>
            {
                {"OriginalName", "\"Value\""}
            });

      var settings = settingsRetreiver.GetSettings<TestSettings>();

      Assert.Equal("Value", settings.RenamedProperty);
    }

    [Fact]
    public void CanLoadLegacySettings()
    {
      store.Save("TestSettings", new Dictionary<string, string>
            {
                {"SettingsProviderNet.Tests.TestSettings.TestProp1", "Value"},
                {"SettingsProviderNet.Tests.TestSettings.SomeEnum", "Value2"},
                {"SettingsProviderNet.Tests.TestSettings.TestProp2", "2"},
                {"SettingsProviderNet.Tests.TestSettings.Boolean", "False"}
            });

      var settings = settingsRetreiver.GetSettings<TestSettings>();

      Assert.Equal("Value", settings.TestProp1);
      Assert.Equal(MyEnum.Value2, settings.SomeEnum);
      Assert.Equal(2, settings.TestProp2);
      Assert.False(settings.Boolean);
    }

    [Fact]
    public void SettingsAreNotFullyQualified()
    {
      settingsRetreiver.SaveSettings(new TestSettings
      {
        TestProp1 = "Value"
      });

      Assert.True(store.Load("TestSettings").ContainsKey("TestProp1"));
    }

    [Fact]
    public void CanSerialiseComplexTypes()
    {
      // arrange
      settingsSaver.SaveSettings(new TestSettings
      {
        Complex = new ComplexType
        {
          SomeProp = "Value"
        }
      });

      // act
      var settings = settingsRetreiver.GetSettings<TestSettings>();

      // assert
      Assert.Equal("Value", settings.Complex.SomeProp);
    }

    [Fact]
    public void settings_provider_can_save_and_retreive_protected_string()
    {
      // arrange
      settingsSaver.SaveSettings(new TestSettings { ProtectedString = "crypto" });

      // act
      var settings = settingsRetreiver.GetSettings<TestSettings>();

      // assert
      Assert.Equal("crypto", settings.ProtectedString);
    }

    [Fact]
    public void settings_provider_retreive_protected_defaultvalue_string()
    {
      // act
      var settings = settingsRetreiver.GetSettings<TestSettings>();

      // assert
      Assert.Equal("test", settings.ProtectedStringWithDefault);
    }
  }
}