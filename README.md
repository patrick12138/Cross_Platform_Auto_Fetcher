# Cross Platform Auto Fetcher

[简体中文](README.zh-CN.md)

![Platform](https://img.shields.io/badge/platform-Windows-blue)
![.NET](https://img.shields.io/badge/.NET-6.0-purple)
![License](https://img.shields.io/badge/license-MIT-green)

Cross Platform Auto Fetcher is a WPF desktop application built with .NET 6.
It fetches chart data from multiple music platforms (QQ Music, NetEase Cloud Music, Kugou Music) and exports the result to CSV.

## Features

- Multi-platform chart fetching from QQ Music, NetEase Cloud Music, and Kugou Music.
- Multiple chart types, including hot/new/rising style rankings.
- CSV export in UTF-8 encoding.
- Built-in retry and logging support for more stable fetch behavior.
- Python prototype scripts in `Cross_Platform_Auto_Fetcher/py_scripts/` for API debugging and validation.

## Tech Stack

- .NET 6 (`net6.0-windows`)
- WPF desktop UI
- Layered service structure:
  - `MainWindow` for UI interaction and orchestration
  - `IMusicDataService` + `MusicServiceBase` for service abstraction
  - `QQMusicService`, `NeteaseMusicService`, `KugouMusicService` for platform-specific implementations

## Quick Start

### Requirements

- Windows 10/11 (x64)
- .NET 6 SDK (for development)
- .NET 6 Runtime (for running packaged app)

### Run from Source

```bash
git clone https://github.com/patrick12138/Cross_Platform_Auto_Fetcher.git
cd Cross_Platform_Auto_Fetcher
dotnet run --project Cross_Platform_Auto_Fetcher/Cross_Platform_Auto_Fetcher.csproj
```

### Publish

```bash
./publish.bat
```

Build output:
- `Release_Package/Cross_Platform_Auto_Fetcher.exe`

## Project Structure

```text
Cross_Platform_Auto_Fetcher/
├─ Cross_Platform_Auto_Fetcher/         # Main WPF project
│  ├─ Services/                          # Platform service implementations
│  ├─ Models/                            # Shared data models
│  ├─ py_scripts/                        # Python API debug/prototype scripts
│  └─ MainWindow.xaml                    # Main UI
├─ publish.bat                           # Local publish script
└─ CLAUDE.md                             # Additional project notes
```

## License

MIT
