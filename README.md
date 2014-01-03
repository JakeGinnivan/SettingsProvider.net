# SettingsProvider.NET
The aim of settings provider is to quickly give you a simple way to store application settings. It only supports simple types, but is very flexible and makes it easy to store the settings values in plain text in a database (OOTB serialises to JSON in isolated storage)  

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
    }

### Reading Settings

    var settingsProvider = new SettingsProvider(); //By default uses IsolatedStorage for storage
    var mySettings = settingsProvider.Load<MySettings>();
    Assert.True(mySettings.RememberMe); 

### Saving Settings

    var settingsProvider = new SettingsProvider(); //By default uses IsolatedStorage for storage
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

## Limitations

To improve upgradability and make SettingsProvider.net resilient to changes, we serialise everything to a string, this means we support the following types:

Types supported by Convert.ChangeType plus a few others - see http://msdn.microsoft.com/en-us/library/dtb69x08.aspx  

 - Guid
 - SByte   
 - Int16  
 - Int32  
 - Int64  
 - Byte  
 - UInt16  
 - UInt32  
 - UInt64  
 - Single  
 - Double  
 - Decimal  
 - Boolean  
 - Char  
 - String  
  
 - DateTime  
 - Enums  
 - Nullable<T> where T is any of the types above
 - List<T> where T is any of the types above
