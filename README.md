# MythByte

[![CI build and test](https://github.com/MythByteDevelopment/MythByte/actions/workflows/ci-build-test.yml/badge.svg?branch=main)](https://github.com/MythByteDevelopment/MythByte/actions/workflows/ci-build-test.yml)
[![Latest release](https://img.shields.io/github/v/release/MythByteDevelopment/MythByte?include_prereleases&sort=semver)](https://github.com/MythByteDevelopment/MythByte/releases)
[![License: all rights reserved](https://img.shields.io/badge/license-all%20rights%20reserved-6b6a64)](LICENSE)

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

`dotnet run --project src/MythByte.Client` opens the game window. It renders at 1920 x 1080 and scales to any window size with black bars (pixel art stays sharp). Settings live in `src/MythByte.Client/appsettings.json` (`Client` section: window size, fullscreen, target FPS, start map).

| Action | Keyboard | Gamepad |
|---|---|---|
| Move / navigate | Arrows or WASD | D-pad or left stick |
| Confirm | Z or Enter | A |
| Back / menu | X or Escape | B or Start |

Screens: title (shows the version), world and a pause menu (Resume, Title Screen, Quit). The world screen loads the Tiled map `Content/Maps/start.tmj` with placeholder tiles; the camera follows a placeholder player marker that moves one tile per key press and is blocked by the collision layer.

Maps are made in [Tiled](https://www.mapeditor.org/) and live in `src/MythByte.Client/Content/Maps` (tilesets in `Content/Tilesets`). Maps are saved as Tiled JSON (`.tmj`, tilesets `.tsj`). The game server copies the same `.tmj` and `.tsj` files to its output.

## Team and access

Access is given through GitHub teams in the MythByteDevelopment organization; they mirror the Discord roles.

| Team | Repo role | Board | Code owner for |
|---|---|---|---|
| Admins | Admin | Admin | Everything, `.github/`, `deploy/`, `Directory.Build.props` |
| Developers | Write | Write | All code |
| Pixel Artists | Write | Write | `src/MythByte.Client/Content/` |
| Testers | Triage | Write | - |

`main` only changes through pull requests: 1 approval from a code owner (`.github/CODEOWNERS`), the `build-test` check must pass, and force-pushes and deleting `main` are blocked.

Published releases are posted to the Discord releases channel by a GitHub webhook. Only Admins can create repositories in the organization, and merged branches are deleted automatically.

Security: Dependabot opens grouped update PRs for NuGet and GitHub Actions every Monday and security fixes as soon as a vulnerability is known, secret scanning blocks pushes that contain secrets, and CodeQL scans C# and the workflows on every push.

Pull requests are labelled automatically by the files they change (`code`, `art`, `docs`, `ci`), and Generate release notes groups them by those labels (`.github/release.yml`). When CI fails on `main` or a release fails, a message is posted in the Discord `mythbyte-github` channel (repository secret `DISCORD_CI_WEBHOOK`).

Issues use the templates in `.github/ISSUE_TEMPLATE` (Bug, User story, Task) and pull requests use `.github/PULL_REQUEST_TEMPLATE.md`. How to contribute is in `CONTRIBUTING.md`.

## License

All rights reserved: the code and art are public to read, not to reuse. See `LICENSE`.


## Continuous integration

`.github/workflows/ci-build-test.yml` runs on every pull request to `main` (and on pushes to `main`): restore, build (Release, `--no-restore`) and test (`--no-build`) of `MythByte.slnx` on .NET 10. A failing build or test fails the check. `main` is protected so pull requests can only be merged when this check passes.

## Releases

Publishing a GitHub release with a tag like `v1.2.3` (or `v1.2.3-beta.1` as pre-release) runs `.github/workflows/release.yml`:

1. Validates the tag and takes the version from it (`v1.2.3` becomes `1.2.3`).
2. Restores, builds and tests the solution with that version. Failing tests stop the release.
3. Waits for an Admin to approve the `release` environment (Actions run > Review deployments) before anything is published.
4. Builds and pushes the Docker images `ghcr.io/mythbytedevelopment/mythbyte-api` and `ghcr.io/mythbytedevelopment/mythbyte-gameserver` with the version tag, plus `latest` for non-pre-releases.
5. Publishes the client self-contained for Windows (`MythByte-<version>-win-x64.zip`) and Linux (`MythByte-<version>-linux-x64.tar.gz`) with `.sha256` files and attaches them to the release.

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
