# Vectorstrap — agent notes

## Project goal

Maintain a **minimal Roblox bootstrapper fork** (Vectorstrap) for Vector external users:

1. **Channel / version switching** — correct Roblox client channel and deployment for Vector compatibility.
2. **Roblox settings** — GBS / client settings editing where supported.
3. **Core bootstrapper behaviour** — install, launch player, updates, essential launch options.

## Out of scope (do not re-expand without explicit request)

- Froststrap remote `Data.json` / community mods API
- Account manager, Studio RPC, region selector, mods, FastFlags UI, integrations hub, appearance customizer
- Froststrap Discord RPC

## Architecture (high level)

- **WPF** app (`Bloxstrap/` folder, `RootNamespace` Bloxstrap, assembly name `Vectorstrap`)
- **Bootstrapper** — downloads Roblox using CDN + local `Data.json` package maps
- **Settings UI** — `UI/Elements/Settings/MainWindow.xaml` navigation (keep lean)
- **Channel logic** — `ChannelViewModel`, `RobloxInterfaces/Deployment.cs`

## Branding

- Product name: **Vectorstrap**
- Author: **Cunzaki**
- Default theme: `Enums.Theme.Vectorstrap` (`UI/Style/Vectorstrap.xaml`)

## Build

```powershell
dotnet build Vectorstrap.slnx -c Release
```
