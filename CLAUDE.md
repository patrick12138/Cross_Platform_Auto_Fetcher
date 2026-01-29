# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

跨平台音乐榜单抓取工具 - 一个 WPF 桌面应用程序，用于抓取和导出 QQ 音乐、酷狗音乐和网易云音乐的榜单数据。

**技术栈**: .NET 6.0 WPF | 目标平台: Windows x64

## 构建和运行

### 开发环境

```bash
# 使用 Visual Studio 解决方案
dotnet build Cross_Platform_Auto_Fetcher.sln

# 或直接编译项目
dotnet build Cross_Platform_Auto_Fetcher/Cross_Platform_Auto_Fetcher.csproj

# 运行应用程序
dotnet run --project Cross_Platform_Auto_Fetcher/Cross_Platform_Auto_Fetcher.csproj
```

### 发布

**推荐方式**: 使用项目根目录的 `publish.bat` 脚本，自动完成编译、打包和版本信息生成：

```bash
publish.bat
# 输出: Release_Package/Cross_Platform_Auto_Fetcher.exe
```

**手动发布**:
```bash
dotnet publish Cross_Platform_Auto_Fetcher/Cross_Platform_Auto_Fetcher.csproj ^
    -c Release -r win-x64 --self-contained true ^
    /p:PublishSingleFile=true ^
    /p:PublishReadyToRun=true ^
    /p:IncludeNativeLibrariesForSelfExtract=true ^
    /p:EnableCompressionInSingleFile=true

# 输出路径: Cross_Platform_Auto_Fetcher/bin/Release/net6.0-windows/win-x64/publish/
```

## 架构设计

### 核心抽象层次

项目采用面向接口的设计，通过继承链实现代码复用和扩展性：

1. **接口层**: `IMusicDataService` 定义统一的音乐数据服务契约
   - `GetTopListAsync()`: 基础榜单获取
   - `GetTopListWithRetryAsync()`: 带重试机制的获取（推荐使用）

2. **基类层**: `MusicServiceBase` 实现通用的重试逻辑和日志记录
   - 所有平台服务都应继承此基类
   - 自动提供重试机制、错误处理和日志记录
   - 默认参数: 3 次重试，每次间隔 2000ms

3. **实现层**: 各平台的具体服务实现
   - `NeteaseMusicService`: 网易云音乐（使用 Weapi 加密接口）
   - `QQMusicService`: QQ 音乐
   - `KugouMusicService`: 酷狗音乐

### 关键设计模式

- **策略模式**: UI 层根据用户选择动态切换不同的音乐服务实现 (MainWindow.xaml.cs:72-78)
- **模板方法模式**: `MusicServiceBase` 定义重试流程框架，子类实现具体的 API 调用逻辑
- **工厂方法**: `CreateMusicService()` 根据平台名称创建服务实例 (MainWindow.xaml.cs:288-297)

## 网易云音乐加密实现

网易云音乐使用 Weapi 加密接口，涉及 AES + RSA 双重加密：

- **加密流程**: Services/Crypto/NeteaseCrypto.cs:18-31
  1. JSON payload → AES(CBC) with nonce → AES(CBC) with random key
  2. Random key reversed → RSA encrypt → encSecKey

- **调试技巧**: 如需验证加密逻辑，参考 `py_scripts/` 目录下的 Python 实现进行对照测试

## 日志系统

所有服务继承自 `MusicServiceBase` 后自动获得日志功能，使用 `Services/Log/FileLogger.cs`：

- 日志文件位置: `应用程序目录/Logs/yyyy-MM-dd.log`
- 每次 API 调用、重试、错误都会自动记录
- 调试时优先查看日志文件定位问题

## 新增平台支持

添加新音乐平台的步骤：

1. 在 `Services/` 目录创建新服务类，继承 `MusicServiceBase`
2. 实现 `GetTopListAsync(string topId, int limit)` 方法
3. 在 `MainWindow.xaml.cs` 的 `_platformCharts` 字典中添加平台配置 (line 24-42)
4. 在 UI 层的 switch 表达式中添加对应的服务实例化分支 (line 72-78)

重试机制和日志记录会自动继承，无需额外实现。

## 数据模型

`Song` 模型位于 `Services/Models/Song.cs`，包含以下字段：
- `Rank`: 排名
- `Title`: 歌曲名
- `Artist`: 艺术家（多个艺术家用 " / " 分隔）
- `Album`: 专辑名

## Python 脚本目录

`py_scripts/` 目录包含原型验证脚本，用于测试 API 接口和加密算法：
- 不参与应用程序构建
- 可用于新平台 API 的快速验证
- 网易云加密实现的 C# 移植参考了这些脚本

**主要脚本**:
- `debug_qqmusic_fixed.py`: QQ 音乐 API 测试
- `kugou_fixed.py`: 酷狗音乐 API 测试
- `qqmusic_optimized.py`: QQ 音乐优化版本

## 导出功能

所有导出文件保存在 `Exports/yyyy-MM-dd_HH-mm-ss/` 时间戳目录下，格式为 UTF-8 CSV：
- CSV 特殊字符处理逻辑见 `SanitizeForCsv()` 方法 (MainWindow.xaml.cs:278-286)
- 导出按钮在操作期间会自动禁用，防止并发问题 (MainWindow.xaml.cs:299-304)

## 榜单 ID 配置

平台榜单 ID 配置位于 `MainWindow.xaml.cs:24-42`：

**网易云音乐**:
- 热歌榜: 3778678
- 新歌榜: 3779629
- 飙升榜: 19723756

**QQ 音乐**:
- 热歌榜: 26
- 新歌榜: 27
- 飙升榜: 62

**酷狗音乐**:
- TOP500榜: 8888
- 飙升榜: 6666

## 故障排查

1. **API 调用失败**: 检查 `Logs/` 目录下的日志文件，查看详细错误信息
2. **数据解析错误**: 音乐平台 API 可能已更新结构，需要更新对应的服务实现
3. **加密问题**: 网易云音乐加密算法如有变化，可先用 Python 脚本验证，再更新 C# 实现

## 版本管理

当前版本定义在:
- `Cross_Platform_Auto_Fetcher.csproj`: `<Version>` 标签
- `publish.bat`: `VERSION` 变量

修改版本号时需要同步更新这两处。
