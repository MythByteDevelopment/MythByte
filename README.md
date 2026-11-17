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
| Client | `src/MythByte.Client` | Desktop game client (MonoGame from US04) | Networking, Shared |
| Tests | `tests/MythByte.UnitTests` | xUnit + Moq unit tests | Application, Domain, Shared |
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
```

Local builds have version `0.0.0-dev` (`Directory.Build.props`); release builds get their version from the Git tag.

## Ports

| Service | Port |
|---|---|
| API (development) | https 7390, http 5390 |
| API (server) | 127.0.0.1:5390 |
| Game server | UDP 7780 |

## Claude skill

`.claude/skills/mythbyte/SKILL.md` describes the coding conventions, architecture and art style (sober medieval, 64 x 64 sprites) for this repo, matching `MythByteDevelopment.md`.
