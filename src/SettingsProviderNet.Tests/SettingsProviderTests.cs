using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public SettingsProviderTests()
        {
            var store = new TestStorage();
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
            settingsSaver.SaveSettings(new TestSettings { List2 = new List<int>{123} });

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
            Assert.NotNull(settings.Complex);
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

    public class TestStorage : JsonSettingsStoreBase
    {
        private readonly Dictionary<string, string> files = new Dictionary<string, string>();

        public Dictionary<string, string> Files
        {
            get { return files; }
        }

        protected override void WriteTextFile(string filename, string fileContents)
        {
            if (!Files.ContainsKey(filename))
                Files.Add(filename, fileContents);
            else
                Files[filename] = fileContents;
        }

        protected override string ReadTextFile(string filename)
        {
            return Files.ContainsKey(filename) ? Files[filename] : null;
        }
    }

    public enum MyEnum
    {
        Value1,
        Value2
    }
}