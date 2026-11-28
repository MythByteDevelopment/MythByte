# Features

## Foundation
- Layered `MythByte.slnx` solution with Backend, Game, Client, Tests and Tools solution folders
- Strict project references per the dependency rules in `MythByteDevelopment.md`
- `Dependencies` class with a `RegisterX()` entry point per project; short `Program.cs` checklists
- Shared base types: `ApiResponse<T>`, `Roles`, `Permissions`, `FieldLimits`, `AppVersion`
- xUnit + Moq test projects (122 tests)
- `Directory.Build.props` with .NET 10, nullable, and version `0.0.0-dev` for local builds
- CI: every pull request to `main` is restored, built and tested by GitHub Actions
- Releases: publishing a GitHub release builds, tests and pushes the API and game server images to GHCR and attaches Windows and Linux client packages
- `GET /api/system/version`; the client shows its version on the title screen and in the window title
- GitHub teams (Admins, Developers, Pixel Artists, Testers) with repo and board roles, `CODEOWNERS` and a protected `main`
- Releases posted to the Discord releases channel; publishing images and client packages waits for an Admin approval (`release` environment)
- Dependabot (NuGet and Actions, weekly and security fixes), secret scanning with push protection and CodeQL
- Pull requests labelled by changed paths; generated release notes grouped by label; CI and release failures posted to Discord
- Issue templates (Bug, User story, Task), a pull request template, `CONTRIBUTING.md` and an all-rights-reserved `LICENSE`

## Client
- MonoGame DesktopGL client on the generic host, Windows and Linux, fixed 60 FPS
- 1920 x 1080 render target scaled with point sampling and letterboxing; resizable window
- Action-based input for keyboard and gamepad (arrows/WASD, Z/Enter, X/Escape, D-pad/stick, A, B/Start)
- Screen stack with fade transitions: title, world and pause menu
- Tiled JSON maps (`.tmj`, external `.tsj` or embedded tilesets) read with a `System.Text.Json` source generator by a shared `TiledMapReader` that the game server can use too
- Ground and decoration layers drawn at 64 px tiles, only the visible part of the map
- Map tiles stay correct when a tileset is reordered: maps store the `name` of each used tile (`tileNames:<tileset>` property), `TiledMapReader` remaps tiles by name and fails with the map, tileset, name and positions when a name no longer exists
- `tools/MythByte.MapTool` `sync-names` command writes or refreshes the `tileNames` properties and, with `--rewrite-ids`, rewrites a map's ids to the new tileset order (keeps Tiled's formatting and flip flags)
- Tiles bigger than the map grid (64 x 128, 128 x 64, 128 x 128) drawn at full size, anchored at the bottom-left of their cell like Tiled; big tiles just outside the view whose artwork reaches into it are drawn too
- Camera that follows the player and stays inside the map (small maps are centred)
- Local collision from the collision layer; placeholder player marker moves one tile per key press
- Placeholder sober medieval tileset and a 48 x 32 start map with town square, pond, path and tall grass
- Grass area tilesets in `Tilesets/Outdoor/Grass`, one sheet per artwork size: `grassarea_64x64` (ground, path and water pieces, small objects), `grassarea_64x128` (birch, dead, small oak and pine trees), `grassarea_128x64` (logs) and `grassarea_128x128` (big oak); every tile has a `name` property
- Built-in 5 x 7 pixel font and sober medieval colour palette

## Game server
- Headless game server on the generic host with `GameServerOptions` (port 7780, 20 Hz, 50 players, disconnect timeout)
- LiteNetLib UDP transport for server and client in `MythByte.Networking`
- Connect handshake with protocol and client version; outdated clients get "Update your client to vX.Y.Z."
- Connection manager with small network ids, max players, timeouts and a shutdown message for connected players

# To-do

## Bugs and fixes

## Foundation
- [ ] Modular content data (US74)

## Client
- [ ] In-app map builder (US79)
- [ ] Single-player movement and following critter (US09)

## Networking
- [ ] Packet contracts and validation (US11)
- [ ] Server-authoritative movement and synchronization (US12)

## World
- [ ] Day/night cycle (US51)
- [ ] Region-bound critters (US52)
- [ ] Housing area (US53)
- [ ] House customisation (US54)
- [ ] Tavern rooms (US56)
- [ ] Fast travel (US57)

## Critters
- [ ] Critter box (US55)
- [ ] Critter types and type chart (US60)
- [ ] Critter rarities (US61)
- [ ] Unique skills and learn sets (US62)
- [ ] Critter accessories (US63)
- [ ] Critter auras (US64)
- [ ] No starter critter: catch your first critter alone (US75)

## Content
- [ ] World quests (US58)

## Battles
- [ ] Raid battles (US59)
- [ ] Skill-based battles (US65)
- [ ] Freeze frame on battle actions (US66)
- [ ] Battle without a critter (US76)

## Economy
- [ ] Nets and net launchers (US67)

## Character
- [ ] Multiple characters per account (US68)
- [ ] Character levels and boosts (US69)
- [ ] Melee accessories (US77)

## Platforms
- [ ] Full controller support (US70)
- [ ] Mouse control (US71)
- [ ] Mobile support (US72)
- [ ] Offline singleplayer (US73)

## Admin
- [ ] Admin tools (US78)

## Hosting
- [ ] Sync GitHub teams to Discord roles (US80)

## Not yet issues
Ideas from the plan that are not GitHub issues yet.

- [ ] Write a lot of lore: world history, regions, critters, NPCs and the main storyline (`Lore.md`)
