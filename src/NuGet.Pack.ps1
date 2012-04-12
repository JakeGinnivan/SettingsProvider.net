$nuget = ls .\packages\NuGet.CommandLine*\tools\NuGet.exe

$buildRoot = ".\NuGetBuild"
$settingsProviderDestination = "$buildRoot\content\Settings"
rm $buildRoot -force -recurse -ErrorAction SilentlyContinue
mkdir $settingsProviderDestination | out-null
mkdir "$buildRoot\Tools" | out-null
$spFile = Join-Path $settingsProviderDestination SettingsProvider.cs.pp
cp .\SettingsProviderNet\SettingsProvider.cs $spFile
(Get-Content $spFile) | 
	Foreach-Object { $_ -replace 'namespace SettingsProviderNet', 'namespace $rootnamespace$.Settings' } | 
	Set-Content $spFile


$storageFile = Join-Path $settingsProviderDestination IsolatedStorageSettingsStore.cs.pp
cp .\SettingsProviderNet\IsolatedStorageSettingsStore.cs $storageFile
(Get-Content $storageFile) | 
	Foreach-Object { $_ -replace 'namespace SettingsProviderNet', 'namespace $rootnamespace$.Settings' } | 
	Set-Content $storageFile

$nuspecFile = "SettingsProviderNet.nuspec"
cp .\SettingsProviderNet\$nuspecFile "$buildRoot\$nuspecFile"
cp .\SettingsProviderNet\install.ps1 "$buildRoot\Tools\install.ps1"
pushd $buildRoot

    & $nuget pack $nuspecFile -OutputDirectory $buildRoot\..\..\..

popd


$buildRoot = ".\NuGetBuildWp7"
$settingsProviderDestination = "$buildRoot\content\Settings"
rm $buildRoot -force -recurse -ErrorAction SilentlyContinue
mkdir $settingsProviderDestination | out-null
mkdir "$buildRoot\Tools" | out-null
cp .\SettingsProviderNet\SettingsProvider.cs (Join-Path $settingsProviderDestination "SettingsProvider.cs.pp")
$spFile = Join-Path $settingsProviderDestination SettingsProvider.cs.pp
(Get-Content $spFile) | 
	Foreach-Object { $_ -replace 'namespace SettingsProviderNet', 'namespace $rootnamespace$.Settings' } | 
	Set-Content $spFile

$storageFile = Join-Path $settingsProviderDestination IsolatedStorageSettingsStore.cs.pp
cp .\SettingsProviderWP7\IsolatedStorageSettingsStore.cs $storageFile
(Get-Content $storageFile) | 
	Foreach-Object { $_ -replace 'namespace SettingsProviderNet', 'namespace $rootnamespace$.Settings' } | 
	Set-Content $storageFile

$dnFile = Join-Path $settingsProviderDestination DisplayNameAttribute.cs.pp
cp .\SettingsProviderWP7\DisplayNameAttribute.cs $dnFile
(Get-Content $dnFile) | 
	Foreach-Object { $_ -replace 'namespace SettingsProviderNet', 'namespace $rootnamespace$.Settings' } | 
	Set-Content $dnFile

$nuspecFile = "SettingsProviderWp7.nuspec"
cp .\SettingsProviderWP7\$nuspecFile "$buildRoot\$nuspecFile"
cp .\SettingsProviderNet\install.ps1 "$buildRoot\Tools\install.ps1"
pushd $buildRoot

    & $nuget pack $nuspecFile -OutputDirectory $buildRoot\..\..\..

popd