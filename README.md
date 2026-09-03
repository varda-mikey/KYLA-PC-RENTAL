# KYLA Piso Net

Windows 11 rental client with a web admin dashboard and one-use vouchers.

The current implementation is in `windows/`. Read [Windows setup](windows/README.md).

Download the self-contained Windows x64 package from the latest successful [Windows package workflow](../../actions). No .NET SDK is needed to run the published package.

Features: centrally validated one-use vouchers, combined codes, additional time, five-minute warning, fullscreen blocker, background service recovery, admin maintenance, and protected pairing credentials.

This is a supervised pilot. Windows 11 Home secure-desktop and game compatibility limitations still apply. Test on the actual rental PC before accepting payment.

The previous prototype under `src/` is retained for history and is superseded. Do not use its free time buttons.
