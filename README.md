# DhogNav

DhogNav is a FINAL FANTASY XIV plugin by McVaxius. The free public plugin provides an introduction and loads modules whose access is granted privately by the owner. Open it with `/dnav`.

## Community and access

Join [The Dumpster Fire community on Discord](https://discord.gg/ac6gjDvR8R) for discussion and access arrangements. You can also [support McVaxius on Ko-fi](https://ko-fi.com/mcvaxius). Support does not automatically grant module access.

Keep the updated public DhogNav host installed and enabled. In `/apm`, include `DhogNav` in the plugin list and confirm publisher trust. Copy the direct private ZIP link and click APM's global **Check clipboard for updates** button. The package contains `DhogNav.Access.dll` and `DhogNav.json`; APM places both in the host's `tasks` directory.

The public host provides the access directory, validation and refresh IPC endpoints required by APM. Private versions advance independently of the public host version.

The introduction groups access information, community links and APM installation steps into three cards. Color, Language and **C** select the whole-window theme, language and compact spacing. The interface supports English, German, French, Spanish, Italian, Russian, Japanese, Korean, Simplified Chinese, Vietnamese, Brazilian Portuguese, Indonesian, Polish, Turkish and Hindi. Preferences share the existing version-1 configuration and preserve private settings. **Check installed access** keeps the existing access refresh action. Hindi captions and editor text use scoped Windows text shaping while retaining the existing native font roles and symbols. Current Debug/x64 source builds and offline Hindi window, control and save-path checks pass. Managed-font readiness, GPU rendering and game acceptance remain pending.

## Build

This repository contains the complete public host source, loader, public trust anchor and manifest validation. It builds without private source or files from `!cryptography`. Keep the `aethertekUI` checkout beside this repository. With .NET SDK 10.0.201 and Dalamud API 15 references available, run `dotnet build DhogNav.csproj -c Release -p:Platform=x64`. The eight-file public package includes `AethertekUI.dll` and `AethertekUI.Dalamud.dll` and is written to `bin/x64/Release/DhogNav/latest.zip`; `Z:\dnavp.bat` also copies it to this repository's `latest.zip`. GitHub Actions checks out the sibling UI repository using the read-only `AETHERTEKUI_DEPLOY_KEY` secret.

Public release version: `1.3.0.0`. The CLR assembly identity remains `1.0.0.0` for private-module compatibility.

The dependency checkout is pinned to published AethertekUI revision `6c193cf06ac67f954c549cafc2033ac0efdd630a`, which contains the required Hindi text renderer/host, window opacity and community icon APIs. When adopting a newer library API, update this existing ref after that library revision is published.

## Ownership

DhogNav is proprietary software. The public plugin is free to use; private module access and redistribution require authorization from McVaxius. See [LICENSE](LICENSE).
