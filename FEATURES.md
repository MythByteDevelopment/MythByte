# Features

## Foundation
- Layered `MythByte.slnx` solution with Backend, Game, Client, Tests and Tools solution folders
- Strict project references per the dependency rules in `MythByteDevelopment.md`
- `Dependencies` class with a `RegisterX()` entry point per project; short `Program.cs` checklists
- Shared base types: `ApiResponse<T>`, `Roles`, `Permissions`, `FieldLimits`, `AppVersion`
- xUnit + Moq test projects (74 tests)
- `Directory.Build.props` with .NET 10, nullable, and version `0.0.0-dev` for local builds
- CI: every pull request to `main` is restored, built and tested by GitHub Actions
- Releases: publishing a GitHub release builds, tests and pushes the API and game server images to GHCR and attaches Windows and Linux client packages
- `GET /api/system/version`; the client shows its version on the title screen and in the window title

## Client
- MonoGame DesktopGL client on the generic host, Windows and Linux, fixed 60 FPS
- 1920 x 1080 render target scaled with point sampling and letterboxing; resizable window
- Action-based input for keyboard and gamepad (arrows/WASD, Z/Enter, X/Escape, D-pad/stick, A, B/Start)
- Screen stack with fade transitions: title, world placeholder and pause menu
- Built-in 5 x 7 pixel font and sober medieval colour palette

# To-do

## Bugs and fixes

## Foundation

## Client
- [ ] Tiled maps and camera (US05)
- [ ] Single-player movement and following critter (US09)

## Networking
- [ ] Dedicated game server and connections (US10)
- [ ] Packet contracts and validation (US11)
- [ ] Server-authoritative movement and synchronization (US12)
