# Contributing to MythByte

## Who does what
Access comes from your team in the MythByteDevelopment organization (see the README section "Team and access"). Ask an Admin to add you to the right team.

## Workflow
1. Pick a task or story from the project board and set it to In Progress.
2. Make a branch from `main` named after the story, for example `US05-tiled-maps-camera`.
3. Follow the conventions in `.claude/skills/mythbyte/SKILL.md` and `MythByteDevelopment.md`: layered projects, options pattern, no comments in code, tests for every rule.
4. Update the README, `FEATURES.md` and the matching wiki page in the same branch.
5. Open a pull request with the template and `Resolves #<story>`. CI (`build-test`) must pass and a code owner must approve.

## Issues
- Bugs: use the Bug template.
- New features: use the User story template; tasks are sub-issues of their story.
- Every new issue goes on the project board.

## Art and maps
Sprites and tiles are 64 x 64 PNG in a sober medieval style; maps are Tiled JSON (`.tmj`). Put them in `src/MythByte.Client/Content/`; Pixel Artists review them.

## License
MythByte is not open source. See `LICENSE`.
