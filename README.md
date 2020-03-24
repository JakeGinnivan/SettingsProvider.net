# SimpleSettingsStorage
The aim of settings provider is to quickly give you a simple way to store application settings and read metadata about those settings, like description, name, default values etc

v2 Has added a few things to make versioning easier, the Key attribute allows you to rename properties, and types are no longer fully qualified (so you can move classes)

## Usage

Start of by creating your settings class, marking up with metadata

    public class MySettings
    {
        [DefaultValue("Jake")]
        [DisplayName("Your Name")]
        public string Name { get; set; }

        [DefaultValue(true)]
        [Description("Should Some App Remember your name?")]
        public bool RememberMe { get;set; }

        public List<Guid> Favourites { get;set; }

        [Key("OriginalName")]
        public string Renamed { get; set; }

        [ProtectedString]
        public string Encrypted { get; set; }
    }

### Reading Settings

      var config = new StorageConfigBuilder()
        .FileName("Test.json")
        .SetAppName("TestApp")
        .SetFolder(Environment.SpecialFolder.LocalApplicationData)
        .CreateIfNotExist(true)
        .Build();

      var storage = new Storages.JsonSettingsStorage();
      storage.Configure(config);

      var provider = new SettingsProvider(storage);
      var mySettings = provider.GetSettings<TestSettings>();

### Saving Settings

    var config = new StorageConfigBuilder()
        .FileName("Test.json")
        .SetAppName("TestApp")
        .SetFolder(Environment.SpecialFolder.LocalApplicationData)
        .CreateIfNotExist(true)
        .Build();

    var storage = new Storages.JsonSettingsStorage();
    storage.Configure(config);

    var settingsProvider = new SettingsProvider(storage);
    var mySettings = new MySettings { Name = "Mr Ginnivan" };
    settingsProvider.Save(mySettings);

### Settings MetaData
This is handy if you want to generate a UI for your settings

	var settingsProvider = new SettingsProvider();
	foreach (var setting in settingsProvider.ReadSettingMetadata<MySettings>())
	{
	    Console.WriteLine("{0} ({1}) - {2}", setting.DisplayName, setting.Description, setting.DefaultValue);
	}
	// Prints:
	//
	// Your Name () - Jake
	// RememberMe (Should Some App Remember your name?) - true

### Additional Notes
 - Nested complex types do not have the same features and simply use the `DataContractJsonSerializer` to serialise
