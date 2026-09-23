# DhogNav

DhogNav is a FINAL FANTASY XIV plugin by McVaxius. The free public plugin provides an introduction and loads modules whose access is granted privately by the owner. Open it with `/dnav`.

## Community and access

Join [The Dumpster Fire community on Discord](https://discord.gg/VsXqydsvpu) for discussion and access arrangements. You can also [support McVaxius on Ko-fi](https://ko-fi.com/mcvaxius). Support does not automatically grant module access.

Keep the updated public DhogNav host installed and enabled. In `/apm`, include `DhogNav` in the plugin list and confirm publisher trust. Copy the direct private ZIP link and click APM's global **Check clipboard for updates** button. The package contains `DhogNav.Access.dll` and `DhogNav.json`; APM places both in the host's `tasks` directory.

The public host provides the access directory, validation and refresh IPC endpoints required by APM. Private versions advance independently of the public host version.

## Build

This repository contains the complete public host source, loader, public trust anchor and manifest validation. It builds without private source or files from `!cryptography`. With .NET 10 and Dalamud API 15 references available, run `dotnet build DhogNav.csproj -c Release -p:Platform=x64`. The package is `bin/x64/Release/DhogNav/latest.zip`; `Z:\dnavp.bat` also copies it to this repository's `latest.zip`.

Public release version: `1.2.0.3`. The CLR assembly identity remains `1.0.0.0` for private-module compatibility.

## Ownership

DhogNav is proprietary software. The public plugin is free to use; private module access and redistribution require authorization from McVaxius. See [LICENSE](LICENSE).
