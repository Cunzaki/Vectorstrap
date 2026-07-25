# Vectorstrap

**Vectorstrap** is a focused Roblox bootstrapper fork based on [Froststrap](https://github.com/Froststrap/Froststrap), maintained by **Cunzaki**.

It is built for **Vector** external users who need reliable **Roblox channel / version switching** and **in-game Roblox settings** management—without the extra Froststrap surface area (mods, FastFlags UI, account manager, region tools, Froststrap remote config, and similar).

## Focus

- Roblox **update channel** selection and deployment info
- **Production / version** controls (static install directory, update blocking, reinstall)
- **Roblox settings** (Global Basic Settings editor)
- Essential **launch behaviour** (multi-instance, process priority, crash handler, etc.)

## Requirements

- Windows 10 or newer
- [.NET SDK 10](https://dotnet.microsoft.com/download) (matching the project target)

## Build

```powershell
dotnet restore Vectorstrap.slnx
dotnet build Vectorstrap.slnx -c Release
```

The executable is emitted as `Bloxstrap\bin\Release\net10.0-windows\Vectorstrap.exe`.

## Development

Open `Vectorstrap.slnx` in Visual Studio or Rider. Submodule dependencies (`wpfui`, `ColorPicker`) are included in the solution.

## Credits

- Based on [Froststrap](https://github.com/Froststrap/Froststrap) / [Fishstrap](https://github.com/fishstrap/fishstrap) / [Bloxstrap](https://github.com/bloxstraplabs/bloxstrap)
- Created by **Cunzaki** for the Vector community

## Licensing

This repository inherits Froststrap’s multi-license model. See `LICENSE`, `LICENSE-MIT`, and `LICENSE-UNLICENSE` in the repo root.
