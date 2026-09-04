#Requires -RunAsAdministrator
param([string]$DeviceToken)
$ErrorActionPreference='Stop'
$baseUrl='https://lwjeroszacnqibggxair.supabase.co/functions/v1/kyla'
$configPath=Join-Path $env:ProgramData 'KylaRental\config.json'
$install=Join-Path $env:ProgramFiles 'KylaRental'
$packageRoot=Split-Path -Parent $MyInvocation.MyCommand.Path
if (!(Test-Path -LiteralPath $configPath)) { throw 'KYLA config not found. Install KYLA first.' }
if (!(Test-Path (Join-Path $packageRoot 'Agent\Kyla.Agent.exe'))) { throw 'Run this script from the extracted KYLA Windows package.' }
if (!(Test-Path (Join-Path $packageRoot 'Client\Kyla.Client.exe'))) { throw 'The extracted package is incomplete.' }
if (!$DeviceToken) { $DeviceToken=Read-Host 'Paste the new KYLA device token' }
if ([string]::IsNullOrWhiteSpace($DeviceToken) -or $DeviceToken.Length -lt 32) { throw 'Invalid device token.' }
$test=Invoke-RestMethod -Uri ($baseUrl.TrimEnd('/')+'/api/device') -Headers @{Authorization=('Bearer '+$DeviceToken)} -TimeoutSec 20
Stop-Service KylaRental -Force
Start-Sleep -Milliseconds 700
Copy-Item -LiteralPath (Join-Path $packageRoot 'Agent') -Destination $install -Recurse -Force
Copy-Item -LiteralPath (Join-Path $packageRoot 'Client') -Destination $install -Recurse -Force
$config=Get-Content -LiteralPath $configPath -Raw | ConvertFrom-Json
$config.BaseUrl=$baseUrl
$config.DeviceToken=$DeviceToken
$config | ConvertTo-Json | Set-Content -LiteralPath $configPath -Encoding UTF8
Start-Service KylaRental
Write-Host ('Connected to '+$test.name+' through Supabase.') -ForegroundColor Green
Write-Host 'KYLA binaries, backend URL, and device pairing were migrated successfully.' -ForegroundColor Green
