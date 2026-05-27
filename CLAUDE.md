# MahjongScoring — Claude session notes

## What this repo is

Per-table score-entry PWA for a Dutch Mahjong club. Players open a link (shared via WhatsApp by the organizer), fill in end scores, and get back a result link to return to the organizer. No backend; everything runs offline-first as Blazor WASM on GitHub Pages.

Deployed at: `https://steffens-bridgemate.github.io/MahjongScoring/`

## Projects

| Project | Type | Purpose |
|---|---|---|
| `Tsump.Scoring` | Blazor WASM | The scoring web app |
| `Tsump.QrScanner` | WinForms .NET | Desktop utility for USB HID QR scanners (opens scanned URL in browser) |
| `external/MahjongIndeler` | git submodule | The organizer app repo — also provides `Tsump.Shared` (ScoreTable component, models, codec) |

## Two-repo architecture

`Tsump.Shared` lives in the **MahjongIndeler** repo. Changes to shared components (e.g. `ScoreTable.razor`) must be committed and pushed there first, then the submodule pointer is bumped here:

```powershell
cd external/MahjongIndeler
git checkout master        # submodule is usually detached HEAD — always checkout first
# ... make changes, commit, push ...
cd ../..
git add external/MahjongIndeler
git commit -m "Bump MahjongIndeler submodule (...)"
git push
```

The organizer app (`Tsump`) and the scoring app both use `ScoreTable`. Parameters that should differ between them (e.g. `ReadOnlyStart`) must be added to the component with a safe default so the organizer is unaffected.

## Deployment (GitHub Actions)

Workflow: `.github/workflows/deploy.yml`

Key steps:
- `actions/checkout@v4` with `submodules: recursive`
- `dotnet publish` with `-p:InformationalVersion="1.${{ github.run_number }}"` — this stamps the build number
- `sed` rewrites `<base href="/" />` → `<base href="/MahjongScoring/" />` for GitHub Pages path
- A second `sed` injects `<div class="build-version">1.${{ github.run_number }}</div>` directly into `index.html` before `</body>` — the version label is static HTML, not Blazor-rendered

## Known gotchas

**Submodule is detached HEAD by default.** Always run `git checkout master` inside `external/MahjongIndeler` before making changes there.

**Version label is in static HTML, not Blazor.** The `.build-version` CSS class (in `app.css`) styles the injected `<div>`. Do not add it via a Blazor component — it won't appear in the raw page source and may not render if Blazor is slow to load.

**Blazor scoped CSS (`.razor.css`) is unreliable** across deployment contexts. Put global styles in `wwwroot/css/app.css` instead.

**`img.naturalWidth` returns 150 in Blazor WASM** for viewBox-only SVGs. Always parse the canvas size from the SVG `viewBox` attribute directly:
```js
var vbMatch = svgMarkup.match(/viewBox="0 0 (\d+)/);
var size = vbMatch ? parseInt(vbMatch[1]) : qrSize;
```

**Do not render a green checkmark overlay on QR codes that will be scanned by ZXing** (html5-qrcode / the organizer app's scanner). ZXing cannot decode colored-overlay QR codes reliably. The overlay is fine for display; strip it before copying to clipboard (`QrCodeRenderer.ToSvg(url)` without the overlay overload).

**Service worker auto-update** uses `skipWaiting()` + `clients.claim()` + a `controllerchange` reload handler in `index.html`. Both `Tsump.Scoring` and the `Tsump` organizer app must have this pattern to pick up new deployments automatically.

## ScoreTable component parameters of note

| Parameter | Default | Set in scoring app |
|---|---|---|
| `ReadOnlyStart` | `false` | `true` — players cannot change starting points |
| `DefaultStartingPoints` | 30000 | From `invite.StartingPoints` |
| `Uma4Players` / `Uma3Players` | standard values | From invite |

## QR scanner desktop app (Tsump.QrScanner)

Small WinForms app for a **USB HID 2D scanner** plugged into a laptop. The scanner emulates a keyboard; the app keeps a TextBox focused, and on Enter opens the URL in the default browser and copies it to the clipboard. Always-on-top, dark-themed. Run with `dotnet run --project Tsump.QrScanner`.
