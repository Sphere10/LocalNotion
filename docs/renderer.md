# Rendering in LocalNotion

LocalNotion consumes the MIT-licensed Sphere10.Framework.Web.VisualRenderer
library from the selected Framework checkout. The renderer shares the Framework
source/package switch and package version; a separate renderer checkout is no
longer required.

LocalNotion.Core/Rendering owns conversion of Notion objects and repository
metadata into the renderer's visual model. LocalNotion also owns repository
paths, persistence, links, synchronization, and optional HTML post-processing.
The renderer accepts visual objects and returns HTML/text and assets without
access to Notion credentials or repositories.

The public rendering API uses the Sphere10.Framework.Web.VisualRenderer namespace. Refer to
the authorized package's README for standalone usage. See
[theme overrides](renderer-themes.md) for LocalNotion customization.

## Licensing and distribution

Sphere10.Framework.Web.VisualRenderer is MIT-licensed, as declared in its project
and included LICENSE.txt. Third-party components and assets retain their
own terms. Preserve those notices when redistributing the package's output.

LocalNotion remains licensed under the [GNU GPL](../LICENSE). The historical
[COPYING.EXCEPTION](../COPYING.EXCEPTION) remains preserved; it grants permissions
only for the copyrights described in that file and does not replace third-party
licenses. The current renderer's MIT license is independent of that exception.

Keep LocalNotion's LICENSE, COPYRIGHT and COPYING.EXCEPTION with distributions.
The CLI project copies them to build and publish output as separate files,
including when publishing a single executable. The renderer's NuGet package
separately supplies its LICENSE.txt and THIRD-PARTY-NOTICES.md under
licenses/Sphere10.Framework.Web.VisualRenderer/, with dependency notices in its third-party
subfolder. Preserve that directory in installers, containers and archives.
Do not replace the renderer license with LocalNotion's GPL license.

## Building

The default build uses the renderer project in `../Framework`. Set
`Sphere10FrameworkRoot` to select another complete Framework checkout. Missing
source produces a configuration error rather than silently changing reference mode.

```powershell
dotnet build LocalNotion.sln -c Release -p:BuildNumber=0 -p:BuildRevision=0
```

For package mode, make one tested Framework package set available through a
configured feed. Supply `UseSphere10FrameworkSource=false` and an exact
`Sphere10FrameworkVersion` for both restore and build.

```powershell
dotnet restore LocalNotion.CLI/LocalNotion.CLI.csproj --configfile <nuget-config> -p:UseSphere10FrameworkSource=false -p:Sphere10FrameworkVersion=<framework-package-version> -p:BuildNumber=0 -p:BuildRevision=0
dotnet build LocalNotion.CLI/LocalNotion.CLI.csproj -c Release --no-restore -p:UseSphere10FrameworkSource=false -p:Sphere10FrameworkVersion=<framework-package-version> -p:BuildNumber=0 -p:BuildRevision=0
```

Source and package builds retain the renderer's MIT and third-party notices in
the application's output. The renderer shares the Framework package version and remains independent of
LocalNotion. Sphere10-specific website themes remain external
site overlays; LocalNotion's CMS presets remain owned by Core, as described in
[theme overrides](renderer-themes.md).

These steps do not publish or install either product. Native CI and Docker
builds fetch the exact Framework source commit recorded in
`build/framework-source.ref`, including the renderer. Keep that pin aligned with
the tested dependency implementation. Retain package and runner identities when verifying
an unpublished renderer.
