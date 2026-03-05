# GEMINI.md - 音乐抓取脚本

## 项目概述

本项目包含一系列Python脚本，旨在从多个中国音乐平台抓取音乐排行榜数据。目前支持QQ音乐和酷狗音乐。

**主要技术:**
- Python 3
- `requests` 库用于执行HTTP调用

**架构:**

该项目由一组独立脚本构成，而非一个正式的软件包。其关键组件如下：

- **API客户端:**
  - `qqmusic_optimized.py`: 一个基于类的客户端 (`QQMusicAPI`)，用于从QQ音乐官方API获取排行榜数据。
  - `kugou_fixed.py`: 一个基于类的客户端 (`KugouAPI`)，通过抓取酷狗排行榜页面并从HTML源码中内嵌的JSON对象里提取歌曲数据。

- **运行/工具脚本:**
  - `test_qqmusic.py`: 一个用于测试 `QQMusicAPI` 功能并在控制台显示结果的简单脚本。
  - `save_toplists.py`: 一个使用 `QQMusicAPI` 来获取QQ音乐排行榜并将结果保存为独立`.csv`文件到 `qqmusic_toplists/` 目录的脚本。

- **目录:**
  - `archive/`: 包含旧的、损坏的或已弃用的抓取脚本。
  - `qqmusic_toplists/`: 由 `save_toplists.py` 生成的CSV文件的输出目录。

## 构建与运行

本项目没有正式的构建流程。脚本直接通过Python解释器运行。

**依赖项:**

唯一的外部依赖是 `requests` 库。可以通过pip安装：
```bash
pip install requests
```

**运行脚本:**

- **测试酷狗排行榜:**
  ```bash
  python kugou_fixed.py
  ```

- **测试QQ音乐排行榜:**
  ```bash
  python test_qqmusic.py
  ```

- **获取QQ音乐数据并保存为CSV:**
  ```bash
  python save_toplists.py
  ```

## 开发约定

- **平台分离:** 每个音乐平台的逻辑都包含在各自专用的文件中（例如 `kugou_fixed.py`, `qqmusic_optimized.py`）。
- **关注点分离:** API客户端类与消费数据的脚本是分开的（例如 `save_toplists.py` 导入并使用 `QQMusicAPI`）。
- **归档:** 过时或无法工作的脚本应被移动到 `archive/` 目录中进行归档，而不是直接删除，以保留历史记录。