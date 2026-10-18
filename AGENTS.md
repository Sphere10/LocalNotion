# LocalNotion agent entrypoint

Use the [Sphere10 workspace master](<D:/Development/Sphere10/AGENTS.md>) and the centrally
registered physical Framework checkout. Keep only product facts and constraints
here; shared workflows, skills, roles and checkout state belong in the workspace.

Local Notion is a cross-platform .NET command-line application and content library that synchronizes Notion resources, renders them through Sphere10.VisualRenderer, and hosts Notion-authored CMS websites.

Preserve the GPL license and existing linking exception, the independently versioned MIT Sphere10.VisualRenderer 3.1.3 dependency, and the vendored Notion.Client provenance. LocalNotion must have no Enterprise/DRM dependencies, embedded product license or license-enforced quotas. Default Framework references use clean post-extraction source at ../Framework, configurable through Sphere10FrameworkRoot; no Enterprise checkout/feed is required. Public Framework.Application 3.1.2 still bundles DRM and is rejected. Package mode requires an explicit separately produced clean version (3.1.3 or later); do not claim public availability or a fresh public-clone build from local-source verification. See README.md for the current availability boundary. Product source: LocalNotion.CLI/** and LocalNotion.Core/** follow Framework source scope. Product NUnit tests: tests/** follow Framework test scope. Modify Notion.Client/** only for necessary SDK capability, preserving its upstream conventions and licensing. Do not invoke sync, publish, integration tests with live credentials, or deployment scripts as a normal build check.

Build: `dotnet build LocalNotion.sln -c Release -p:BuildNumber=0 -p:BuildRevision=0`
Test: `dotnet test tests/LocalNotion.Core.Tests/LocalNotion.Core.Tests.csproj -c Release -p:BuildNumber=0 -p:BuildRevision=0`
Launch: `dotnet run --project LocalNotion.CLI/LocalNotion.CLI.csproj -c Debug -p:BuildNumber=0 -p:BuildRevision=0 -- --help`

Pass an explicit BuildNumber for local verification: otherwise Directory.Build.targets increments the tracked LOCAL-BUILD-NUMBER file. User-facing CMS documentation lives in docs/cms; the embedded blank CMS schema lives with its Core implementation. Keep their supported behavior and command examples aligned.
