# KYLA Windows package — supervised pilot

This package runs on Windows 11 x64. .NET is bundled; installing the SDK is unnecessary for customers.

## Before installing

1. Have a separate Windows administrator account whose password customers do not know.
2. Create a **standard local customer account** in Windows Settings. The installer refuses an administrator account.
3. In the dashboard, add a PC and download `kyla-pairing.json`. Treat it like a password. Each PC needs its own file.
4. The device API must be reachable without a browser sign-in, with its device bearer credential. If the dashboard is still published privately, device installation will not work until device access is enabled.
5. Extract the whole Windows ZIP into a folder accessible to your administrator account.

## Install

Open **Windows PowerShell as administrator**, change to the extracted package folder, and run:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\Install.ps1
```

This invocation applies the script policy only to this process. It does not change the computer's permanent execution policy. Enter the full pairing-file path and the existing standard local customer username when prompted. The installer checks connectivity before installing the service. Sign in to the customer account afterwards.

The downloaded pairing file is no longer needed after installation is verified; remove its unprotected downloaded copy. Do not put it in a shared folder or repository.

## Use

- Sell a voucher generated in the dashboard. Codes are one use across all PCs belonging to that same admin account.
- On the rental PC, enter one code per line, up to 20 codes. All codes in a submission must be valid and unused, otherwise none are consumed.
- Time is added to the current session. A retry of an accepted submission does not add it again.
- Use the small timer button to extend. At five minutes, it turns orange.
- Expiry covers ordinary desktop apps and intercepts normal keyboard/mouse input. It does **not** send Escape, close, or pause games.
- The Windows service checks every two seconds for a missing client and restarts it in the configured customer's active console session.
- The cloud is checked every ten seconds. Remote changes apply after the next successful check.
- During a network outage, confirmed time counts down using a monotonic clock. New voucher submissions are retained in protected storage and retried with their original request ID.
- After reboot/service restart, the client stays blocked until online validation. Purchased time is measured as elapsed wall time, so it continues to expire while the PC is off.
- Maintenance mode temporarily unlocks the PC and keeps counting existing paid time. It is renewed by live server checks; it expires locally after 30 seconds without confirmation.

## Recovery and uninstall

Sign in to the separate Windows administrator account. The customer blocker is not launched on that account.
Run `C:\Program Files\KylaRental\Stop-Kyla.ps1` as administrator to stop protection. Run `Start-Kyla.ps1` to resume. Run `Uninstall-Kyla.ps1` to remove the service, then disable its pairing in the dashboard.

## Required on-device pilot checks

1. Start with zero time: confirm blocking at customer login.
2. Redeem a one-minute test voucher; confirm one minute is credited and expiry blocks.
3. Reuse the same voucher on the same and another PC: both must reject it.
4. Enter two unused codes together and confirm combined time.
5. Submit a valid code plus a used code: neither should be newly redeemed.
6. Extend while paid time remains. Confirm the time is added, not replaced.
7. Kill only the client from an administrator session; confirm its return within a few seconds when the customer is active.
8. Disconnect internet after successful redemption. Time should continue and expire. Reboot offline: the PC must stay blocked.
9. Test maintenance on/off, remote lock, disable pairing, and local administrator recovery.
10. Test each offered game in **borderless/windowed mode**, warning visibility, expiry, Alt+Tab, Windows key, and every connected monitor.

## Limits

Windows 11 Home is not a hardened kiosk. Ctrl+Alt+Delete, UAC/secure desktop, physical access, OS tampering, suspended processes, low-level-hook removal, and exclusive-fullscreen/anti-cheat interactions are not solved by this overlay. Controllers and other direct-input devices are not filtered. Restrict the pilot to supervised keyboard/mouse use and tested games. Reopening a killed app can leave a short gap. The package is unsigned and must be tested on the actual PC before collecting payment. Code/build validation is not evidence that Windows lockdown was tested.

The legacy `src/KylaPcRental.Client` prototype in GitHub is superseded by `windows/Client` and `windows/Agent`; do not use its old free time buttons.
