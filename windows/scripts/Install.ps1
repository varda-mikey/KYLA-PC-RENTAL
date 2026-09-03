#Requires -RunAsAdministrator
param([string]$PairingFile,[string]$CustomerUser)
$ErrorActionPreference='Stop'
if (!$PairingFile) { $PairingFile=Read-Host 'Full path to kyla-pairing.json' }
if (!$CustomerUser) { $CustomerUser=Read-Host 'Existing standard local Windows customer username' }
$user=Get-LocalUser -Name $CustomerUser
if (!$user.Enabled) { throw 'Customer account must be enabled.' }
$admins=Get-LocalGroupMember -SID 'S-1-5-32-544'
if ($admins.SID.Value -contains $user.SID.Value) { throw 'Use a standard customer account, not an administrator.' }
$config=Get-Content -LiteralPath $PairingFile -Raw | ConvertFrom-Json
if (([Uri]$config.BaseUrl).Scheme -ne 'https' -or $config.DeviceToken.Length -lt 32) { throw 'Invalid pairing file.' }
# Verify pairing before enabling any screen protection.
$null=Invoke-RestMethod -Uri ($config.BaseUrl.TrimEnd('/')+'/api/device') -Headers @{Authorization=('Bearer '+$config.DeviceToken)} -TimeoutSec 20
$install=Join-Path $env:ProgramFiles 'KylaRental'
$data=Join-Path $env:ProgramData 'KylaRental'
if (Get-Service KylaRental -ErrorAction SilentlyContinue) { throw 'Kyla is already installed. Uninstall it first before pairing again.' }
$source=$PSScriptRoot
if (!(Test-Path (Join-Path $source 'Agent/Kyla.Agent.exe')) -or !(Test-Path (Join-Path $source 'Client/Kyla.Client.exe'))) { throw 'Extract the entire Windows package before installing.' }
New-Item -ItemType Directory -Path $install,$data -Force | Out-Null
# Credentials and pending requests are accessible only to SYSTEM and Windows administrators.
& icacls.exe $data /inheritance:r /grant:r '*S-1-5-18:(OI)(CI)F' '*S-1-5-32-544:(OI)(CI)F' | Out-Null
if($LASTEXITCODE -ne 0){throw 'Unable to protect configuration folder.'}
& icacls.exe $install /inheritance:r /grant:r '*S-1-5-18:(OI)(CI)F' '*S-1-5-32-544:(OI)(CI)F' '*S-1-5-32-545:(OI)(CI)RX' | Out-Null
if($LASTEXITCODE -ne 0){throw 'Unable to protect installation folder.'}
Copy-Item -LiteralPath (Join-Path $source 'Agent'),(Join-Path $source 'Client') -Destination $install -Recurse -Force
Copy-Item -Path (Join-Path $source '*-Kyla.ps1') -Destination $install -Force
@{BaseUrl=$config.BaseUrl;DeviceToken=$config.DeviceToken;CustomerSid=$user.SID.Value} | ConvertTo-Json | Set-Content (Join-Path $data 'config.json') -Encoding UTF8
New-Service -Name KylaRental -DisplayName 'KYLA Rental Protection' -BinaryPathName ('"'+(Join-Path $install 'Agent/Kyla.Agent.exe')+'"') -StartupType Automatic -Description 'Rental timer and recovery for the configured customer account.' | Out-Null
& sc.exe failure KylaRental reset= 86400 actions= restart/2000/restart/2000/restart/2000 | Out-Null
& sc.exe failureflag KylaRental 1 | Out-Null
Start-Service KylaRental
Write-Host 'Installed. Sign in to the configured customer account and test a short voucher.' -ForegroundColor Green
Write-Host 'Keep the pairing file private; remove its downloaded copy after verifying installation.'
Write-Host ('Emergency recovery: sign in as Windows admin and run '+(Join-Path $install 'Stop-Kyla.ps1'))
