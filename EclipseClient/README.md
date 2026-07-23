# Eclipse Client

Professional external Minecraft CPVP client with glass/black UI theme, authentication, module management, and DLL injection.

## Requirements

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Optional: MSVC (Visual Studio Build Tools) for compiling `EclipseCore.dll`

## Build (Command Prompt)

```cmd
cd EclipseClient
build.bat
```

Or manually:

```cmd
dotnet build src\EclipseClient\EclipseClient.csproj -c Release
```

## Run

```cmd
src\EclipseClient\bin\Release\net8.0-windows\EclipseClient.exe
```

Run as **Administrator** for DLL injection into Minecraft.

## Default Admin Account

| Email | Password |
|-------|----------|
| dropisnotdev0512@gmail.com | anas@drop007 |

## Features

- **Authentication** — Login, remember me, session saving, admin user management
- **Glass UI** — Borderless draggable window, custom title bar, dark eclipse theme
- **13+ Modules** — SPVP, Mace, Misc tabs with toggles and configurable settings
- **Customize** — Stream proof, UI/blur refresh rates, light/dark theme, 7 accent colors
- **Injection** — Finds `javaw.exe`, injects `EclipseCore.dll` via Win32 API
- **Footer Stats** — MC connection status, FPS, RAM, CPU

## Project Structure

```
EclipseClient/
├── build.bat
├── src/EclipseClient/     # WPF application
├── native/EclipseCore/    # Injectable DLL source
└── scripts/               # Icon generation
```

## Data Storage

User accounts and settings are stored in:
`%APPDATA%\EclipseClient\`
