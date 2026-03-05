@echo off
chcp 65001 > nul
echo ========================================
echo   璺ㄥ钩鍙伴煶涔愭鍗曟姄鍙栧伐鍏?- 鍙戝竷鑴氭湰
echo ========================================
echo.

set PROJECT_DIR=CrossPlatformAutoFetcher
set OUTPUT_DIR=Release_Package
set VERSION=1.1.0

echo [1/4] 娓呯悊鏃х殑鍙戝竷鏂囦欢...
if exist "%OUTPUT_DIR%" rmdir /s /q "%OUTPUT_DIR%"
mkdir "%OUTPUT_DIR%"

echo [2/4] 鍙戝竷 Windows x64 鍗曟枃浠剁増鏈?..
dotnet publish "%PROJECT_DIR%\CrossPlatformAutoFetcher.csproj" ^
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
    echo 鉂?鍙戝竷澶辫触!
    pause
    exit /b 1
)

echo [3/4] 鏁寸悊鍙戝竷鏂囦欢...
move "%OUTPUT_DIR%\temp\CrossPlatformAutoFetcher.exe" "%OUTPUT_DIR%\"
rmdir /s /q "%OUTPUT_DIR%\temp"

echo [4/4] 鍒涘缓鐗堟湰淇℃伅...
echo 璺ㄥ钩鍙伴煶涔愭鍗曟姄鍙栧伐鍏?v%VERSION% > "%OUTPUT_DIR%\鐗堟湰淇℃伅.txt"
echo. >> "%OUTPUT_DIR%\鐗堟湰淇℃伅.txt"
echo 鍙戝竷鏃ユ湡: %date% %time% >> "%OUTPUT_DIR%\鐗堟湰淇℃伅.txt"
echo. >> "%OUTPUT_DIR%\鐗堟湰淇℃伅.txt"
echo 鏀寔骞冲彴: >> "%OUTPUT_DIR%\鐗堟湰淇℃伅.txt"
echo - QQ闊充箰 >> "%OUTPUT_DIR%\鐗堟湰淇℃伅.txt"
echo - 閰风嫍闊充箰 >> "%OUTPUT_DIR%\鐗堟湰淇℃伅.txt"
echo - 缃戞槗浜戦煶涔?>> "%OUTPUT_DIR%\鐗堟湰淇℃伅.txt"

echo.
echo ========================================
echo 鉁?鍙戝竷瀹屾垚!
echo ========================================
echo.
echo 鍙戝竷鏂囦欢浣嶇疆: %cd%\%OUTPUT_DIR%
echo 鍙墽琛屾枃浠? CrossPlatformAutoFetcher.exe
echo.
echo 鏂囦欢澶у皬:
dir "%OUTPUT_DIR%\CrossPlatformAutoFetcher.exe" | find "CrossPlatformAutoFetcher.exe"
echo.
pause

