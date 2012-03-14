using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using NSubstitute;
using Xunit;

namespace SettingsProviderNet.Tests
{
    public class SettingsProviderTests
    {
        readonly Dictionary<string, string> backingStore;
        readonly ISettingsStorage applicationSettingsStore;
        readonly SettingsProvider settingsRetreiver;
        readonly SettingsProvider settingsSaver;

        public SettingsProviderTests()
        {
            applicationSettingsStore = Substitute.For<ISettingsStorage>();
            backingStore = new Dictionary<string, string>();
            applicationSettingsStore.Load("TestSettings.settings").Returns(backingStore);
            applicationSettingsStore
                .When(s=>s.Save("TestSettings.settings", Arg.Any<Dictionary<string, string>>()))
                .Do(c=>
                    {
                        backingStore.Clear();
                        foreach (var setting in c.Arg<Dictionary<string, string>>())
                        {
                            backingStore.Add(setting.Key, setting.Value);
                        }
                    });

            settingsRetreiver = new SettingsProvider(applicationSettingsStore);
            settingsSaver = new SettingsProvider(applicationSettingsStore);
        }

        [Fact]
        public void settings_provider_can_persist_int()
        {
            // arrange
            var settings = new TestSettings { TestProp2 = 123 };
            var settingDescriptor = new SettingsProvider.SettingDescriptor(typeof(TestSettings).GetProperty("TestProp2"));
            var key = SettingsProvider.GetKey<TestSettings>(settingDescriptor);

            // act
            settingsSaver.SaveSettings(settings);

            // assert
            Assert.Equal(settings.TestProp2, Convert.ToInt32(backingStore[key]));
        }

        [Fact]
        public void settings_provider_can_persist_list()
        {
            // arrange
            var settings = new TestSettings { List = new List<string>{"Testing", "Testing2"} };
            var settingDescriptor = new SettingsProvider.SettingDescriptor(typeof(TestSettings).GetProperty("List"));
            var key = SettingsProvider.GetKey<TestSettings>(settingDescriptor);

            // act
            settingsSaver.SaveSettings(settings);

            // assert
            Assert.Equal("[\"Testing\",\"Testing2\"]", backingStore[key]);
        }

        [Fact]
        public void settings_provider_can_retreive_list()
        {
            // arrange
            settingsSaver.SaveSettings(new TestSettings { List2 = new List<int>{123} });

            // act
            var settings = settingsRetreiver.GetSettings<TestSettings>();

            // assert
            Assert.Equal(123, settings.List2.Single());
        }

        [Fact]
        public void settings_provider_can_retreive_int()
        {
            // arrange
            settingsSaver.SaveSettings(new TestSettings { TestProp2 = 123 });

            // act
            var settings = settingsRetreiver.GetSettings<TestSettings>();

            // assert
            Assert.Equal(123, settings.TestProp2);
        }

        [Fact]
        public void settings_provider_can_persist_string()
        {
            // arrange
            var settings = new TestSettings { TestProp1 = "bar" };
            var settingDescriptor = new SettingsProvider.SettingDescriptor(typeof(TestSettings).GetProperty("TestProp1"));
            var key = SettingsProvider.GetKey<TestSettings>(settingDescriptor);

            // act
            settingsSaver.SaveSettings(settings);

            // assert
            Assert.Equal(settings.TestProp1, backingStore[key]);
        }

        [Fact]
        public void settings_provider_can_retreive_string()
        {
            // arrange
            settingsSaver.SaveSettings(new TestSettings { TestProp1 = "bar" });

            // act
            var settings = settingsRetreiver.GetSettings<TestSettings>();

            // assert
            Assert.Equal("bar", settings.TestProp1);
        }

        [Fact]
        public void settings_provider_can_persist_nullable_datetime()
        {
            // arrange
            var firstRun = DateTime.Now;
            var settings = new TestSettings { FirstRun = firstRun };
            var settingDescriptor = new SettingsProvider.SettingDescriptor(typeof(TestSettings).GetProperty("FirstRun"));
            var key = SettingsProvider.GetKey<TestSettings>(settingDescriptor);

            // act
            settingsSaver.SaveSettings(settings);

            // assert
            Assert.Equal(firstRun.ToString(CultureInfo.InvariantCulture), backingStore[key]);
        }

        [Fact]
        public void settings_provider_can_retreive_nullable_datetime()
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
        public void settings_provider_can_retreive_null_nullable_datetime()
        {
            // arrange
            settingsSaver.SaveSettings(new TestSettings { FirstRun = null });

            // act
            var settings = settingsRetreiver.GetSettings<TestSettings>();

            // assert
            Assert.Null(settings.FirstRun);
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
            Assert.NotNull(settings.Complex);
        }

        [Fact]
        public void settings_provider_can_persist_enum()
        {
            // arrange
            var settings = new TestSettings { SomeEnum = MyEnum.Value1 };
            var settingDescriptor = new SettingsProvider.SettingDescriptor(typeof(TestSettings).GetProperty("SomeEnum"));
            var key = SettingsProvider.GetKey<TestSettings>(settingDescriptor);

            // act
            settingsSaver.SaveSettings(settings);

            // assert
            Assert.Equal(Enum.GetName(typeof(MyEnum), settings.SomeEnum), backingStore[key]);
        }

        [Fact]
        public void settings_provider_can_retreive_enum()
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

        public class TestSettings
        {
            [DefaultValue("foo")]
            public string TestProp1 { get; set; }

            public MyEnum SomeEnum { get; set; }

            public int TestProp2 { get; set; }

            public DateTime? FirstRun { get; set; }

            public ComplexType Complex { get { return new ComplexType(); } }

            public List<string> List { get; set; }

            public List<int> List2 { get; set; }

            public IList<Guid> IdList { get; set; }

            public class ComplexType
            {
            }
        }
    }

    public enum MyEnum
    {
        Value1,
        Value2
    }
}