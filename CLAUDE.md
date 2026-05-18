# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this project does

JwlMediaWin is a Windows system tray utility that fixes the JW Library media window so it stays visible on a secondary display when other apps take focus. It uses Windows UI Automation — it never modifies JW Library's code, only automates window management actions the user could do manually.

## Building

Open `JwlMediaWin.sln` in Visual Studio (2019 or later). The solution targets .NET Framework 4.8. There are no tests.

From the command line:
```
msbuild JwlMediaWin.sln /p:Configuration=Release
```

NuGet packages are managed via `packages.config` (not SDK-style PackageReference). Run **Restore NuGet Packages** in Visual Studio or `nuget restore` before building if packages are missing.

## Project structure

Two projects:

- **`JwlMediaWin.Core`** — class library; all UI Automation logic lives here
  - `Fixer.cs` — finds the JWL media window via `AutomationElement` traversal and "fixes" it by sending `Win+Shift+Return` (converts UWP window to a movable window) then calling `SetWindowPos` to reposition it and `EnableWindow(false)` to suppress the title bar controls
  - `FixerRunner.cs` — runs `Fixer.Execute()` in a background loop; adjusts poll interval based on whether JWL is running

- **`JwlMediaWin`** — WPF tray icon app
  - `NotifyIconViewModel.cs` — the single ViewModel; drives `FixerRunner` and surfaces the three options (JW Library / JW Library Sign Language / keep-on-top)
  - `App.xaml.cs` — startup: enforces single instance via `Mutex`, wires up Serilog logger and tray icon
  - `Services/OptionsService.cs` — reads/writes `options.json` via Newtonsoft.Json

## Key architecture notes

**The "fix" is one-shot per JWL launch.** `Fixer` caches `_cachedWindowElements` after it finds and fixes the media window. If the cached window becomes unavailable (`ElementNotAvailableException`), both caches are cleared and the next poll starts fresh.

**Window detection logic.** The media window is identified as a top-most desktop child whose `Name` contains "JW Library". The inner `Windows.UI.Core.CoreWindow` is verified by looking for a `WebView2` or `ProgressRing` control (for standard JWL) or an `Image` control (for Sign Language).

**Two supported app types.** `JwLibAppTypes.JwLibrary` (process `JWLibrary`) and `JwLibAppTypes.JwLibrarySignLanguage` (process `JWLibrary.Forms.UWP`). Only one can be active at a time; enabling one disables the other in `NotifyIconViewModel`.

**User data paths:**
- Options: `%APPDATA%\JwlMediaWin\1\options.json`
- Logs: `Documents\JwlMediaWin\Logs\log-{Date}.txt` (28-day rolling, Serilog)

## Code style

StyleCop.Analyzers is enforced (rules in `JwlMediaWin.ruleset`). Suppressed rules: SA1101, SA1309, SA1633, SA1652. `SolutionInfo.cs` at solution root is linked into both projects for shared assembly attributes.
