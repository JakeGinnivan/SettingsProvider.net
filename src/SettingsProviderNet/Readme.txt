v2.0 Changes
 - BREAKING: Changed to a class library
   - DELETE The files `Settings\SettingsProvider.cs` and `Settings\IsolatedStorageSettingsStore.cs`
   - Add `using SettingsProviderNet;` to classes which use settings provider