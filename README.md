# MythByte

Top-down multiplayer critter RPG in sober medieval pixel art: explore a shared world, catch critters, battle and trade. Built in C# with a MonoGame client, a LiteNetLib game server and an ASP.NET Core API.

The full blueprint is `MythByteDevelopment.md`; the full manual lives in the GitHub wiki.

## Projects

| Solution folder | Project | Purpose | References |
|---|---|---|---|
| Backend | `src/MythByte.Domain` | Entities, enums, repository and service interfaces | nothing |
| Backend | `src/MythByte.Application` | Services, gameplay rules, validation, mapping | Domain, Shared |
| Backend | `src/MythByte.Shared` | `ApiResponse<T>`, DTOs, packet contracts, `Roles`/`Permissions`, `FieldLimits`, `AppVersion` | nothing |
| Backend | `src/MythByte.Infrastructure` | EF Core, repositories, hashing | Application, Domain |
| Backend | `src/MythByte.API` | REST API (accounts, characters, session tickets) | Application, Infrastructure, Shared |
| Game | `src/MythByte.Networking` | LiteNetLib transport and packet serialization | Shared |
| Game | `src/MythByte.GameServer` | Authoritative game server | Application, Infrastructure, Networking, Shared |
| Client | `src/MythByte.Client` | MonoGame DesktopGL game client (Windows and Linux) | Networking, Shared |
| Tests | `tests/MythByte.UnitTests` | xUnit + Moq unit tests | Application, Client, Domain, Shared |
| Tests | `tests/MythByte.IntegrationTests` | Integration tests | Application, Infrastructure, Networking, Shared |
| Tools | `tools/MythByte.BotClient` | Headless bot client for load tests | Networking, Shared |

Every project that registers services has a static `Dependencies` class with one `RegisterX()` entry point (`RegisterApplication`, `RegisterInfrastructure`, `RegisterNetworking`, `RegisterAPI`, `RegisterGameServer`, `RegisterClient`, `RegisterBotClient`).

## Quick start

Requirements: .NET 10 SDK, Visual Studio 2022 17.14+ / Visual Studio 2026 or Rider.

```
dotnet build MythByte.slnx
dotnet test MythByte.slnx
dotnet run --project src/MythByte.API
dotnet run --project src/MythByte.GameServer
dotnet run --project src/MythByte.Client
```

Local builds have version `0.0.0-dev` (`Directory.Build.props`); release builds get their version from the Git tag.

## Client

`dotnet run --project src/MythByte.Client` opens the game window. It renders at 1920 x 1080 and scales to any window size with black bars (pixel art stays sharp). Settings live in `src/MythByte.Client/appsettings.json` (`Client` section: window size, fullscreen, target FPS).

| Action | Keyboard | Gamepad |
|---|---|---|
| Move / navigate | Arrows or WASD | D-pad or left stick |
| Confirm | Z or Enter | A |
| Back / menu | X or Escape | B or Start |

Screens: title (shows the version), world (placeholder grid until US05) and a pause menu (Resume, Title Screen, Quit).

## Continuous integration

`.github/workflows/ci-build-test.yml` runs on every pull request to `main` (and on pushes to `main`): restore, build (Release, `--no-restore`) and test (`--no-build`) of `MythByte.slnx` on .NET 10. A failing build or test fails the check. `main` is protected so pull requests can only be merged when this check passes.

## Releases

Publishing a GitHub release with a tag like `v1.2.3` (or `v1.2.3-beta.1` as pre-release) runs `.github/workflows/release.yml`:

1. Validates the tag and takes the version from it (`v1.2.3` becomes `1.2.3`).
2. Restores, builds and tests the solution with that version. Failing tests stop the release.
3. Builds and pushes the Docker images `ghcr.io/mythbytedevelopment/mythbyte-api` and `ghcr.io/mythbytedevelopment/mythbyte-gameserver` with the version tag, plus `latest` for non-pre-releases.
4. Publishes the client self-contained for Windows (`MythByte-<version>-win-x64.zip`) and Linux (`MythByte-<version>-linux-x64.tar.gz`) with `.sha256` files and attaches them to the release.

Version numbers are never edited by hand. `GET /api/system/version` returns the running API version and the client shows its version on the title screen and in the window title.

To release: GitHub > Releases > Draft a new release, tag `vX.Y.Z` on `main`, Publish (or `gh release create vX.Y.Z --generate-notes`).

## Ports

| Service | Port |
|---|---|
| API (development) | https 7390, http 5390 |
| API (server) | 127.0.0.1:5390 |
| Game server | UDP 7780 |

## Claude skill

`.claude/skills/mythbyte/SKILL.md` describes the coding conventions, architecture and art style (sober medieval, 64 x 64 sprites) for this repo, matching `MythByteDevelopment.md`.
