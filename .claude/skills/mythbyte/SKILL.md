---
name: mythbyte
description: How to build MythByte, the multiplayer critter RPG (MonoGame client, LiteNetLib game server, ASP.NET Core API, SQL Server), in Jochem's .NET conventions and the MythByte development plan. Use for any code, art, design, docs, issues or deployment work in this repository.
---

# MythByte

MythByte is a top-down multiplayer critter-collecting RPG in the style of classic Pokémon games: a shared, semi-persistent world for about 50 players, turn-based battles, catching critters with nets, a team of 5 plus a critter box, a following critter, day/night and region-bound spawns, housing, fast travel, trading, PvP, raids and world quests, plus an offline singleplayer mode. Everything is built new in C#. Nothing from the old ESP32/SDL2 C prototype is reused, only its game idea.

The full blueprint is `MythByteDevelopment.md` in the repo root (plan version 1.3). When this skill and the plan disagree, the plan wins; mention the difference.

## Working rules

- Never run `git commit`, `git push` or any git command that changes the index or history. Make the file changes; Jochem commits.
- Only do what is asked. A client task stays in the client; mention related improvements instead of building them.
- Every change also updates `README.md`, `FEATURES.md` and the matching wiki page in `wiki/` (and a `<FEATURE>_README.md` when formulas change).
- Write code without comments.
- When something is unclear and several options are reasonable, ask first with the multiple-choice question prompt (2–4 options, recommended first). Pick obvious defaults yourself and mention them.
- Before saying something works, build and run the tests (or launch the client/server). If it could not be tested, say so.
- When something is reported broken, find the real cause before changing code.
- Secrets never go in code or committed files: use `.env`, user secrets or options bound from configuration.
- Delete temporary helper files right after use.
- Replies short and direct: what changed and what Jochem still has to do.

## Solution layout

`MythByte.slnx` at the repo root, projects named `MythByte.<Layer>`:

| Solution folder | Projects |
|---|---|
| `/Backend/` | Domain, Application, Shared, Infrastructure, API |
| `/Game/` | Networking, GameServer |
| `/Client/` | Client (MonoGame DesktopGL) |
| `/Tests/` | UnitTests, IntegrationTests |
| `/Tools/` | BotClient |
| `/Solution Items/` | workflows, README, FEATURES.md, feature READMEs, `.gitignore` |

References, strictly:

- Domain → nothing.
- Shared → nothing.
- Application → Domain, Shared.
- Infrastructure → Application, Domain.
- Networking → Shared.
- API → Application, Infrastructure, Shared.
- GameServer → Application, Infrastructure, Shared, Networking. It uses the same layers as the API; it never calls the API for persistence.
- Client and BotClient → Shared, Networking only. Never Domain, Application or Infrastructure.

What lives where:

- **Domain**: entities, enums, repository and service interfaces.
- **Application**: services, pure rule classes (`BattleRules`, `EncounterRules`, `CatchRules`, `MovementRules`, `ExperienceTable`), `Validate()` extensions, `ToDto()` mappers, enum converters.
- **Shared**: `ApiResponse<T>`, API DTOs, packet contracts and `PacketType`, protocol version, `Permissions`, `FieldLimits`, `AppVersion`.
- **Infrastructure**: `MythByteDbContext`, per-entity `Configure()` and seeders, repositories, `IPassword` (BCrypt).
- **Networking**: LiteNetLib transport, packet serialization, size limits.

## .NET conventions

- .NET 10, nullable enabled, file-scoped namespaces.
- One static `Dependencies` class per project with a public `RegisterX()` (`RegisterApplication`, `RegisterInfrastructure`, `RegisterNetworking`, `RegisterAPI`, `RegisterGameServer`, `RegisterClient`) and private helpers per concern. `Program.cs` is a short checklist.
- Options pattern only: class with `const string SectionName`, `required` properties, `AddOptions<T>().Bind(...).ValidateDataAnnotations().ValidateOnStart()`, injected as `IOptions<T>`. Options in this project: `JwtOptions`, `TicketOptions`, `GameServerOptions`, `WorldOptions`, `GameRulesOptions`, `ClientOptions`. Never `Configuration["A:B"]` at call sites and never gameplay numbers as constants in code.
- Mapping with `ToDto()` / `ToXxxResponse()` extension methods. No AutoMapper.
- Validation with a static `Validate()` extension per request DTO and per incoming packet, called as the first line of the service or handler, throwing on the first problem. No FluentValidation.
- Domain enums and DTO enums are separate, converted with switch-based extensions that throw `ArgumentOutOfRangeException` on unknown values.
- Services throw `KeyNotFoundException` (404), `ArgumentException` (400), `AuthenticationException` (401). The API maps them in one `GlobalExceptionHandler : IExceptionHandler`; no try/catch per action. 500s log and return a generic message.
- JSON: camelCase, `JsonStringEnumConverter`, ignore nulls, `UtcDateTimeJsonConverter`. Never serialize EF entities.

## API

- Every endpoint returns `ApiResponse<T>` (`Value`, `Succeeded`, `Message`, static `Ok` / `Fail`). Controllers are thin.
- JWT bearer from `TokenService` (claims NameIdentifier, Name, Role; HmacSha256; `ClockSkew = TimeSpan.Zero`). `OnChallenge` / `OnForbidden` and the rate limiter's `OnRejected` write the same `ApiResponse` failure envelope.
- Roles `Player`, `Moderator`, `Admin` as `const string` lists in Shared `Permissions`, used in `[Authorize(Roles = Permissions.X)]`.
- Rate limiter: fixed window 50/min, partitioned by user id, falling back to IP. Forwarded headers on (API runs behind nginx).
- Session tickets: random token from `RandomNumberGenerator`, stored as a `GameSessions` row with character, expiry and `RedeemedAt`; the game server redeems it once through `ISessionTicketService`.
- `GET /api/system/version` (anonymous) for the updater health check.

## Data

- SQL Server 2022 with EF Core. Guid ids, UTC `CreatedAt` / `UpdatedAt` / `DeletedAt`, soft delete via `HasQueryFilter`.
- Generic `IRepository<T>` with specific repositories inheriting it; interfaces in Domain, implementations in Infrastructure.
- Per-entity `Configure()` extension and per-entity seeder (only on empty tables, deterministic, fixed Guids, demo accounts on `@example.com`, never on production data).
- `MigrationsHistoryTable("Migrations")`, `EnableRetryOnFailure`, explicit transactions inside `CreateExecutionStrategy().ExecuteAsync(...)`. Read every generated migration before applying it. Only the API migrates at startup; the game server never does.
- Catching, trading, items and currency are always one transaction. Item and currency changes are rows in the append-only `ItemMovements` table, never silent overwrites. A traded critter changes owner; it is never deleted and recreated.
- Never save positions every tick. Save battle results and trades immediately.

## Game server

- Generic host. The simulation runs at 20 Hz in a `BackgroundService` that owns the simulation thread. Network callbacks only enqueue commands; nothing touches world state from another thread.
- Battles are server-side instances resolved per turn (not per tick) through the same command queue. The client only sends a choice and animates the result.
- Persistence work uses a new scope per save via `IServiceScopeFactory`, never on the simulation thread, with try/catch inside the loop.
- Server-authoritative everything: tile-based movement (one tile per step), encounters rolled only on the server, battle turns, catches, team changes, accessory changes, trades.
- Network ids are small per-session integers; database Guids never go over the wire. Battle and trade messages reliable-ordered, movement unreliable-sequenced. Protocol and client version checked on connect, with a clear "update your client" message on mismatch.
- UDP port 7780 (7777–7779 are taken on the server).

## Gameplay rules

- Rules are pure Application classes: plain inputs plus an injected random source, plain results, no EF. Unit-test them directly.
- Battle actions: Fight (equipped skill), Catch (net), Critter (info only), Run. One critter per side for the whole battle, no switching. Catch and run resolve first, then skills by priority and speed. A failed catch or run gives the opponent a free turn. HP and speed are clamped (speed minimum 1, heal never above max). Every action gets a short freeze frame. Battles are skill-based: types, statuses, priority, cooldowns.
- Basic skills: Attack (20 × attack multiplier), Block (takes 30% damage this turn), Heal (10% of max HP), Slow (−20 speed for the battle). Every species also has unique skills, learned by level; equipped skills (4) can be changed on level up.
- The critters from the old ESP32 prototype are never used; MythByte gets six new starting species (still to be designed).
- Encounters only in encounter areas; chance depends on area, time of day and the critter's spawn weight (rarity). No grace period.
- Catch chance depends on the critter's HP, its level compared to the player's level, its catch rate/rarity and the net. Run chance depends on HP and speed.
- Team size 5; other critters go to the critter box (opened from the house or a tavern room).
- All content is modular data (JSON data files loaded by the seeders): species, types, rarities, regions, skills, items, nets, accessories, encounter tables, furniture, quests, raids. Numbers live in data and `GameRulesOptions`, never in code. Formulas are documented in `BATTLE_README.md`.
- A critter at 0 HP cannot be equipped until healed at a healing center.

## Client (MonoGame)

- MonoGame DesktopGL, Windows and Linux. Built with `Host.CreateApplicationBuilder` and `Dependencies.RegisterClient()`; the `Game` class comes from DI.
- Renders to a 1920 × 1080 render target, scaled to the window with point sampling (letterboxed). Tile size 64 px; about 30 × 17 tiles visible. 60 FPS fixed timestep; remote entities interpolated.
- Maps are Tiled `.tmx` with ground, decoration, collision and encounter layers; the server reads the same files.
- `IApiClient` always returns `ApiResponse<T>`; every failure collapses into a failed envelope.
- Screens: title, login, online/offline choice, character select, character creator, wardrobe, world, battle, team, critter box, bag, trade, house editor, fast travel map, menus.
- Keyboard, controller and mouse everywhere (everything clickable); mobile later.
- Show the version from `AppVersion` on the title screen.

## Art style

- **Sober medieval pixel art.** Muted, earthy palettes (stone greys, wood browns, moss and forest greens, faded reds and blues, iron and leather tones). Limited colours per sprite, clean readable silhouettes, light consistent from the top left. No neon, no glossy gradients, no modern or sci-fi elements, no cartoon exaggeration.
- Every sprite and tile is **64 × 64** PNG, grouped in sprite sheets per subject in `Content/Sprites` and `Content/Tilesets`.
- Characters have walk frames for down, up, left and right and are drawn in layers: Back (first, or last when facing up), Body, Shoes, Bottom, Top, Hair, Face, Hat. Every accessory sheet uses the same frame layout as the base body.
- Critters have a front sprite (opponent side) and a back sprite (player side). Battle screens may scale them up by whole numbers only.
- Towns, routes, caves and interiors follow the same medieval look: timber, thatch, cobblestone, banners, lanterns.

## Character customisation

- Slots: Body (required preset), Hair, Hat, Face, Top, Bottom, Shoes, Back (each may be empty). Presets only, no colour picker.
- Accessories are defined by `Accessories` rows (slot, name, sprite key, `UnlockType`: Default, Shop, Quest, Event) and owned through `CharacterAccessories`. Adding one is a sprite sheet plus a seeded row, no code change.
- Changeable any time outside battle through the wardrobe; the server only accepts owned accessories in the right slot. Appearance travels as accessory ids per slot, sent on spawn and on change.

## Testing

- xUnit + Moq. One `Mock<T>` per constructor dependency, system under test in `_sut`.
- Names `MethodName_Scenario_ExpectedResult`; Arrange / Act / Assert separated by a blank line.
- Found and not-found repository cases as separate tests. `Verify(..., Times.Exactly(n))` for writes. `Assert.ThrowsAsync` for expected failures.
- Pure rules with `[Theory]` + `[InlineData]`; boundary values (speed tie, HP exactly 0, heal at full HP, speed at 1, team exactly full) as their own `[Fact]`. Random outcomes with a fixed random source.

## Deployment and versions

- Version only from Git tags (`vX.Y.Z`); `Directory.Build.props` sets `0.0.0-dev` locally; CI passes `-p:Version=`.
- `ci-build-test.yml` on pull requests: restore, build `--no-restore`, test `--no-build`.
- `release.yml` on published releases: validate tag, build, test, push `ghcr.io/<owner>/mythbyte-api` and `mythbyte-gameserver` (lowercase owner, `latest` only for non-pre-releases), and attach the self-contained client as `win-x64` `.zip` and `linux-x64` `.tar.gz` with `.sha256`.
- Server: Docker Compose in `/opt/mythbyte` (db SQL Server 2022 internal only, api on `127.0.0.1:5390`, gameserver on `0.0.0.0:7780/udp`), nginx snippet for `jochemkok.nl/mythbyte/api/`, pull-based `update.sh` on a systemd timer with database backup, health check on `/api/system/version` and rollback. Port 5391 is reserved for a later admin panel. Never SSH or push from CI.
- Deploy files live in `deploy/` with LF line endings. Secrets in the server's `.env` only.

## Docs and issues

- Short `README.md` (what it is, projects, quick start, dev logins, ports, releases and automatic updates); full manual in the GitHub wiki cloned in the ignored `wiki/` folder.
- `FEATURES.md`: features per area, then `# To-do` with `- [ ]` items, bugs first.
- Issues and user stories in English, using the `[USxx]` / `[USxx-Tn]` format with epic labels, Depends on / Needed for kept in sync both ways, and every item on the project board with Status Todo, Sprint Backlog and hours filled in.
