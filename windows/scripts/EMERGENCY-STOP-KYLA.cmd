@echo off
setlocal
net session >nul 2>&1
if %errorlevel% neq 0 (
  powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
  exit /b
)
echo.
echo KYLA EMERGENCY RECOVERY
echo This stops rental protection so the administrator can recover the PC.
echo.
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "Stop-Service KylaRental -Force -ErrorAction SilentlyContinue; Get-Process Kyla.Client -ErrorAction SilentlyContinue | Stop-Process -Force; Write-Host 'KYLA protection stopped. You may now troubleshoot safely.' -ForegroundColor Green"
echo.
pause
