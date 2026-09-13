# Rendering in LocalNotion

LocalNotion consumes the independently versioned Sphere10.VisualRenderer NuGet
package. Its source and unit tests live in the private Sphere10 Commercial
workspace and are not part of this solution.

LocalNotion.Core/Rendering owns conversion of Notion objects and repository
metadata into the renderer's visual model. LocalNotion also owns repository
paths, persistence, links, synchronization, and optional HTML post-processing.
The renderer accepts visual objects and returns HTML/text and assets without
access to Notion credentials or repositories.

The public rendering API uses the Sphere10.VisualRenderer namespace. Refer to
the authorized package's README for standalone usage. See
[theme overrides](renderer-themes.md) for LocalNotion customization.

## Licensing and distribution

Sphere10.VisualRenderer is proprietary software supplied under the Sphere10
VisualRenderer License 1.0. That license permits royalty-free binary use and
redistribution in commercial, internal, hosted and free/open-source projects,
subject to its terms and any other applicable licenses. It does not license
the renderer's implementation source code for public distribution. Third-party
components and assets retain their own terms.

LocalNotion remains licensed under the [GNU GPL](../LICENSE). The separate
[COPYING.EXCEPTION](../COPYING.EXCEPTION) grants permission to link and convey
LocalNotion with the renderer only for material whose copyrights Herman
Schoenfeld or Sphere 10 Software Pty Ltd own or are authorized to license.
It does not grant an exception on behalf of other contributors. Before
redistributing a combination, comply with the exception and obtain any
additional permissions required by other copyright holders and third-party
suppliers. This exception does not make the renderer open source or change
the license of LocalNotion source code.

Keep LocalNotion's LICENSE, COPYRIGHT and COPYING.EXCEPTION with distributions.
The CLI project copies them to build and publish output as separate files,
including when publishing a single executable. The renderer's NuGet package
separately supplies its LICENSE.txt and THIRD-PARTY-NOTICES.md under
licenses/Sphere10.VisualRenderer/, with dependency notices in its third-party
subfolder. Preserve that directory in installers, containers and archives.
Do not replace the renderer license with LocalNotion's GPL license.

## Building

Restore the Sphere10.VisualRenderer version pinned in LocalNotion.Core.csproj
from a NuGet source containing the authorized binary package. LocalNotion does
not build, publish, or reach into the Commercial source tree.

For an unpublished local package, register its containing directory once:

~~~powershell
dotnet nuget add source <absolute-package-directory> --name sphere10-commercial-local
dotnet restore LocalNotion.sln
~~~

CI must have access to the pinned binary package before a LocalNotion release
can be built. Distribute the binary through the intended feed separately;
do not upload Commercial source as part of LocalNotion release automation.