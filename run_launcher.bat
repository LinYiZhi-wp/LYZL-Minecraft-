@echo off
echo [GeminiLauncher] Checking environment...

where dotnet >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] .NET SDK not found!
    echo Please install .NET 8 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b
)

echo [GeminiLauncher] Starting...
cd GeminiLauncher
dotnet run
if %errorlevel% neq 0 (
    echo [ERROR] Application failed to run.
)
pause
