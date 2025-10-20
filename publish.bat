@echo off
chcp 65001 > nul
echo ========================================
echo   跨平台音乐榜单抓取工具 - 发布脚本
echo ========================================
echo.

set PROJECT_DIR=Cross_Platform_Auto_Fetcher
set OUTPUT_DIR=Release_Package
set VERSION=1.0.0

echo [1/4] 清理旧的发布文件...
if exist "%OUTPUT_DIR%" rmdir /s /q "%OUTPUT_DIR%"
mkdir "%OUTPUT_DIR%"

echo [2/4] 发布 Windows x64 单文件版本...
dotnet publish "%PROJECT_DIR%\Cross_Platform_Auto_Fetcher.csproj" ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output "%OUTPUT_DIR%\temp" ^
    /p:PublishSingleFile=true ^
    /p:PublishReadyToRun=true ^
    /p:IncludeNativeLibrariesForSelfExtract=true ^
    /p:EnableCompressionInSingleFile=true

if errorlevel 1 (
    echo.
    echo ❌ 发布失败!
    pause
    exit /b 1
)

echo [3/4] 整理发布文件...
move "%OUTPUT_DIR%\temp\Cross_Platform_Auto_Fetcher.exe" "%OUTPUT_DIR%\"
rmdir /s /q "%OUTPUT_DIR%\temp"

echo [4/4] 创建版本信息...
echo 跨平台音乐榜单抓取工具 v%VERSION% > "%OUTPUT_DIR%\版本信息.txt"
echo. >> "%OUTPUT_DIR%\版本信息.txt"
echo 发布日期: %date% %time% >> "%OUTPUT_DIR%\版本信息.txt"
echo. >> "%OUTPUT_DIR%\版本信息.txt"
echo 支持平台: >> "%OUTPUT_DIR%\版本信息.txt"
echo - QQ音乐 >> "%OUTPUT_DIR%\版本信息.txt"
echo - 酷狗音乐 >> "%OUTPUT_DIR%\版本信息.txt"
echo - 网易云音乐 >> "%OUTPUT_DIR%\版本信息.txt"

echo.
echo ========================================
echo ✅ 发布完成!
echo ========================================
echo.
echo 发布文件位置: %cd%\%OUTPUT_DIR%
echo 可执行文件: Cross_Platform_Auto_Fetcher.exe
echo.
echo 文件大小:
dir "%OUTPUT_DIR%\Cross_Platform_Auto_Fetcher.exe" | find "Cross_Platform_Auto_Fetcher.exe"
echo.
pause
