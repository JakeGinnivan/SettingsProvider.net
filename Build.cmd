@ECHO OFF

tools\GitHubFlowVersion\GitHubFlowVersion.exe /p SettingsProvider.proj /UpdateAssemblyInfo

IF NOT ERRORLEVEL 0 EXIT /B %ERRORLEVEL%

pause