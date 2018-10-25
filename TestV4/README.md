# Test V4

## App.config and appSettings.Debug.config (or 'appSettings.<configuration>.config')

The App.config includes appSettings.config as a file= source.
There is a post build event in Test.csproj which will copy
the appSettings.<configuration>.config to the target directory (the bin-folder)
which is where the App.config will end up.

If you are developing, you should use the Debug configuration.
So to set up your test endpoint, you should copy appSettings.Debug.config.template
to appSettings.Debug.config and change the **Host** value to your liking.
