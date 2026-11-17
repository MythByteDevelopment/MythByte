# MythByte: Full Multiplayer Critter RPG Development Plan

**Project blueprint — Version 1.3**

MythByte is a top-down, pixel-art multiplayer critter-collecting RPG in the style of classic Pokémon games, with a shared, semi-persistent world supporting approximately 50 concurrent players.

Players explore the world with a critter following them, run into wild critters in tall grass, fight them in turn-based battles, catch them, build a team, and battle and trade with other players.

The game is built new from the ground up: a MonoGame client in C#, a dedicated C# game server and an ASP.NET Core API, following the layered conventions from mydotnethelpers.nl, self-hosted on the jochemkok.nl Linux server with automatic, release-based updates. Only the game idea comes from the earlier ESP32/SDL2 MythByte prototype; none of its code is reused.

This document is an architectural and development blueprint, not an implementation guide. It intentionally contains no source code.

---

# 1. Project vision

## 1.1 Game concept

MythByte is a multiplayer critter RPG inspired by classic Pokémon games and older MMORPGs.

Players enter a shared world where they can:

- Explore towns, routes, forests, caves and dungeons.
- Walk through tall grass and run into wild critters.
- Fight wild critters in turn-based battles.
- Catch critters and build a team.
- Customise their character with preset accessories they can change at any time.
- Choose which critter follows them through the world.
- Level up their critters.
- Interact with NPCs, trainers and other players.
- Battle other players.
- Trade critters and items.
- Complete quests and discover secrets.
- Form parties and guilds.
- Explore the world together.

The world is shared between players rather than creating an individual world for every player. Battles are separate turn-based screens; the world keeps running around them.

## 1.2 Core gameplay loop

1. The player walks around the world with their equipped critter following them.
2. Every step in an encounter zone (tall grass, caves) has a chance to start a wild encounter.
3. A turn-based battle opens between the player's equipped critter and the wild critter.
4. Each turn the player chooses one of four actions:

| Action | Effect |
|---|---|
| Fight | Pick one of the critter's skills |
| Catch | Try to catch the wild critter; a failed attempt gives the opponent a free turn |
| Critter | View both critters' HP and speed (later: switch to another team critter) |
| Run | Try to escape; a failed attempt gives the opponent a free turn |

5. The critter with the higher current speed acts first that turn.
6. The battle ends when one critter reaches 0 HP, the wild critter is caught or the player escapes.
7. Caught critters join the player's team (maximum 10). Any team critter can be equipped.

## 1.3 Starting content

These come from the original game idea and become the first seed data.

**Starter skills:**

| Skill | Effect |
|---|---|
| Attack | Deals 20 base damage × the attacker's attack multiplier |
| Block | The critter takes only 30% of incoming damage this turn |
| Heal | Restores 10% of the critter's maximum HP, never above the maximum |
| Slow | Lowers the opponent's speed by 20 for the rest of the battle (minimum 1) |

**Starting species:**

| Species | HP | Speed | Attack multiplier |
|---|---:|---:|---:|
| Pile of Shit (starter) | 69 | 110 | 0.8 |
| Sir Gobby the Third | 70 | 130 | 0.6 |
| Flying Meatball | 130 | 70 | 1.0 |
| Tall Guy | 120 | 90 | 1.2 |
| Raccoon | 150 | 110 | 2.1 |
| El Donte | 200 | 150 | 1.5 |

**Starting rules:**

| Rule | Value |
|---|---|
| Encounter chance | 3% per step in an encounter zone |
| Encounter grace period | 5 seconds after entering the world or ending a battle |
| Catch chance | 30% |
| Run chance | 70% |
| Team size | 10 critters |

All values are data (seeded rows and `GameRulesOptions`), not constants in code, so balancing never needs a code change.

## 1.4 Initial technical targets

| Category | Target |
|---|---|
| Perspective | Top-down 2D |
| Graphics | Pixel art (64 × 64 sprites and tiles) |
| Virtual resolution | 1920 × 1080 (30 × ~17 tiles visible), scaled to the window |
| Client framework | MonoGame |
| Client platform | Windows and Linux desktop |
| Primary language | C# |
| Backend | ASP.NET Core |
| Database | SQL Server 2022 |
| Networking | UDP with LiteNetLib |
| World type | Shared, semi-persistent |
| Players per world | 50 |
| Initial server tick rate | 20 Hz |
| Client rendering target | 60 FPS |
| Hosting | Self-hosted Linux server (Docker) |

These are initial design targets, not guaranteed performance specifications.

# 2. Technology architecture

## 2.1 Technology stack

| Layer | Technology | Purpose |
|---|---|---|
| Client | C# / .NET 10 + MonoGame (DesktopGL) | Rendering, input, audio, UI, game orchestration |
| Networking | LiteNetLib | Real-time multiplayer |
| Game server | C# / .NET 10 (generic host) | World simulation, encounters, battles |
| Backend | ASP.NET Core | REST API |
| Authentication | JWT bearer (BCrypt, TokenService) + single-use session tickets | Secure authentication |
| Database | SQL Server 2022 | Persistent data |
| ORM | Entity Framework Core | Database operations |
| Testing | xUnit + Moq | Automated testing |
| Hosting | Linux + Docker Compose | Dedicated game hosting |
| Reverse proxy | Host Nginx (existing jochemkok.nl server block) | API HTTPS and routing |
| Deployment | GitHub Releases → GHCR images → pull-based `update.sh` (systemd timer) | Automatic versioned deploys |

## 2.2 High-level architecture

```text
MythByte Client
  C# + MonoGame
  Rendering · Input · UI · Animation · Audio
      |                              |
      | UDP / LiteNetLib             | HTTPS (login, characters, tickets)
      v                              v
MythByte Game Server            MythByte API
  C# / .NET 10                    ASP.NET Core
  World · Encounters · Battles    Accounts · Authentication
  Trades · Networking             Characters · Session tickets
      |                              |
      |   both use Application + Infrastructure (same layers, same DbContext)
      v                              v
                 SQL Server 2022
       Persistent player, critter and world data
```

The game server does not call the API for persistence. It registers the same Application and Infrastructure layers through the generic host, the way the WinForms and CLI apps in the conventions reuse the backend. The API and the game server are two hosts on top of one set of business rules.

### Architectural principles

1. The game server is authoritative.
2. The client handles presentation and player input.
3. The server controls gameplay decisions: movement, encounters, battle results, catches and trades.
4. The API handles accounts, characters and session tickets.
5. The database is never directly accessible to game clients.
6. Shared contracts (API DTOs and packet contracts) live in Shared.
7. Game logic is independent of rendering technology.
8. Gameplay rules (movement, encounters, battle turns, catch and run chances, experience) are pure, dependency-free Application classes with no EF inside, unit-tested directly.
9. Every endpoint returns `ApiResponse<T>`; every packet result the client sees uses the same succeeded/message shape.

# 3. Solution and project structure

The project consists of layered .NET projects in one `.slnx` solution. Every project is named `MythByte.<Layer>`.

## 3.1 Solution structure

```text
MythByte/
│
├── MythByte.slnx
├── Directory.Build.props
├── .gitattributes
├── .gitignore
├── .dockerignore
├── README.md
├── FEATURES.md
├── NETWORKING_README.md
├── BATTLE_README.md
│
├── .github/
│   └── workflows/
│       ├── ci-build-test.yml
│       └── release.yml
│
├── src/
│   ├── MythByte.Domain/
│   ├── MythByte.Application/
│   ├── MythByte.Shared/
│   ├── MythByte.Infrastructure/
│   ├── MythByte.Networking/
│   ├── MythByte.API/
│   │   └── Dockerfile
│   ├── MythByte.GameServer/
│   │   └── Dockerfile
│   └── MythByte.Client/
│       └── Content/
│           ├── Sprites/
│           ├── Tilesets/
│           ├── Maps/
│           ├── Fonts/
│           └── Audio/
│
├── tests/
│   ├── MythByte.UnitTests/
│   └── MythByte.IntegrationTests/
│
├── tools/
│   └── MythByte.BotClient/
│
├── deploy/
│   ├── docker-compose.yml
│   ├── update.sh
│   ├── .env.example
│   ├── updater.env.example
│   ├── nginx/mythbyte.conf
│   └── systemd/
│       ├── mythbyte-update.service
│       └── mythbyte-update.timer
│
└── wiki/   (clone of MythByte.wiki.git, ignored by the main repo)
```

Solution folders in `MythByte.slnx`:

| Solution folder | Projects |
|---|---|
| `/Backend/` | Domain, Application, Shared, Infrastructure, API |
| `/Game/` | Networking, GameServer |
| `/Client/` | Client |
| `/Tests/` | UnitTests, IntegrationTests |
| `/Tools/` | BotClient |
| `/Solution Items/` | workflows, README, FEATURES.md, feature READMEs, `.gitignore` |

## 3.2 Project responsibilities

| Project | Responsibility |
|---|---|
| MythByte.Domain | Entities, enums, repository and service interfaces |
| MythByte.Application | Services, gameplay rules (`BattleRules`, `EncounterRules`, `CatchRules`, `MovementRules`, `ExperienceTable`), `Validate()` extensions, `ToDto()` mappers, enum converters |
| MythByte.Shared | API request/response DTOs, `ApiResponse<T>`, packet contracts, protocol version, `Permissions`, `FieldLimits`, `AppVersion` |
| MythByte.Infrastructure | `MythByteDbContext`, per-entity `Configure()` and seeders, repositories, `IPassword` (BCrypt) |
| MythByte.Networking | LiteNetLib transport, packet serialization, packet size limits |
| MythByte.API | Controllers, JWT, ticket issuing, rate limiter, Swagger, `GlobalExceptionHandler` |
| MythByte.GameServer | Authoritative multiplayer server (generic host, simulation loop, battle instances) |
| MythByte.Client | MonoGame desktop game |
| MythByte.UnitTests | Services against mocked repositories, pure rules with `[Theory]` |
| MythByte.IntegrationTests | Client/server, API and database tests |
| MythByte.BotClient | Headless test client for load tests |

## 3.3 Project dependency rules

- Domain references nothing.
- Application references Domain and Shared.
- Shared references nothing.
- Infrastructure references Application and Domain and implements the Domain interfaces.
- Networking references Shared.
- API references Application, Infrastructure and Shared.
- GameServer references Application, Infrastructure, Shared and Networking.
- Client references Shared and Networking only.
- BotClient references Shared and Networking only.

The game client never references Domain, Application, Infrastructure or server-side business logic. Rules the client needs for prediction (movement only) are mirrored through Shared contracts, not by referencing Application.

## 3.4 DI and startup

- Every backend project has one static `Dependencies` class with a public entry point: `RegisterApplication`, `RegisterInfrastructure`, `RegisterNetworking`, `RegisterAPI`, `RegisterGameServer`, each with private helpers per concern (`RegisterServices`, `RegisterRepositories`, `RegisterPersistence`, `RegisterAuthentication`, `RegisterRateLimiter`, `RegisterSwagger`, `RegisterSimulation`, `RegisterBattles`).
- The API `Program.cs` stays a checklist: register layers, `Database.Migrate()` in a scope, Swagger in Development (UI prefix `api`), forwarded headers, CORS, authentication, authorization, rate limiter, exception handler, `MapControllers`.
- The GameServer `Program.cs` is the same kind of checklist on `Host.CreateApplicationBuilder`: register Application, Infrastructure, Networking and GameServer, then run. The game server never migrates the database; the API owns migrations.
- Configuration uses the options pattern everywhere: `JwtOptions`, `TicketOptions`, `GameServerOptions` (port, tick rate, max players, snapshot rate), `WorldOptions` (chunk size, reset rules), `GameRulesOptions` (encounter chance, grace period, catch chance, run chance, team size, PvP turn timer). Each has a `SectionName`, `required` properties and `ValidateDataAnnotations().ValidateOnStart()`.

# 4. Client and rendering (MonoGame)

The client is a MonoGame DesktopGL project written entirely in C#. There is no native engine and no interop layer.

## 4.1 Responsibilities of MonoGame

MonoGame handles:

- Window creation.
- Rendering through `SpriteBatch`.
- Sprite and texture loading through the content pipeline.
- Tilemap rendering.
- Camera movement.
- Keyboard, mouse and gamepad input.
- Animation playback.
- Audio playback.
- Visual effects.

## 4.2 Responsibilities of the client code

The client coordinates:

- Screens: title, login, character select, character creator, wardrobe, world, battle, team, bag, trade, menus.
- Game state.
- Network connections.
- Player input messages.
- Remote player state and their following critters.
- Battle presentation (the server decides, the client animates).
- Team and bag interfaces.
- Game UI.
- Server communication.
- API communication (login, characters, tickets) through a small `IApiClient` that always returns `ApiResponse<T>`, with every failure collapsed into a failed envelope.

## 4.3 Rendering rules

- Built for PC: every sprite and tile is 64 × 64 pixels (tile size 64 px).
- Everything renders to a 1920 × 1080 render target that is scaled to the window with point sampling (letterboxed when the aspect ratio differs), so pixel art stays sharp. The camera shows about 30 × 17 tiles.
- Sprites are 64 × 64 PNGs in `Content/Sprites`, grouped in sprite sheets per subject; critters have a front sprite (opponent side) and a back sprite (player side); characters have walk frames per direction (down, up, left, right) and are drawn from layers (see 4.5). Battle screens may draw critters scaled up by whole numbers.
- Maps are made in Tiled (`.tmx`) with layers for ground, decoration, collision and encounter zones; the server reads the same map files for collision and encounter zones.
- Fixed timestep at 60 FPS on the client; the server's 20 Hz world is interpolated.

## 4.4 Client composition

- The client builds its services with `Host.CreateApplicationBuilder` and a `Dependencies.RegisterClient()` entry point, like the WinForms and CLI apps in the conventions, and resolves the MonoGame `Game` class from DI.
- Settings (API base URL, game server host and port) come from `appsettings.json` copied to the output, bound to `ClientOptions`.
- The JWT is kept in memory for the session; the client only stores what it needs to log in again.
- The client shows its version (from `AppVersion`) on the title screen.
- Controls: arrow keys or WASD to walk and move through menus, Z / Enter to confirm, X / Escape to go back, a key to open the team screen; gamepad mapped to the same actions.

## 4.5 Character customisation

Players build their look from presets; there is no free colour picking or drawing.

| Slot | Examples |
|---|---|
| Body | Base body presets (skin tones and builds) |
| Hair | Hairstyles in a few colours |
| Hat | Caps, beanies, hoods |
| Face | Glasses, masks |
| Top | Shirts, jackets |
| Bottom | Trousers, shorts, skirts |
| Shoes | Sneakers, boots |
| Back | Backpacks, capes |

Rules:

- Every slot except Body may be empty.
- Each accessory is a 64 × 64 sprite sheet with the same frames and directions as the base body, so the client draws the character as layers in a fixed order (Back when facing up last, otherwise first; then Body, Shoes, Bottom, Top, Hair, Face, Hat) without per-accessory code.
- A new character picks a body preset and accessories from the free starting set in the character creator, with a live preview walking in all four directions.
- Accessories can be changed at any time outside battle through the wardrobe screen; the change is visible to nearby players immediately.
- More accessories are unlocked later through shops, quests, leaders and events (`Accessories.UnlockType`: `Default`, `Shop`, `Quest`, `Event`).
- Appearance travels over the network as a small list of accessory ids per slot, sent once on spawn and again only when it changes.
- The server only accepts accessories the character owns and that fit the slot; adding a new accessory is a new sprite sheet plus a seeded `Accessories` row, no code change.

# 5. Multiplayer networking

## 5.1 Networking technology

**LiteNetLib over UDP**

The networking system should support:

- Reliable messages.
- Unreliable messages.
- Ordered messages.
- Connection management.
- Player authentication.
- Packet serialization.
- Connection timeouts.
- Disconnection handling.
- Network latency monitoring.

Initial configuration:

| Setting | Value |
|---|---|
| Protocol | UDP |
| Library | LiteNetLib |
| Port | 7780 (7777 Terraria, 7778 tModLoader and 7779 SCP:SL are taken on the server) |
| Maximum players | 50 |
| Server tick rate | 20 Hz |
| Snapshot frequency | 10–20 Hz |
| Client rendering | 60 FPS |
| Authentication | Short-lived, single-use session ticket |

All values come from `GameServerOptions`, not constants in code.

## 5.2 Network packet categories

| Category | Messages |
|---|---|
| Connection | Connect, authenticate, disconnect |
| Player | Input, movement, state, equipped critter |
| World | Chunk data, world snapshots |
| Entities | Spawn, update, despawn |
| Encounter | Wild encounter started |
| Battle | Battle started, choose action, turn result, battle ended |
| Team | Team list, equip critter, reorder, rename |
| Appearance | Equip accessory, appearance changed |
| Interaction | NPC interaction, object interaction |
| Inventory | Bag changes, item use |
| Trade | Trade request, offer, confirm, cancel, completed |
| Social | Chat, battle challenge, party and guild events |

Packet rules:

- Packet contracts and the `PacketType` enum live in Shared; serialization lives in Networking.
- Every incoming packet type has a static `Validate()` extension that the handler calls first, the same pattern as request DTOs in the API.
- The connect packet carries the protocol version and the client version. A mismatch is rejected with a clear message ("Update your client to vX.Y.Z") instead of a silent drop.
- Network ids for live entities are small integers assigned by the server per session; database Guids never go over the wire.
- Battle and trade messages are reliable and ordered; movement is unreliable and sequenced.
- The packet format is documented in `NETWORKING_README.md`.

## 5.3 Server-authoritative movement

The movement process:

1. Player presses a movement key.
2. Client sends movement input.
3. Server validates the input.
4. Server calculates movement.
5. Server checks collisions.
6. Server updates player position.
7. Server rolls for an encounter if the player stepped onto an encounter tile.
8. Server broadcasts relevant state.
9. Other clients interpolate the movement of the player and their following critter.

The client can predict its own movement to make controls responsive, but the server remains authoritative.

The movement and collision rules are a pure Application class (`MovementRules`) that takes a position, input, speed and collision data and returns the new position. Movement is tile-based with smooth interpolation between tiles, like classic critter games, which keeps validation simple: one tile per step, never through a collision tile.

## 5.4 Interest management

The server should not send every entity to every player.

The world will be divided into spatial chunks.

Initial design:

- 32 × 32 tiles per chunk (from `WorldOptions`).
- Players receive information about nearby chunks.
- Players entering a new area receive updated subscriptions.
- Entities outside visibility are not continuously synchronized.
- Static map data is shipped with the client and only verified by hash.
- Battle messages only go to the players in that battle.

This reduces unnecessary network traffic and server processing.

# 6. Game server architecture

The game server runs independently of the desktop client, as its own Docker container next to the API.

## 6.1 Server modules

| Module | Responsibility |
|---|---|
| Networking | Connections and packets |
| Connection Manager | Connected players |
| Authentication | Ticket redemption through `ISessionTicketService` |
| World Manager | Active maps |
| World Simulation | Fixed-timestep updates |
| Entity Manager | Players, following critters, NPCs and trainers |
| Movement System | Movement and speed validation |
| Collision System | Server-side collision |
| Encounter System | Encounter zones, step rolls and wild critter selection |
| Battle Manager | Battle instances, turn collection, turn resolution |
| Team System | Equipping, reordering and renaming critters |
| Trade System | Two-sided trade sessions |
| Interaction System | NPCs, healing centers, objects |
| Chunk Manager | World partitioning |
| Interest Management | Visibility and synchronization |
| Persistence | Saving and loading state through Application services |

## 6.2 Simulation model

The server should use a fixed-timestep simulation.

Initial target:

- 20 world updates per second.
- 50 milliseconds per tick.
- Independent network processing.
- Periodic state snapshots.
- Controlled simulation timing.

The server processes commands through a centralized simulation loop, hosted as a `BackgroundService` that owns the simulation thread and passes the `stoppingToken` through.

Network callbacks enqueue commands rather than directly modifying world entities from multiple threads.

Battles are turn-based and do not need the 20 Hz loop: the Battle Manager resolves a turn as soon as every side has chosen an action (or the PvP turn timer runs out), inside the same command queue, so battle state is never touched from two threads.

Persistence work (saving characters, critters, world state) creates a new DI scope per save via `IServiceScopeFactory`, never on the simulation thread itself, with try/catch inside the save loop so one failed save never stops the world.

## 6.3 Entity system

Initial entity types:

| Entity | Description |
|---|---|
| Player | Human-controlled character |
| Following critter | The player's equipped critter, walking one tile behind them |
| NPC | Non-playable character (shopkeeper, healer, quest giver) |
| Trainer | NPC that challenges players to a battle when they walk into view |
| Object | Interactable world object (sign, item ball, door) |

Wild critters are not world entities: they only exist inside a battle instance. Every entity has a unique identifier, position and relevant state. Live entities use the server's network id; persisted ones also have their Guid database id.

## 6.4 Battle system

Battles are server-side instances that the client only renders.

Battle types:

| Type | Sides |
|---|---|
| Wild | Player critter vs. wild critter (server picks the wild action) |
| Trainer | Player critter vs. NPC trainer team |
| PvP | Player critter vs. another player's critter |

Turn flow:

1. Server creates the battle instance with each side's critter and its current HP and speed.
2. Server sends "battle started" to the players involved; they are marked "in battle" for everyone nearby and cannot be encountered or challenged.
3. Each player sends one action: a skill, catch, switch critter or run.
4. For wild and trainer battles the server chooses the opponent's action.
5. Server resolves the turn with `BattleRules`: catch and run happen first, then the faster critter acts, then the slower one; Block reduces damage for that turn; HP and speed are clamped.
6. Server sends the turn result (actions, damage, healing, new HP and speed, messages to show).
7. Repeat until a critter faints, the wild critter is caught or a player runs.
8. Server applies the result (experience, caught critter, HP saved) through Application services and closes the instance.

Rules:

- `BattleRules`, `CatchRules` and `ExperienceTable` take plain inputs plus an injected random source and return results, so every outcome is reproducible in tests.
- Wild opponent actions use a simple weighted choice between its skills; trainers can get smarter choices later.
- PvP turns have a timer (`GameRulesOptions`); a player who does not choose in time uses Block.
- A disconnect during a wild battle ends it as a run; during PvP it counts as a loss after a short reconnect window.
- A critter at 0 HP cannot be equipped until it is healed at a healing center.
- Formulas and examples are documented in `BATTLE_README.md`.

## 6.5 Encounter system

- Encounter zones are marked in the map's encounter layer and link to an encounter table.
- Each encounter table lists species with a weight and a level range per zone.
- `EncounterRules` takes the step, the zone's table, the grace timer and a random source and returns either "no encounter" or the species and level.
- The grace period after joining or after a battle is tracked per player on the server.
- The server always decides; the client only learns about an encounter when the battle starts.

# 7. World architecture

## 7.1 World structure

The world should support:

- Multiple maps.
- Towns with a healing center and a shop.
- Routes with tall grass.
- Forests.
- Caves.
- Dungeons.
- Buildings and interior locations.
- Transition points.

Maps should be loaded independently.

## 7.2 World chunks

Each map is divided into chunks.

Chunks should contain:

- Tile information.
- Collision information.
- Encounter zones.
- Static object placement.
- NPC and trainer spawn definitions.
- Environmental metadata.

The server uses chunks for simulation and visibility. The client uses them for rendering and local caching.

## 7.3 Map transitions

Players should be able to move between maps.

Transition flow:

1. Player enters a transition area.
2. Server validates the transition.
3. Server changes the player's map.
4. Server updates nearby-player subscriptions.
5. Client receives the destination map id.
6. Client loads the destination.
7. Server confirms the player's new location.

# 8. Semi-persistent world and persistence

The world should not reset completely every time the server restarts, but it also should not require every environmental change to be permanent.

## 8.1 Permanent player data

- Account information.
- Character name and appearance (body preset and equipped accessory per slot).
- Unlocked accessories.
- Team of critters (species, nickname, level, experience, current HP, team slot, equipped).
- Bag items.
- Currency.
- Defeated trainers and leaders.
- Quest progression.
- Critter log (species seen and caught).

## 8.2 Semi-persistent world data

- Picked-up item balls.
- Rematch timers for trainers.
- Temporary events (for example a rare critter appearing in a zone for an hour).
- Temporary environmental changes.

These can expire or reset according to configurable rules in `WorldOptions`.

## 8.3 Permanent world data

- Map layout.
- Encounter tables.
- Species and skill definitions.
- Static NPC and trainer definitions.
- Buildings.
- Static objects.
- World configuration.
- Spawn locations.

## 8.4 Persistence rules

The server should:

- Keep active gameplay state in memory.
- Save battle results (caught critter, experience, HP) right after the battle ends.
- Save trades immediately, in one transaction.
- Periodically persist positions and recoverable world state.
- Restore valid world changes at startup.
- Remove expired temporary changes.
- Protect player progression from world resets.

World resets must never automatically reset player progression.

# 9. Backend architecture

The ASP.NET Core API is responsible for account management, characters and session tickets. It follows the standard API conventions: thin controllers, logic in services, queries in repositories, `ApiResponse<T>` on every endpoint and a central `GlobalExceptionHandler` mapping `KeyNotFoundException` → 404, `ArgumentException` → 400 and `AuthenticationException` → 401.

## 9.1 Authentication

Features:

- Account registration.
- Login.
- Password hashing with BCrypt behind `IPassword`.
- JWT bearer tokens from `TokenService` (claims NameIdentifier, Name, Role; HmacSha256; `ClockSkew = TimeSpan.Zero`). Bearer header rather than cookies, because the client is a native app.
- `OnChallenge` / `OnForbidden` write the same `ApiResponse` failure envelope with 401/403.
- Character ownership.
- Game session ticket generation.
- Administrative authentication with roles `Player`, `Moderator` and `Admin`, listed as `const string` role lists in a Shared `Permissions` class and used in `[Authorize(Roles = Permissions.X)]`.

The API issues short-lived, single-use tickets for entering the game server. Because the API and game server share the database, a ticket is a random token (`RandomNumberGenerator`) stored as a `GameSessions` row with the character, expiry and a `RedeemedAt` field. The game server redeems it through `ISessionTicketService`; a ticket that is expired or already redeemed is rejected, which also prevents replay.

The permanent account credentials and the JWT are never sent through the gameplay protocol.

## 9.2 Character management

Players should be able to:

- Create characters: name, body preset, starting accessories and starter critter.
- Change accessories in the wardrobe.
- Select characters.
- Rename characters, subject to rules.
- Delete characters (soft delete with `DeletedAt`).
- View character information and their team.
- Load saved progression.

Character and critter nicknames are validated by a `Validate()` extension on the request DTO (length from `FieldLimits`, allowed characters, uniqueness checked in the service for character names).

## 9.3 Database architecture

SQL Server 2022 will store the persistent game data.

Initial database entities:

| Table | Responsibility |
|---|---|
| Users | Player accounts |
| Characters | Player characters (including body preset) |
| Accessories | Accessory definitions (slot, name, sprite key, how it is unlocked) |
| CharacterAccessories | Accessories a character owns, and which one is equipped per slot |
| Species | Critter definitions (name, base HP, speed, attack multiplier, sprite key) |
| Skills | Skill definitions (damage, healing, block, slow) |
| SpeciesSkills | Which skills each species can use |
| Critters | Owned critter instances (owner, species, nickname, level, experience, current HP, team slot, equipped) |
| EncounterTables | Encounter zones with species, weights and level ranges |
| Items | Item definitions (catch items, potions) |
| Inventory | Bag contents per character |
| Quests | Quest definitions |
| CharacterQuests | Player quest progress |
| CritterLog | Species seen and caught per character |
| WorldStates | Temporary world changes |
| PlayerPositions | Saved positions |
| GameSessions | Session tickets and active or historical sessions |
| BattleLogs | One row per finished battle (type, sides, result, turns) |
| Trades | Finished trades with the critters and items exchanged |
| ItemMovements | Append-only log of item and currency changes |
| AuditLogs | Admin and moderator changes (`FindDifferences`) |

Use Entity Framework Core for persistence, following the data conventions:

- Guid ids and UTC `CreatedAt` / `UpdatedAt` / `DeletedAt`; soft delete via `HasQueryFilter`.
- Generic `IRepository<T>` with specific repositories inheriting it; interfaces in Domain, implementations in Infrastructure.
- Per-entity `Configure()` extension and per-entity seeder (the starting species and skills from section 1.3, encounter tables, items, a demo account per role for development only, seed emails on `@example.com`).
- `MigrationsHistoryTable("Migrations")`; read every generated migration before applying it.
- `EnableRetryOnFailure`, so explicit transactions are wrapped in `CreateExecutionStrategy().ExecuteAsync(...)`.

Important rules:

- Never save positions every simulation tick.
- Save battle results and trades immediately.
- Use database transactions for catching, trading, items and currency.
- A traded critter changes owner in the same transaction as the `Trades` row; it is never deleted and recreated.
- Never silently overwrite an item or currency amount: every change is an `ItemMovements` row (character, item, signed quantity, reason, timestamp), and the current amount is a reconcilable counter.
- Keep active simulation and battle state in memory.
- Use server-side validation for all changes.

# 10. Gameplay systems roadmap

These are the gameplay systems planned after multiplayer foundations work.

| System | Planned functionality |
|---|---|
| Player movement | Tile-based walking and running, four directions |
| Character customisation | Body presets and changeable preset accessories per slot |
| Following critter | Equipped critter walks behind the player, visible to others |
| Encounters | Step-based wild encounters in grass and caves |
| Wild battles | Turn-based Fight / Catch / Critter / Run |
| Skills | Attack, Block, Heal, Slow; more skills per species later |
| Catching | Catch chance, catch items with better odds |
| Team | Up to 10 critters, equip, reorder, rename |
| Critter progression | Experience from battles, levels raising HP, speed and attack |
| Critter log | Seen and caught species |
| Healing centers | Restore the whole team in towns |
| Items | Catch items, potions, speed boosts |
| Shops | NPC-based buying and selling |
| Economy | Currency from battles and selling |
| NPCs | Dialogue and interaction |
| Trainers | NPC battles with rewards and rematch timers |
| Leaders | Strong trainers that gate new areas |
| PvP battles | Challenge another player nearby |
| Trading | Player-to-player critter and item trades |
| Quests | Objectives and rewards |
| Chat | Global, local and party chat |
| Party system | Explore in a group |
| Guild system | Social organization |
| Events | Rare critters in a zone for a limited time |

These systems should be implemented incrementally, not all at once.

Rules with formulas (battle turns, catch chance, experience curve, encounter tables) live in pure Application classes and are documented in `BATTLE_README.md`.

# 11. Full development roadmap

## Phase 1: Client foundation

**Objective:** Create the MonoGame client foundation.

Tasks:

- Create the MonoGame DesktopGL project on .NET 10.
- Set up the content pipeline for sprites, tilesets, fonts and audio.
- Render to a 1920 × 1080 target with point-sampled scaling and 64 × 64 sprites.
- Load a Tiled map.
- Implement the screen system (title, world, battle, team).
- Implement input mapping for keyboard and gamepad.
- Verify Windows and Linux builds.

**Deliverable:** A MonoGame window that shows a scaled pixel-art map on Windows and Linux.

## Phase 2: .NET solution

**Objective:** Establish the software architecture.

Tasks:

- Create the `MythByte.slnx` solution with `dotnet new sln --format slnx`.
- Create all projects and solution folders.
- Configure project references exactly as in section 3.3.
- Establish shared contracts and `ApiResponse<T>` in Shared.
- Add a `Dependencies` class with `RegisterX()` per project.
- Configure options classes and logging.
- Establish test projects (xUnit + Moq).
- Add `Directory.Build.props` (`0.0.0-dev`), `AppVersion`, `ci-build-test.yml` and `release.yml`.
- Write the README, `FEATURES.md` and the first wiki pages (Architecture, Getting-started).

**Deliverable:** A clean, buildable solution with CI and versioning from day one.

## Phase 3: Single-player prototype

**Objective:** Recreate the core game idea locally.

Tasks:

- Implement tile-based player movement.
- Implement camera following.
- Add collision.
- Add sprite animations.
- Add the following critter.
- Add encounter zones and the encounter roll.
- Add a local battle screen using `BattleRules` (Fight, Catch, Critter, Run).
- Add the team screen with equip.
- Add basic UI.

**Deliverable:** A playable single-player version of the original MythByte idea.

## Phase 4: Multiplayer foundation

**Objective:** Connect multiple clients to one dedicated server.

Tasks:

- Implement LiteNetLib transport.
- Create dedicated server on the generic host.
- Implement connection management.
- Define packet serialization and packet `Validate()` extensions.
- Implement player authentication for the prototype.
- Implement movement input messages.
- Implement server-side movement with `MovementRules`.
- Implement world snapshots, including each player's following critter.
- Implement remote-player rendering.
- Implement disconnect handling.
- Add the game server Dockerfile and a first `deploy/docker-compose.yml`.

**Deliverable:** Two computers walking in the same world, each with their critter following them.

## Phase 5: Authentication and characters

**Objective:** Introduce persistent player identities.

Tasks:

- Build the API with JWT, `GlobalExceptionHandler`, rate limiter and Swagger.
- Create the SQL Server database, `DbContext`, configurations, seeders and first migration.
- Seed the starting species and skills.
- Implement account registration.
- Implement login.
- Implement session tickets.
- Implement character creation with name, body preset, starting accessories and starter choice.
- Implement the wardrobe (change accessories) and appearance sync to other players.
- Implement character selection.
- Load and save characters and teams from the game server through Application services.
- Add `GET /api/system/version`.

**Deliverable:** Players can log in and enter the world with their own character and starter critter.

## Phase 6: World simulation

**Objective:** Turn the multiplayer prototype into an actual shared world.

Tasks:

- Implement world manager.
- Implement maps and transitions.
- Implement chunk loading.
- Implement interest management.
- Implement server-side collision.
- Implement server-side encounter zones and encounter tables.
- Implement NPC entities.
- Implement healing centers.

**Deliverable:** A shared world with towns, routes and encounter zones.

## Phase 7: Battles and catching

**Objective:** Introduce the core critter mechanics on the server.

Tasks:

- Battle Manager and battle instances.
- Wild battles with Fight, Catch, Critter and Run.
- Skills: Attack, Block, Heal, Slow.
- Speed-based turn order.
- Catching and adding to the team.
- Team management: equip, reorder, rename.
- Experience and levels.
- Critter log.
- Fainted critters and healing.
- Trainer battles.

**Deliverable:** A playable multiplayer critter RPG.

## Phase 8: Persistence and economy

**Objective:** Make the world and player progression persistent.

Tasks:

- Character and team persistence.
- Battle logs.
- Bag and item persistence with `ItemMovements`.
- Quest persistence.
- World-state persistence.
- Item ball respawning.
- Trainer rematch timers.
- World reset configuration.
- Shops.
- Currency.

**Deliverable:** A semi-persistent world with player progression.

## Phase 9: Social features

**Objective:** Add multiplayer community systems.

Tasks:

- Local chat.
- Global chat.
- PvP battle challenges.
- Critter and item trading.
- Friends.
- Party system.
- Guilds.
- Group quests.

**Deliverable:** A social multiplayer experience.

## Phase 10: Hosting and optimization

**Objective:** Prepare MythByte for public use.

Tasks:

- Server deployment with Docker Compose in `/opt/mythbyte`.
- Pull-based updater (`update.sh` + systemd timer) with database backup and rollback.
- Nginx snippet for `jochemkok.nl/mythbyte/api/`.
- Firewall and router configuration for UDP 7780.
- Logging and monitoring.
- Crash recovery (`restart: unless-stopped`).
- Network optimization.
- Security review.
- Load testing with BotClient.
- 50-player simulation.

**Deliverable:** Publicly accessible MythByte server that updates itself on every GitHub release.

# 12. First playable milestone: MythByte v0.1

## Objective

Two players on separate computers should be able to connect to one dedicated server and walk around the same world, each with their critter following them.

### Required functionality

| Feature | Requirement |
|---|---|
| Client | MonoGame window renders the map correctly on Windows and Linux |
| Dedicated server | Runs without graphical interface |
| Networking | Clients connect over UDP |
| Player identity | Unique ID per connection |
| Movement | Server-authoritative, tile-based |
| Synchronization | Players see one another and each other's following critter |
| Interpolation | Remote movement appears smooth |
| Disconnection | Player removed from world |
| Reconnection | New valid session |
| Security | Arbitrary client teleportation prevented |
| Versioning | Client and server show their version; protocol mismatch rejected with a message |

### Explicitly excluded from v0.1

- Account registration.
- Character creation and customisation (v0.1 uses one fixed default look).
- Database.
- Encounters.
- Battles.
- Catching.
- NPCs.
- Quests.
- Trading.
- Guilds.
- Economy.

These features can wait until the multiplayer foundation is proven.

# 13. Testing strategy

Testing should be part of development rather than postponed until the game is supposedly finished.

## 13.1 Unit tests

xUnit + Moq, following the conventions:

- One `Mock<T>` per constructor dependency, system under test in `_sut`.
- Names follow `MethodName_Scenario_ExpectedResult`.
- Arrange / Act / Assert separated by blank lines.
- Found and not-found repository cases as separate tests; `Verify(..., Times.Exactly(n))` for writes.
- Pure rules use `[Theory]` + `[InlineData]`, with boundary values as their own `[Fact]` (speed tie, HP exactly 0, heal at full HP, speed at the minimum of 1, team exactly full).
- Random outcomes are tested by passing a fixed random source.

Test individual systems:

- Movement calculations.
- Collision detection.
- Encounter rolls and grace period.
- Battle turn order and skill effects.
- Block, heal and slow clamping.
- Catch and run chances, including the opponent's free turn.
- Team limits and equipping.
- Accessory equip rules (owned, right slot, not in battle).
- Experience and levels.
- Trade validation.
- World-state expiration.
- Packet and request `Validate()` extensions.

## 13.2 Integration tests

Test interactions between components:

- Client/server connections.
- Authentication.
- Ticket redemption, expiry and replay.
- Character, appearance and team loading.
- Character and team saving.
- Full wild battle from encounter to saved result.
- Trade between two connected players.
- World transitions.
- Disconnections, including during a battle.
- Reconnection.

## 13.3 Multiplayer load tests

Gradually increase simulated players using `MythByte.BotClient`, a headless client that logs in, redeems a ticket, walks through encounter zones and plays out battles.

| Test | Concurrent players |
|---|---:|
| Initial connection test | 2 |
| Basic synchronization | 5 |
| Early performance test | 10 |
| Intermediate test | 25 |
| Target performance test | 50 |

Measure:

- Server CPU usage.
- Server memory.
- Network bandwidth.
- Average latency.
- Packet loss.
- Server tick duration.
- Snapshot delivery.
- Battle turn resolution time.
- Database response time.

The target should be that the server consistently completes its 50 ms simulation budget under the intended 50-player workload, with many players in battle at the same time.

## 13.4 Continuous integration

- `ci-build-test.yml` runs on every pull request to `main`: checkout, setup-dotnet, restore, build `--no-restore`, test `--no-build`.
- Failing tests block the merge and block a release.

# 14. Hosting and deployment architecture

The hosting environment is the existing jochemkok.nl Linux server. Like the other apps there, MythByte runs in Docker and updates itself from GitHub Releases. There is no inbound SSH, so deploys are pull-based: the server polls GitHub, CI never pushes to it.

## 14.1 Deployment layout

```text
jochemkok.nl Linux server
│
├── /opt/mythbyte/
│   ├── docker-compose.yml
│   ├── .env
│   ├── update.sh
│   ├── backups/
│   └── state/
│
├── Containers (restart: unless-stopped)
│   ├── db           SQL Server 2022 (internal only)
│   ├── api          ASP.NET Core → 127.0.0.1:5390
│   └── gameserver   LiteNetLib → 0.0.0.0:7780/udp
│
├── Host Nginx (existing jochemkok.nl server block)
│   └── /etc/nginx/snippets/mythbyte.conf → /mythbyte/api/ → 127.0.0.1:5390
│
└── systemd
    ├── mythbyte-update.service
    └── mythbyte-update.timer
```

Port 5391 is reserved for a later admin panel. The API uses forwarded headers so the rate limiter sees real player IPs.

## 14.2 Version management (automatic)

- The version comes from the Git tag (`v1.2.3`); `Directory.Build.props` sets `0.0.0-dev` for local builds and CI passes `-p:Version=...`.
- `release.yml` runs on a published GitHub release: validate the tag, build and test, then push `ghcr.io/jochemkok132/mythbyte-api` and `ghcr.io/jochemkok132/mythbyte-gameserver` (version tag, plus `latest` for non-pre-releases).
- The same workflow publishes the MonoGame client as self-contained `win-x64` (`.zip`) and `linux-x64` (`.tar.gz`) packages with `.sha256` files and attaches them to the release. The client is not a Docker image.
- On the server, `update.sh` (systemd timer, every 5 minutes) checks the latest release, backs up the SQL Server database, sets `APP_VERSION`, runs `docker compose pull && docker compose up -d`, waits for `/api/system/version` to report the new version and rolls back image and database when it doesn't. `update.sh vX.Y.Z` pins a version, `update.sh latest` unpins.
- Because API and game server ship together under one version, the protocol version check in the client always matches what is running.

## 14.3 Deployment requirements

- Dedicated game server container.
- Separate API container.
- SQL Server database container.
- Firewall rules (published Docker ports bypass ufw, so only the ports in the compose file are reachable).
- UDP port forwarding for 7780 on the KPN modem.
- HTTPS for API communication through the existing Let's Encrypt setup.
- Automated database backups before every update.
- Server logs (`docker compose logs`).
- Restart policies.
- Health monitoring through `/api/system/version`.
- Secrets (`DB_PASSWORD`, `JWT_KEY`) in `/opt/mythbyte/.env`, never in the repository.
- README section "Releases and automatic updates" with the server commands.

## 14.4 Future hosting improvements

Once the player count grows:

- Separate database hosting.
- Multiple game worlds.
- Multiple server instances (then migrations move from API startup to a pipeline step).
- Centralized authentication.
- Server discovery.
- Load balancing.
- Administrative dashboard (Blazor WebAssembly at `/mythbyte/admin/`, using the `ApiResponse` + `IHttpService` pattern) for balancing species, skills and encounter tables.
- Update-now button in the admin dashboard (request file + systemd path unit, no Docker socket).

These are future possibilities, not requirements for the first release.

# 15. Security requirements

Security should be designed into the architecture.

| Area | Requirement |
|---|---|
| Authentication | Short-lived, single-use game session tickets |
| Passwords | BCrypt behind `IPassword` |
| Movement | Server validation, one tile per step |
| Encounters | Rolled only on the server |
| Battles | Server resolves every turn; the client only sends a choice |
| Catching | Server rolls the catch and adds the critter |
| Team | Server controls all changes |
| Appearance | Only owned accessories in the right slot are accepted |
| Currency | Server-controlled transactions with `ItemMovements` |
| Trading | Both players confirm the final offer; atomic transaction in an execution strategy |
| Networking | Packet `Validate()` and size limits |
| Connections | Rate limiting on the API (50/min per user, IP fallback) and per-connection packet rate limits on the game server |
| Database | No direct client access; container not published |
| Administration | Separate roles via `Permissions` |
| Deployment | Secrets in `.env`, outside source control |
| Errors | `GlobalExceptionHandler`; 500s log and return a generic message |

Additional considerations:

- Prevent replayed authentication tickets (`RedeemedAt`).
- Validate packet sequence numbers.
- Protect against malformed packets.
- Limit incoming packet rates.
- Reject battle actions for a critter or skill the player does not own.
- Lock a trade offer once both players confirm; any change resets both confirmations.
- Handle disconnects safely, including mid-battle and mid-trade.
- Avoid trusting client timestamps.
- Log suspicious gameplay behavior.
- Audit admin and moderator changes with `FindDifferences`.

# 16. Recommended development priorities

## Critical — Must work before expanding the game

- MonoGame client foundation.
- Dedicated server.
- UDP networking.
- Authoritative movement.
- Player and following-critter synchronization.
- Disconnect handling.
- CI and release-based versioning.

## High — Required for a playable critter RPG

- Authentication.
- Character management, customisation and starter choice.
- World maps.
- Encounters.
- Wild battles.
- Catching.
- Team management.
- Persistence.

## Medium — Required for a richer multiplayer experience

- Critter progression.
- Healing centers and items.
- Trainers.
- Shops.
- PvP battles.
- Trading.
- Chat.
- Quests.

## Later — Long-term additions

- Leaders.
- Parties and guilds.
- Timed rare-critter events.
- More skills per species and smarter opponents.
- Multiple world instances.
- Advanced economy.
- Admin dashboard.

# 17. Project milestones and deliverables

| Version | Milestone | Completion criteria |
|---|---|---|
| v0.1 | Multiplayer prototype | Two players and their critters share a world |
| v0.2 | Authentication | Accounts, customised characters and starters work |
| v0.3 | World simulation | Maps, towns, routes and encounter zones |
| v0.4 | Battles | Wild battles, catching and team management |
| v0.5 | Persistence | Characters, teams and world saving |
| v0.6 | Social systems | Chat, PvP battles and trading |
| v0.7 | Content expansion | Trainers, leaders, quests and more species |
| v0.8 | Hosting beta | Public server deployment |
| v0.9 | Performance beta | 50-player load testing |
| v1.0 | Initial release | Stable playable multiplayer |

These versions represent development milestones, not fixed deadlines. Each one is a GitHub release, so publishing it also deploys it.

# 18. Final architecture decisions

| Decision | Selected approach |
|---|---|
| Game genre | Multiplayer critter-collecting RPG |
| Perspective | Top-down 2D, tile-based movement |
| Art style | Pixel art, 64 × 64 sprites and tiles, 1920 × 1080 virtual resolution |
| Character look | Layered body preset + one preset accessory per slot, changeable at any time |
| Combat | Turn-based battles resolved on the server |
| Client | MonoGame DesktopGL, C# |
| Main application language | C# |
| Runtime | .NET 10 |
| Solution | `MythByte.slnx`, layered (Domain, Application, Shared, Infrastructure, API) |
| Desktop client | Windows and Linux, released as GitHub release assets |
| Maps | Tiled `.tmx`, read by client and server |
| Networking | LiteNetLib / UDP |
| Server | Dedicated C# server on the generic host, reusing Application + Infrastructure |
| Backend | ASP.NET Core with `ApiResponse<T>` |
| Authentication | JWT bearer + single-use session tickets in the database |
| Database | SQL Server 2022 |
| ORM | Entity Framework Core |
| Mapping and validation | `ToDto()` and `Validate()` extensions (no AutoMapper, no FluentValidation) |
| Testing | xUnit + Moq |
| World type | Shared, semi-persistent |
| Player capacity | 50 concurrent players |
| Simulation | 20 Hz world, event-driven battles |
| Hosting | Self-hosted Linux, Docker Compose |
| Deployment | GitHub Releases → GHCR → pull-based updater with backup and rollback |
| Architecture | Server-authoritative |

# 19. Immediate next steps

Follow this order:

- [ ] Create the MonoGame DesktopGL project and render a scaled 1920 × 1080 test map with 64 × 64 tiles.
- [ ] Draw the first 64 × 64 sprites (base body with walk frames, a few accessories, starter critter, grass tiles) as PNGs.
- [ ] Create `MythByte.slnx`, the projects and their references.
- [ ] Add `Directory.Build.props`, `ci-build-test.yml` and `release.yml`.
- [ ] Create MythByte.Shared networking contracts.
- [ ] Integrate LiteNetLib.
- [ ] Build the dedicated game server.
- [ ] Connect one client.
- [ ] Connect a second client.
- [ ] Implement authoritative movement and synchronization, including the following critter.
- [ ] Test disconnects and reconnections.

## Final recommendation

Do not build battles, catching and the whole backend immediately, or start with 50-player optimization.

First prove that a MonoGame client can cooperate with a C# dedicated server.

Once two players can walk around the same world with their critters behind them, MythByte has its multiplayer foundation. Battles, catching and trading then become incremental engineering problems rather than a giant unknown.

**Initial target: MythByte v0.1 — two players, two critters, one server, one shared pixel-art world.**
