@echo off
setlocal
cd /d "%~dp0"
net session >nul 2>&1
if %errorlevel% neq 0 (
  echo KYLA needs Administrator permission to install safely.
  powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
  exit /b
)
echo.
echo ========================================
echo       KYLA PC RENTAL - SAFE INSTALL
echo ========================================
echo.
echo IMPORTANT: Installation verifies the server BEFORE enabling protection.
echo Keep your Windows administrator account available for emergency recovery.
echo.
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Install.ps1"
echo.
if errorlevel 1 (
  echo INSTALLATION DID NOT COMPLETE. KYLA protection was not intentionally enabled by this launcher.
) else (
  echo KYLA installation completed.
)
echo.
pause
