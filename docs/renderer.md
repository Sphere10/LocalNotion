# Rendering in LocalNotion

LocalNotion consumes the independently versioned Sphere10.VisualRenderer NuGet
package, currently 3.1.3 under the MIT license. Its implementation is not
built by this solution.

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

Restore the pinned Sphere10.VisualRenderer 3.1.3 package from public NuGet.
LocalNotion does not build or publish the renderer. Its version changes
independently of LocalNotion and of Framework. See the main README's Framework
source/package instructions: removing Enterprise/DRM does not make legacy
Framework.Application 3.1.2 a clean substitute.
