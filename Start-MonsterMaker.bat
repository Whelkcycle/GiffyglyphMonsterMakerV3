@echo off
setlocal
cd /d "%~dp0"

where dotnet >nul 2>nul
if errorlevel 1 (
  echo .NET 9 SDK was not found.
  echo Install the .NET 9 SDK, then run this file again.
  pause
  exit /b 1
)

dotnet --list-sdks | findstr /b "9." >nul
if errorlevel 1 (
  echo .NET 9 SDK is required.
  echo Install the .NET 9 SDK, then run this file again.
  pause
  exit /b 1
)

set ASPNETCORE_ENVIRONMENT=Development
echo.
echo Starting your local Monster Maker.
echo Keep this window open while using the app.
echo The first launch creates the local database automatically.
echo.
start "" http://localhost:5080
dotnet run --project "GiffyglyphMonsterMakerV3\GiffyglyphMonsterMakerV3.csproj" --no-launch-profile --urls "http://localhost:5080"
echo.
echo Monster Maker has stopped.
pause
