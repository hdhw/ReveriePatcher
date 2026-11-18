@echo off
cd /d "%~dp0"
if "%REVERIE_GAME_DIR%"=="" (
  echo Set REVERIE_GAME_DIR to your 20230414 Rec Room folder first.
  exit /b 1
)
dotnet build -c Release
