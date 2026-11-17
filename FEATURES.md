# Features

## Foundation
- Layered `MythByte.slnx` solution with Backend, Game, Client, Tests and Tools solution folders
- Strict project references per the dependency rules in `MythByteDevelopment.md`
- `Dependencies` class with a `RegisterX()` entry point per project; short `Program.cs` checklists
- Shared base types: `ApiResponse<T>`, `Roles`, `Permissions`, `FieldLimits`, `AppVersion`
- xUnit + Moq test projects (9 tests)
- `Directory.Build.props` with .NET 10, nullable, and version `0.0.0-dev` for local builds

# To-do

## Bugs and fixes

## Foundation
- [ ] Build and test on every pull request (US02)
- [ ] Automatic releases from GitHub Releases (US03)

## Client
- [ ] MonoGame client foundation (US04)
- [ ] Tiled maps and camera (US05)
- [ ] Single-player movement and following critter (US09)

## Networking
- [ ] Dedicated game server and connections (US10)
- [ ] Packet contracts and validation (US11)
- [ ] Server-authoritative movement and synchronization (US12)
