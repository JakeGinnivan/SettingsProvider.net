v2.0 Changes
 - BREAKING: Changed to a class library
   - DELETE The files `Settings\SettingsProvider.cs` and `Settings\IsolatedStorageSettingsStore.cs`
   - Add `using SettingsProviderNet;` to classes which use settings provider
 - Added: RoamingAppDataStorage (to easily save settings to app data)
 - Added: [Key("Name")] attribute to control the key that property will be saved to (useful when renaming)
 - Improved: Serialisation of complex types added, this simply uses the `DataContractJsonSerializer` but improves flexibility
 - Change: Keys are now not fully qualified (fully qualified keys will still be read, so this is a NON breaking change)


It is recommended to use the new RoamingAppDataStorage. To do so simply create your settings provider like this:

new SettingsProvider(new RoamingAppDataStorage("MyAppName"));

Your settings files will now be saved to `c:\ProgramData\MyAppName\ClassName.settings` making it much easier to find