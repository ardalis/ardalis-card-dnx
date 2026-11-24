# Issue #75 — Add Support for DotNetConf 2026

Issue: https://github.com/ardalis/ardalis-card-dnx/issues/75

## Context

- `dotnetconf-score` / `dotnetconf-stats` looks up the playlist for a year from
  `https://ardalis.com/playlists.json`. That file currently lists 2021–2025 only, so
  `ardalis dotnetconf-stats 2026` reports "No playlist found".
- 2026 playlist (from the issue): https://www.youtube.com/playlist?list=PLJL9Ln_f6sGM
  (verified: page title ".NET Conf 2026").
- Ardalis's 2026 session: https://youtu.be/HK3IwBezg0w → highlight video ID `HK3IwBezg0w`.

## Assumptions

- The CLI itself should know about 2026 without waiting on a website deploy. The CLI will
  ship a built-in list of known .NET Conf playlists (2021–2026) and merge it with the
  remote `playlists.json`. Remote entries win for any year they define; built-in entries
  fill in missing years.
- If the remote `playlists.json` can't be fetched or parsed, the CLI falls back to the
  built-in list instead of failing.
- Updating `ardalis.com/playlists.json` (separate repo `ardalis-com-hugo`) is out of scope
  for this PR; it's a recommended follow-up.

## Phase 1 — Implementation

- [x] Add built-in playlist list (2021–2026) including the 2026 playlist + highlight video.
- [x] Merge remote and built-in playlists by year (remote takes precedence), sorted newest first.
- [x] Match the requested year by extracted year rather than substring.
- [x] Fall back to built-in playlists when the remote fetch/parse fails.
- [x] Highlight-video ID extraction supports `youtu.be/<id>` short links.
- [x] Update README examples to mention 2026.

## Phase 2 — Automated tests (tests/Ardalis.Cli.Tests, TUnit)

- [x] Unit: built-in playlists include 2026 with the issue's playlist URL and highlight video.
- [x] Unit: merge adds 2026 when remote lacks it; remote entry wins for duplicate years; result sorted descending.
- [x] Unit: `ExtractVideoId` handles `youtu.be` short links.
- [x] Handler: invalid remote JSON does not throw (falls back to built-ins).

## Phase 3 — Independent review

- [x] Separate agent reviews the full diff (correctness, requirements, tests, security, performance, accessibility).
- [x] Address all significant findings (or record reasons here).

## Phase 4 — Verification

- [x] `dotnet build && dotnet test && dotnet format --verify-no-changes` passes.
