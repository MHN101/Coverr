# Coverr 🎵

A minimalist, modern Windows desktop cover art widget built with **WPF** and **.NET 8**.

![Coverr Logo](app.png)

## Features

- **Live Media Detection**: Integrates with Windows System Media Transport Controls (SMTC) to automatically fetch and display current playing music album art across Spotify, Apple Music, YouTube Music, web browsers, and media players.
- **Ambient Blurred Background**: Real-time Gaussian blur backdrop effect that reflects the vibrant colors of the album artwork.
- **Frameless & Draggable**: Clean, distraction-free floating window you can click and drag anywhere on your desktop.
- **Interactive**:
  - Click & drag to move the widget.
  - Double-click to toggle between normal and maximized view.
- **Standalone Portable Executable**: Can be built as a single, self-contained `.exe` without requiring .NET installation on target systems.

## Requirements

- Windows 10 (version 19041+) or Windows 11
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Getting Started

### Build and Run
```bash
dotnet build Coverr.csproj
dotnet run --project Coverr.csproj
```

### Publish Single-File Executable
```bash
dotnet publish Coverr.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o ./publish
```

## License
MIT License
