@echo off
echo ================================================================================
echo                    WEBDULICH - Travel Management System
echo ================================================================================
echo.
echo Checking for running processes...
tasklist | findstr WEBDULICH.exe >nul
if %errorlevel% == 0 (
    echo Found running WEBDULICH process. Stopping it...
    taskkill /F /IM WEBDULICH.exe
    timeout /t 2 >nul
)
echo.
echo Starting WEBDULICH application...
echo.
dotnet run
