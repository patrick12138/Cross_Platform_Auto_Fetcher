# Cross Platform Auto Fetcher (多平台音乐榜单抓取工具)

![Platform](https://img.shields.io/badge/platform-Windows-blue)
![.NET](https://img.shields.io/badge/.NET-6.0-purple)
![License](https://img.shields.io/badge/license-MIT-green)

这是一个基于 .NET 6 WPF 开发的桌面应用程序，旨在帮助用户快速抓取并导出各大主流音乐平台（QQ音乐、网易云音乐、酷狗音乐）的实时榜单数据。

## ✨ 主要功能

- **多平台支持**：集成 QQ音乐、网易云音乐、酷狗音乐三大平台接口。
- **榜单丰富**：支持抓取飙升榜、热歌榜、新歌榜等核心榜单。
- **数据导出**：一键导出为 UTF-8 编码的 CSV 文件，解决乱码问题。
- **稳定可靠**：内置重试机制和异常处理，针对反爬虫策略进行了优化（如网易云 API 降级方案）。
- **Python 验证**：提供 `py_scripts/` 目录，包含核心 API 的 Python 原型实现，便于调试和算法验证。

## 🏗️ 技术架构

本项目采用经典的 WPF 分层架构：

- **UI 层** (`MainWindow`): 负责用户交互，使用**策略模式**根据用户选择动态切换音乐服务。
- **服务层** (`Services`):
  - `IMusicDataService`: 定义统一的数据获取接口。
  - `MusicServiceBase`: 抽象基类，封装了 HTTP 请求、日志记录和重试逻辑。
  - `NeteaseMusicService` / `QQMusicService` / `KugouMusicService`: 具体平台的 API 实现。
- **模型层** (`Models`): 统一的数据实体（`Song`），屏蔽不同平台的数据结构差异。

## 🚀 快速开始

### 环境要求
- Windows 10/11 (x64)
- .NET 6.0 SDK (开发需要) / .NET 6.0 Runtime (运行需要)

### 构建与运行

1. **克隆项目**
   ```bash
   git clone https://github.com/patrick12138/Cross_Platform_Auto_Fetcher.git
   cd Cross_Platform_Auto_Fetcher
   ```

2. **编译运行**
   ```bash
   dotnet run --project Cross_Platform_Auto_Fetcher/Cross_Platform_Auto_Fetcher.csproj
   ```

3. **发布打包**
   运行根目录下的 `publish.bat` 脚本，将自动生成单文件可执行程序：
   ```bash
   ./publish.bat
   # 输出位置: Release_Package/Cross_Platform_Auto_Fetcher.exe
   ```

## 📂 项目结构

```text
Cross_Platform_Auto_Fetcher/
├── Cross_Platform_Auto_Fetcher/  # 主程序源码
│   ├── Services/                 # 核心业务逻辑 (API实现)
│   ├── Models/                   # 数据模型
│   ├── py_scripts/               # Python 原型/调试脚本
│   └── MainWindow.xaml           # UI 界面
├── Exports/                      # (运行时生成) 导出的 CSV 数据
├── Logs/                         # (运行时生成) 运行日志
└── CLAUDE.md                     # AI 助手开发指南与上下文
```

## 📝 开发指南

如果您希望贡献代码或修改 API 逻辑：

1. **API 调试**：建议先在 `py_scripts/` 下运行 Python 脚本验证接口可用性。
2. **文档参考**：详细的开发规范、API 参数和调试技巧请参考 [CLAUDE.md](CLAUDE.md)。

## 📄 许可证

MIT License
