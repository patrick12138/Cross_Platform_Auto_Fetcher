# Cross Platform Auto Fetcher

[English](README.md)

![平台](https://img.shields.io/badge/platform-Windows-blue)
![.NET](https://img.shields.io/badge/.NET-6.0-purple)
![许可证](https://img.shields.io/badge/license-MIT-green)

Cross Platform Auto Fetcher 是一个基于 .NET 6 的 WPF 桌面应用。
它用于抓取多个音乐平台（QQ 音乐、网易云音乐、酷狗音乐）的榜单数据，并导出为 CSV。

## 功能特性

- 支持 QQ 音乐、网易云音乐、酷狗音乐多平台榜单抓取。
- 支持多种榜单类型（如热歌榜、新歌榜、飙升榜）。
- 支持 UTF-8 编码的 CSV 导出。
- 内置重试与日志机制，提升抓取稳定性。
- 提供 `CrossPlatformAutoFetcher/py_scripts/` 下的 Python 原型脚本，便于接口调试与验证。

## 技术栈

- .NET 6（`net6.0-windows`）
- WPF 桌面界面
- 分层服务结构：
  - `MainWindow` 负责界面交互和流程编排
  - `IMusicDataService` + `MusicServiceBase` 提供统一服务抽象
  - `QQMusicService`、`NeteaseMusicService`、`KugouMusicService` 负责平台实现

## 快速开始

### 环境要求

- Windows 10/11（x64）
- .NET 6 SDK（开发时）
- .NET 6 Runtime（运行发布包时）

### 源码运行

```bash
git clone https://github.com/patrick12138/CrossPlatformAutoFetcher.git
cd CrossPlatformAutoFetcher
dotnet run --project CrossPlatformAutoFetcher/CrossPlatformAutoFetcher.csproj
```

### 发布

```bash
./publish.bat
```

输出路径：
- `Release_Package/CrossPlatformAutoFetcher.exe`

## 项目结构

```text
CrossPlatformAutoFetcher/
|- CrossPlatformAutoFetcher/         # 主 WPF 工程
|  |- Services/                      # 平台服务实现
|  |- Models/                        # 共享数据模型
|  |- py_scripts/                    # Python 接口调试/原型脚本
|  `- MainWindow.xaml                # 主界面
|- publish.bat                       # 本地发布脚本
`- CLAUDE.md                         # 项目补充说明
```

## 许可证

MIT
