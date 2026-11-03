# Rendering in LocalNotion

LocalNotion consumes the independently versioned, MIT-licensed
Sphere10.VisualRenderer library. Workspace builds reference its current source
at `../Components/Sphere10.VisualRenderer`; a compatible NuGet package can be
selected explicitly as described below. The public 3.1.3 package lacks the
embedded-page APIs required by the current LocalNotion projection.

LocalNotion.Core/Rendering owns conversion of Notion objects and repository
metadata into the renderer's visual model. LocalNotion also owns repository
paths, persistence, links, synchronization, and optional HTML post-processing.
The renderer accepts visual objects and returns HTML/text and assets without
access to Notion credentials or repositories.

The public rendering API uses the Sphere10.VisualRenderer namespace. Refer to
the authorized package's README for standalone usage. See
[theme overrides](renderer-themes.md) for LocalNotion customization.

## Licensing and distribution

Sphere10.VisualRenderer 3.1.3 is MIT-licensed, as declared in its public NuGet
package and included LICENSE.txt. Third-party components and assets retain their
own terms. Preserve those notices when redistributing the package's output.

LocalNotion remains licensed under the [GNU GPL](../LICENSE). The historical
[COPYING.EXCEPTION](../COPYING.EXCEPTION) remains preserved; it grants permissions
only for the copyrights described in that file and does not replace third-party
licenses. The current renderer's MIT license is independent of that exception.

Keep LocalNotion's LICENSE, COPYRIGHT and COPYING.EXCEPTION with distributions.
The CLI project copies them to build and publish output as separate files,
including when publishing a single executable. The renderer's NuGet package
separately supplies its LICENSE.txt and THIRD-PARTY-NOTICES.md under
licenses/Sphere10.VisualRenderer/, with dependency notices in its third-party
subfolder. Preserve that directory in installers, containers and archives.
Do not replace the renderer license with LocalNotion's GPL license.

## Building

The default build uses the updated renderer checkout at
`../Components/Sphere10.VisualRenderer`. Set `Sphere10VisualRendererRoot` to use
another location. It is built as a separate project with the same configuration
as LocalNotion. A missing source checkout produces a configuration error rather
than falling back to an incompatible package.

```powershell
dotnet build LocalNotion.sln -c Release -p:BuildNumber=0 -p:BuildRevision=0
```

For package mode, first produce a distinctly versioned package from the updated
renderer and make it available through a configured NuGet feed. Set
`UseSphere10VisualRendererSource=false` and an exact
`Sphere10VisualRendererVersion` for both restore and build. Supplying a renderer
version also selects package mode when the source switch
is omitted, preserving existing development-package commands.

```powershell
dotnet restore LocalNotion.CLI/LocalNotion.CLI.csproj --configfile <nuget-config> -p:UseSphere10VisualRendererSource=false -p:Sphere10VisualRendererVersion=<renderer-package-version> -p:BuildNumber=0 -p:BuildRevision=0
dotnet build LocalNotion.CLI/LocalNotion.CLI.csproj -c Release --no-restore -p:UseSphere10VisualRendererSource=false -p:Sphere10VisualRendererVersion=<renderer-package-version> -p:BuildNumber=0 -p:BuildRevision=0
```

Source and package builds retain the renderer's MIT and third-party notices in
the application's output. The renderer version remains independent of
LocalNotion and Framework. Sphere10-specific website themes remain external
site overlays; LocalNotion's CMS presets remain owned by Core, as described in
[theme overrides](renderer-themes.md).

These steps do not publish or install either product. Native CI and Docker
builds fetch the exact source commits recorded in `build/framework-source.ref`
and `build/visual-renderer-source.ref`. Keep those pins aligned with the tested
dependency implementations. Retain package and runner identities when verifying
an unpublished renderer.
