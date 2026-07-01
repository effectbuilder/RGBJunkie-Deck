# RGBJunkie-Deck

Stream Deck plugin for [RGBJunkie](https://www.rgbjunkie.com) — control effects, scenes, and app views from your Elgato Stream Deck.

Inspired by [SignalRGB-Deck](https://github.com/bill-z-bawb/SignalRGB-Deck); uses RGBJunkie’s `rgbjunkie://` deep links instead of a custom API.

## Requirements

- Windows 10+
- [Elgato Stream Deck](https://www.elgato.com/stream-deck) software 6.4+
- [RGBJunkie](https://www.rgbjunkie.com) installed (registers the `rgbjunkie://` URL scheme)
- [.NET SDK](https://dotnet.microsoft.com/download) 10+
- **Optional:** [Visual Studio Community](https://visualstudio.microsoft.com/) with **.NET desktop development** — easier debugging, not required to build

## Get the source

```powershell
git clone --recurse-submodules https://github.com/rgbjunkie/RGBJunkie-Deck.git
cd RGBJunkie-Deck
```

If you already cloned without submodules:

```powershell
git submodule update --init --recursive
```

The [BarRaider Stream Deck SDK](https://github.com/BarRaider/barraider-sdtools) lives in `barraider-sdtools/` as a git submodule.

## Build

```powershell
cd RGBJunkieDeckPlugin
dotnet build -c Release
```

Output folder:

```
RGBJunkieDeckPlugin\bin\Release\com.rgbjunkie.deck.sdPlugin\
```

## Install

### Automatic (recommended)

From the repo root, double-click **`install-deck-plugin.bat`** or run:

```powershell
.\install-deck-plugin.ps1
```

This builds Release, stops Stream Deck, copies the plugin to `%APPDATA%\Elgato\StreamDeck\Plugins\`, and restarts Stream Deck.

Options:

| Flag | Meaning |
|------|---------|
| `-SkipBuild` | Install the last build without compiling |
| `-NoRestart` | Install only; leave Stream Deck closed |
| `-StreamDeckExe "C:\...\StreamDeck.exe"` | Override Stream Deck location |

Set `$env:RGBJUNKIE_STREAMDECK_EXE` if Stream Deck is not under Program Files.

### Manual

1. Quit the Stream Deck app (tray icon → Quit).
2. Copy the entire `com.rgbjunkie.deck.sdPlugin` folder to:

   ```
   %APPDATA%\Elgato\StreamDeck\Plugins\
   ```

3. Start Stream Deck again. Look for **RGBJunkie** in the action list.

Or double-click the `.sdPlugin` folder if Windows associates it with Stream Deck.

## Actions

| Action | What it does |
|--------|----------------|
| **RGBJunkie Effect** | Pick an effect from the dropdown |
| **Previous / Next Effect** | Cycle your effect list |
| **RGBJunkie Scene** | Pick a saved scene |
| **Previous / Next Scene** | Cycle saved scenes |
| **Effect Browser** | Open the in-app effect browser |
| **Hardware / Installed / Logs** | Open Settings tabs |
| **Plugins Folder** | Open `%APPDATA%\RGBJunkie\plugins` |
| **Restart** | Restart RGBJunkie |
| **Toggle Lights** | Pause the effect and turn all device LEDs off; press again to restore |
| **Brightness Up / Down** | Step master brightness ±5% |
| **Master Brightness (Dial)** | Stream Deck + dial: rotate to dim/brighten; press for 100% |
| **Cycle Effect / Scene (Dial)** | Rotate to step through effects or scenes |
| **Pause + Brightness (Dial)** | Press to pause lights; rotate for brightness |

All actions use `silent=1` so RGBJunkie stays in the tray. See [APP_DEEP_LINKS.md](https://github.com/rgbjunkie/RGBJunkieApp/blob/main/docs/APP_DEEP_LINKS.md) in the main app repo.

## Debugging

1. In `Program.cs`, uncomment the debugger wait loop.
2. Build Debug, install/copy the Debug `.sdPlugin` output.
3. Attach Visual Studio to the `com.rgbjunkie.deck` process after adding an action to the deck.

## Project layout

- `RGBJunkieDeckPlugin/` — plugin source (manifest v1.2.2)
- `barraider-sdtools/` — [BarRaider Stream Deck SDK](https://github.com/BarRaider/barraider-sdtools) (git submodule)
- `scripts/` — icon generator (`npm install` then `node generate-icons.mjs`)

## Stream Deck + administrator elevation

RGBJunkie runs as administrator for USB access. Elgato Stream Deck is not elevated, so opening `rgbjunkie://` links directly can show a **run as administrator** prompt on every button press.

**RGBJunkie-Deck v1.0.2+** avoids that when RGBJunkie is already running: it drops links into `%APPDATA%\RGBJunkie\ipc\deeplink\` instead of relaunching the exe. If RGBJunkie is closed, it still uses `rgbjunkie://` (you may get one UAC prompt to start the app).

Rebuild and reinstall the plugin after updating RGBJunkie.

## Icons

Branded icons are generated from the official RGBJunkie mark (`public/app-icon.svg` in the main app repo):

```powershell
cd scripts
npm install
node generate-icons.mjs
```

This writes 72×72 and 144×144 PNGs under `RGBJunkieDeckPlugin/Images/` (plugin icon, category icon, and per-action key art). Rebuild the plugin after regenerating icons.

Palette: dark `#1c1c22` background, accent `#00dc82`.

## License

Plugin source: MIT (see LICENSE). RGBJunkie itself is proprietary.
