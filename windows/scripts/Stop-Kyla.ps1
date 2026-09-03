#Requires -RunAsAdministrator
$ErrorActionPreference='Stop'
# Explicit local administrator recovery; no customer-facing master voucher.
Stop-Service KylaRental
Get-Process Kyla.Client -ErrorAction SilentlyContinue | Stop-Process -Force
Write-Host 'Rental protection stopped. Run Start-Kyla.ps1 to resume. A restart also starts the service.'
