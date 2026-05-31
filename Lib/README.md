# Lib/ — build dependencies (not committed)

`PrintingPodRecharge` references **`ONITwitchLib.dll`** here at compile time (it's the public API of
the *Twitch Integration* mod, used by PrintingPodRecharge's optional Twitch features). The DLL is a
third-party binary, so it is **gitignored** rather than committed.

To build, drop `ONITwitchLib.dll` into this folder:

1. Download `ONITwitchLib.zip` from the Twitch Integration release:
   https://github.com/asquared31415/ONITwitch/releases/download/v1.2.9/ONITwitchLib.zip
2. Extract `ONITwitchLib.dll` into `Lib/`.

It is referenced (not bundled) in `PrintingPodRecharge.csproj`; the Twitch code paths are guarded at
runtime, so the mod still loads for users who don't have the Twitch Integration mod installed.
