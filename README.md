# KYLA PC RENTAL

Windows gaming-station rental system.

## V1 goals
- ₱40/hour base rate
- 1, 2, or 3 hour sessions
- 15-minute and 5-minute warnings
- Expiry sends ESC before locking the screen
- Full-screen rental lock overlay
- Game remains running in the background
- 1/2/3-hour extensions
- Admin PIN for operator controls
- Local-first timer; internet is not required for the core session timer

## Planned architecture
- Windows client: C# / .NET
- Local rental/session engine
- Full-screen lock overlay
- Game launcher for games installed on an external SSD
- Later: phone/admin controller over local Wi-Fi

## Important behavior
The rental client should lock the customer's interaction at expiry without intentionally closing or killing the game process. Game-specific pause behavior will be configurable because not every game treats `Esc` as pause.
