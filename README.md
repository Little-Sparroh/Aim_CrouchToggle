# AimCrouchToggles

A BepInEx mod for Mycopunk that adds optional toggle aim and toggle crouch.

## Features

- **Toggle Aim**: Press aim once to enter ADS, press again to exit — no need to hold.
- **Toggle Crouch**: Press the slide/crouch button to stay crouched until you press it again. Sliding still works as normal when you have momentum.

Both features are off by default and can be enabled independently in the config.

## Getting Started

### Dependencies

* Mycopunk (base game)
* [BepInEx](https://github.com/BepInEx/BepInEx) - Version 5.4.2403 or compatible
* .NET Framework 4.8
* [HarmonyLib](https://github.com/pardeike/Harmony) (included via NuGet)

### Building/Compiling

1. Clone this repository
2. Open the solution file in Visual Studio, Rider, or your preferred C# IDE
3. Build the project in Release mode to generate the .dll file

Alternatively, use dotnet CLI:
```bash
dotnet build --configuration Release
```

### Installing

**Via Thunderstore (Recommended)**:
1. Download and install via Thunderstore Mod Manager
2. The mod will be automatically installed to the correct directory

**Manual Installation**:
1. Place the built `AimCrouchToggles.dll` in your `<Mycopunk Directory>/BepInEx/plugins/` folder

### Executing program

The mod loads automatically through BepInEx when the game starts. Check the BepInEx console for loading confirmation messages.

## Configuration

Access mod settings through the BepInEx configuration file at `<Mycopunk Directory>/BepInEx/config/sparroh.aimcrouchtoggles.cfg`:

| Setting | Default | Description |
|---------|---------|-------------|
| Toggle Aim | `false` | Aim becomes press-to-toggle instead of hold |
| Toggle Crouch | `false` | Crouch stays engaged until toggled off (via slide button) |

Config changes are hot-reloaded while the game is running — save the `.cfg` file and the mod applies the new values automatically (check the BepInEx log for `Config reloaded from disk.`).

## Help

* **Mod not loading?** Verify BepInEx is installed correctly and check console logs for errors
* **Toggle not working?** Confirm the setting is enabled in the config file; save the file to hot-reload without restarting
* **Aim stuck on?** Toggle aim resets on death/resurrect; you can also disable the setting to force-clear it

## Authors

- Sparroh

## License

This project is licensed under the MIT License - see the LICENSE file for details
