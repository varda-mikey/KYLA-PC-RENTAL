#Requires -RunAsAdministrator
$ErrorActionPreference='Stop'
Stop-Service KylaRental -ErrorAction SilentlyContinue
Get-Process Kyla.Client -ErrorAction SilentlyContinue | Stop-Process -Force
& sc.exe delete KylaRental
Write-Host 'Service removed. Restart Windows, then remove Program Files\KylaRental and ProgramData\KylaRental if no longer needed.'
Write-Host 'Disable the old PC pairing in the dashboard. Voucher records remain on the server.'
