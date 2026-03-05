# Cross Platform Auto Fetcher

[English](README.md)

![骞冲彴](https://img.shields.io/badge/骞冲彴-Windows-blue)
![.NET](https://img.shields.io/badge/.NET-6.0-purple)
![璁稿彲璇乚(https://img.shields.io/badge/license-MIT-green)

Cross Platform Auto Fetcher 鏄竴涓熀浜?.NET 6 鐨?WPF 妗岄潰搴旂敤銆?鐢ㄤ簬鎶撳彇澶氫釜闊充箰骞冲彴锛圦Q 闊充箰銆佺綉鏄撲簯闊充箰銆侀叿鐙楅煶涔愶級鐨勬鍗曟暟鎹紝骞跺鍑轰负 CSV 鏂囦欢銆?
## 鍔熻兘鐗规€?
- 鏀寔 QQ 闊充箰銆佺綉鏄撲簯闊充箰銆侀叿鐙楅煶涔愪笁骞冲彴姒滃崟鎶撳彇銆?- 鏀寔澶氱被姒滃崟锛堝鐑瓕姒溿€佹柊姝屾銆侀鍗囨绛夛級銆?- 鏀寔 UTF-8 缂栫爜鐨?CSV 瀵煎嚭銆?- 鍐呯疆閲嶈瘯涓庢棩蹇楁満鍒讹紝鎻愬崌鎶撳彇绋冲畾鎬с€?- 鎻愪緵 `CrossPlatformAutoFetcher/py_scripts/` 涓嬬殑 Python 鍘熷瀷鑴氭湰锛屼究浜庢帴鍙ｈ皟璇曚笌楠岃瘉銆?
## 鎶€鏈爤

- .NET 6锛坄net6.0-windows`锛?- WPF 妗岄潰鐣岄潰
- 鍒嗗眰鏈嶅姟缁撴瀯锛?  - `MainWindow` 璐熻矗鐣岄潰浜や簰涓庢祦绋嬬紪鎺?  - `IMusicDataService` + `MusicServiceBase` 鎻愪緵缁熶竴鏈嶅姟鎶借薄
  - `QQMusicService`銆乣NeteaseMusicService`銆乣KugouMusicService` 璐熻矗骞冲彴瀹炵幇

## 蹇€熷紑濮?
### 鐜瑕佹眰

- Windows 10/11锛坸64锛?- .NET 6 SDK锛堝紑鍙戞椂锛?- .NET 6 Runtime锛堣繍琛屽彂甯冨寘鏃讹級

### 婧愮爜杩愯

```bash
git clone https://github.com/patrick12138/CrossPlatformAutoFetcher.git
cd CrossPlatformAutoFetcher
dotnet run --project CrossPlatformAutoFetcher/CrossPlatformAutoFetcher.csproj
```

### 鍙戝竷

```bash
./publish.bat
```

杈撳嚭璺緞锛?- `Release_Package/CrossPlatformAutoFetcher.exe`

## 椤圭洰缁撴瀯

```text
CrossPlatformAutoFetcher/
鈹溾攢 CrossPlatformAutoFetcher/         # 涓?WPF 宸ョ▼
鈹? 鈹溾攢 Services/                          # 骞冲彴鏈嶅姟瀹炵幇
鈹? 鈹溾攢 Models/                            # 鍏变韩鏁版嵁妯″瀷
鈹? 鈹溾攢 py_scripts/                        # Python 鎺ュ彛璋冭瘯/鍘熷瀷鑴氭湰
鈹? 鈹斺攢 MainWindow.xaml                    # 涓荤晫闈?鈹溾攢 publish.bat                           # 鏈湴鍙戝竷鑴氭湰
鈹斺攢 CLAUDE.md                             # 椤圭洰琛ュ厖璇存槑
```

## 璁稿彲璇?
MIT

