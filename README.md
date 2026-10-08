# DhogNav

DhogNav is a FINAL FANTASY XIV plugin by McVaxius. The free public plugin provides an introduction and loads modules whose access is granted privately by the owner. Open it with `/dnav`.

## Dalamud Release requirement

The public host checks the running Dalamud `BetaTrack` once when the plugin
instance is created. Only `release` (ignoring case and surrounding whitespace)
allows normal access loading. Staging, dev, `apiNN` previews, unknown/blank tracks
and detection errors keep the public shell enabled and immediately show only:

> You are not on Dalamud Release

Commands and Open Main/Config reopen that error window. Refresh returns false
without loading private code; access validation raises that explicit error before
processing package bytes. Directory/Validate/Refresh IPC providers remain
registered. The startup log records the captured track and diagnostic details;
callbacks reuse the decision without checking again or logging every frame.
Reloading the host makes one new check. Changing the running Dalamud branch
requires restarting the game.

Release eligibility does not replace signature, manifest, ABI or exact dependency
checks. Delivering this guard requires a public-host update; private build and
package processes are unaffected.
Developer hosts that link this source retain their existing behavior through the
existing `LOCAL_DEV_BUILD` compile flag.

Run `dotnet run --project tests/ReleaseGuard/ReleaseGuard.csproj -c Release`
for source-linked lifecycle tests with synthetic Dalamud/UI/package services.
They cover host behavior, not protected packaging or live Dalamud/APM acceptance.
Pass `-p:ReleaseGuardLocalDev=true` to check the shared developer-host path.

## Community and access

Join [The Dumpster Fire community on Discord](https://discord.gg/ac6gjDvR8R) for discussion and access arrangements. You can also [support McVaxius on Ko-fi](https://ko-fi.com/mcvaxius). Support does not automatically grant module access.

Keep the updated public DhogNav host installed and enabled. In `/apm`, include `DhogNav` in the plugin list and confirm publisher trust. Copy the direct private ZIP link and click APM's global **Check clipboard for updates** button. The package contains `DhogNav.Access.dll` and `DhogNav.json`; APM places both in the host's `tasks` directory.

The public host provides the access directory, validation and refresh IPC endpoints required by APM. Private versions advance independently of the public host version.

The introduction groups access information, community links and APM installation steps into three cards. Color, Language and **C** select the whole-window theme, language and compact spacing. The interface supports English, German, French, Spanish, Italian, Russian, Japanese, Korean, Simplified Chinese, Vietnamese, Brazilian Portuguese, Indonesian, Polish, Turkish and Hindi. Preferences share the existing version-1 configuration and preserve private settings. **Check installed access** keeps the existing access refresh action. Hindi captions and editor text use scoped Windows text shaping while retaining the existing native font roles and symbols. Current Debug/x64 source builds and offline Hindi window, control and save-path checks pass. Managed-font readiness, GPU rendering and game acceptance remain pending.

Window appearance retains colour, language and compact access when their Main
shortcuts are hidden. Transparency defaults to 100% opacity and fades to 50%
after ten unfocused seconds; settings retain both opacity values and the delay.
The titlebar keeps appearance and installed-access shortcuts. Main branding and
its expanded/collapsed title use the packaged icon, with the image space retained
while its texture loads. There is no public Mini window. Navigation, filters and
command enable switches belong to the separately authenticated module;
refreshing access does not start a navigation command. Image changes still
require game/GPU acceptance.

Hindi text uses native Windows font fallback. An unavailable menu caption becomes
a disabled **Hindi (unavailable)** choice without blocking other languages.
A failed saved Hindi selection shows an ASCII status and **Use English** through
existing preference saving; the saved language changes only on that action.
The public Release build passes; native/game acceptance of font recovery remains pending.

## Build

This repository contains the complete public host source, loader, public trust anchor and manifest validation. It builds without private source or files from `!cryptography`. Keep the `aethertekUI` checkout beside this repository. With .NET SDK 10.0.201 and Dalamud API 15 references available, run `dotnet build DhogNav.csproj -c Release -p:Platform=x64`. The eight-file public package includes `AethertekUI.dll` and `AethertekUI.Dalamud.dll` and is written to `bin/x64/Release/DhogNav/latest.zip`; `Z:\dnavp.bat` also copies it to this repository's `latest.zip`. GitHub Actions checks out the sibling UI repository using the read-only `AETHERTEKUI_DEPLOY_KEY` secret.

Public release version: `1.3.0.0`. The CLR assembly identity remains `1.0.0.0` for private-module compatibility.

The dependency checkout follows published AethertekUI main so compatible API additions are available without per-plugin pin updates. Publish shared library APIs before consumer changes. Authentication uses the repository-specific read-only SSH deploy key, which has no expiration; checkout does not persist credentials.

## Ownership

DhogNav is proprietary software. The public plugin is free to use; private module access and redistribution require authorization from McVaxius. See [LICENSE](LICENSE).
