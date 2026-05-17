# MahjongScoring

The per-table score-entry companion to the Mahjong club organizer at
[`MahjongIndeler`](https://github.com/Steffens-Bridgemate/MahjongIndeler).

A player opens a link shared by the organizer (via WhatsApp) and lands directly on a
score-entry view for their table. Filled-in scores are returned to the organizer as
another link. No backend; both apps run offline-first as PWAs.

Deployed to: `https://steffens-bridgemate.github.io/MahjongScoring/`

## Structure

```
MahjongScoring/
├── Tsump.Scoring/                  ← the scoring WASM app
├── external/MahjongIndeler/         ← git submodule providing Tsump.Shared
└── MahjongScoring.slnx
```

`Tsump.Shared` (the score-table component, models, language service, and payload codec)
lives in the **MahjongIndeler** repo and is included here via git submodule, so changes
made there are picked up here without duplication.

## First-time setup

```powershell
git clone https://github.com/Steffens-Bridgemate/MahjongScoring.git
cd MahjongScoring
git submodule update --init --recursive
dotnet build MahjongScoring.slnx
```

The CI workflow already runs `actions/checkout@v4` with `submodules: recursive`, so
deployments don't need any extra steps.

## Updating the shared library

When `Tsump.Shared` changes in the MahjongIndeler repo:

```powershell
cd external/MahjongIndeler
git pull
cd ../..
git add external/MahjongIndeler
git commit -m "Bump shared lib to <short-sha>"
git push
```

That advances the submodule pointer to the new commit; CI will rebuild against it.

## Running locally

```powershell
dotnet run --project Tsump.Scoring
```

Then in the organizer (`Player registration app` / MahjongIndeler) running on its own
port, set Settings → enable score entry and enable external scoring. The "Share scoring
link" buttons will produce URLs pointing at the deployed MahjongScoring origin (or at
`localhost:5151` while you're testing locally, depending on `ScoringAppConfig.DeployedUrl`).
