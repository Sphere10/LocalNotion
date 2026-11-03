#requires -Version 7.4

<#
.SYNOPSIS
Checks release metadata against valid and invalid immutable dependency source pins.
.DESCRIPTION
Runs the real metadata script in temporary fixtures with a mocked local Git HEAD.
No repository, dependency checkout, application, or external service is changed.
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$testRoot = [IO.Path]::GetFullPath((Join-Path $repoRoot 'publish/.framework-pin-tests'))
$testDirectory = Join-Path $testRoot ([guid]::NewGuid().ToString('N'))
$fixtureBuild = Join-Path $testDirectory 'build'
$revision = '0123456789abcdef0123456789abcdef01234567'
$rendererRevision = 'abcdef01abcdef01abcdef01abcdef01abcdef01'
$head = '89abcdef0123456789abcdef0123456789abcdef'
$previousOutput = $env:GITHUB_OUTPUT

function Test-SourcePin {
    param([string] $Scenario, [string] $Pin, [bool] $Valid, [switch] $VisualRenderer)

    $outputPath = Join-Path $testDirectory "$Scenario.outputs"
    $env:GITHUB_OUTPUT = $outputPath
    [IO.File]::WriteAllText((Join-Path $fixtureBuild 'framework-source.ref'), "HermanSchoenfeld/Sphere10.Framework $revision`n", [Text.UTF8Encoding]::new($false))
    [IO.File]::WriteAllText((Join-Path $fixtureBuild 'visual-renderer-source.ref'), "HermanSchoenfeld/Sphere10.VisualRenderer $rendererRevision`n", [Text.UTF8Encoding]::new($false))
    $pinFile = if ($VisualRenderer) { 'visual-renderer-source.ref' } else { 'framework-source.ref' }
    [IO.File]::WriteAllText((Join-Path $fixtureBuild $pinFile), $Pin, [Text.UTF8Encoding]::new($false))
    $calls = [Collections.Generic.List[string]]::new()
    function git {
        $arguments = @($args)
        if ($arguments.Count -ne 4 -or $arguments[0] -cne '-C' -or $arguments[1] -cne $testDirectory -or
            $arguments[2] -cne 'rev-parse' -or $arguments[3] -cne 'HEAD') {
            throw 'The metadata fixture attempted an unexpected Git operation.'
        }
        $calls.Add('rev-parse HEAD')
        $global:LASTEXITCODE = 0
        return $head
    }

    $failure = $null
    $result = $null
    try { $result = & (Join-Path $fixtureBuild 'ci-metadata.ps1') -BuildNumber 17 -Tag v1.7.0 }
    catch { $failure = $_.Exception.Message }
    if ($Valid) {
        if ($failure) { throw "Valid source pin '$Scenario' was rejected: $failure" }
        if ($result.framework_repository -cne 'HermanSchoenfeld/Sphere10.Framework' -or
            $result.framework_revision -cne $revision -or
            $result.visual_renderer_repository -cne 'HermanSchoenfeld/Sphere10.VisualRenderer' -or
            $result.visual_renderer_revision -cne $rendererRevision -or $result.sha -cne $head -or
            $result.version -cne '1.7.0' -or $result.build_number -ne 17 -or $result.publish -cne 'false' -or $calls.Count -ne 1) {
            throw "Dependency source or release identity was lost for '$Scenario'."
        }
        $outputs = @(Get-Content -LiteralPath $outputPath)
        if ('framework_repository=HermanSchoenfeld/Sphere10.Framework' -cnotin $outputs -or
            "framework_revision=$revision" -cnotin $outputs -or
            'visual_renderer_repository=HermanSchoenfeld/Sphere10.VisualRenderer' -cnotin $outputs -or
            "visual_renderer_revision=$rendererRevision" -cnotin $outputs -or "sha=$head" -cnotin $outputs) {
            throw "Dependency pins for '$Scenario' were not written to the workflow outputs."
        }
    } else {
        $failurePattern = if ($VisualRenderer) { 'VisualRenderer|visual-renderer-source' } else { 'Framework|framework-source' }
        if ($null -eq $failure -or $failure -notmatch $failurePattern -or $calls.Count -ne 0 -or (Test-Path -LiteralPath $outputPath)) {
            throw "Invalid source pin '$Scenario' was accepted or produced release metadata."
        }
    }
    Write-Host "Passed source pin metadata: $Scenario."
}

try {
    $null = New-Item -ItemType Directory -Path $fixtureBuild -Force
    Copy-Item -LiteralPath (Join-Path $repoRoot 'build/ci-metadata.ps1'), (Join-Path $repoRoot 'build/platforms.json') -Destination $fixtureBuild
    [IO.File]::WriteAllText((Join-Path $testDirectory 'Version.props'), '<Project><PropertyGroup><ReleaseVersion>1.7.0</ReleaseVersion></PropertyGroup></Project>', [Text.UTF8Encoding]::new($false))
    $pin = "HermanSchoenfeld/Sphere10.Framework $revision"
    Test-SourcePin -Scenario 'valid-lf' -Pin "$pin`n" -Valid $true
    Test-SourcePin -Scenario 'valid-crlf' -Pin "$pin`r`n" -Valid $true
    Test-SourcePin -Scenario 'valid-no-newline' -Pin $pin -Valid $true
    Test-SourcePin -Scenario 'different-repository' -Pin "Other/Framework $revision`n" -Valid $false
    Test-SourcePin -Scenario 'uppercase-revision' -Pin "HermanSchoenfeld/Sphere10.Framework $($revision.ToUpperInvariant())`n" -Valid $false
    Test-SourcePin -Scenario 'short-revision' -Pin "HermanSchoenfeld/Sphere10.Framework abcdef1`n" -Valid $false
    Test-SourcePin -Scenario 'pending-revision' -Pin "HermanSchoenfeld/Sphere10.Framework $('0' * 40)`n" -Valid $false
    Test-SourcePin -Scenario 'extra-token' -Pin "$pin extra`n" -Valid $false
    Test-SourcePin -Scenario 'extra-line' -Pin "$pin`nframework_revision=unexpected`n" -Valid $false
    Test-SourcePin -Scenario 'leading-whitespace' -Pin " $pin`n" -Valid $false

    $rendererPin = "HermanSchoenfeld/Sphere10.VisualRenderer $rendererRevision"
    Test-SourcePin -Scenario 'renderer-valid-lf' -Pin "$rendererPin`n" -Valid $true -VisualRenderer
    Test-SourcePin -Scenario 'renderer-valid-crlf' -Pin "$rendererPin`r`n" -Valid $true -VisualRenderer
    Test-SourcePin -Scenario 'renderer-valid-no-newline' -Pin $rendererPin -Valid $true -VisualRenderer
    Test-SourcePin -Scenario 'renderer-different-repository' -Pin "Other/VisualRenderer $rendererRevision`n" -Valid $false -VisualRenderer
    Test-SourcePin -Scenario 'renderer-uppercase-revision' -Pin "HermanSchoenfeld/Sphere10.VisualRenderer $($rendererRevision.ToUpperInvariant())`n" -Valid $false -VisualRenderer
    Test-SourcePin -Scenario 'renderer-short-revision' -Pin "HermanSchoenfeld/Sphere10.VisualRenderer abcdef1`n" -Valid $false -VisualRenderer
    Test-SourcePin -Scenario 'renderer-pending-revision' -Pin "HermanSchoenfeld/Sphere10.VisualRenderer $('0' * 40)`n" -Valid $false -VisualRenderer
    Test-SourcePin -Scenario 'renderer-extra-token' -Pin "$rendererPin extra`n" -Valid $false -VisualRenderer
    Test-SourcePin -Scenario 'renderer-extra-line' -Pin "$rendererPin`nvisual_renderer_revision=unexpected`n" -Valid $false -VisualRenderer
    Test-SourcePin -Scenario 'renderer-leading-whitespace' -Pin " $rendererPin`n" -Valid $false -VisualRenderer
} finally {
    $env:GITHUB_OUTPUT = $previousOutput
    if (Test-Path -LiteralPath $testDirectory) {
        $resolvedRoot = (Resolve-Path -LiteralPath $testRoot).ProviderPath.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
        $resolvedTarget = (Resolve-Path -LiteralPath $testDirectory).ProviderPath
        $comparison = if ($IsWindows) { [StringComparison]::OrdinalIgnoreCase } else { [StringComparison]::Ordinal }
        if (-not $resolvedTarget.StartsWith($resolvedRoot, $comparison) -or
            ((Get-Item -LiteralPath $resolvedTarget).Attributes -band [IO.FileAttributes]::ReparsePoint)) {
            throw "Refusing to clean a test directory outside '$resolvedRoot': $resolvedTarget"
        }
        Remove-Item -LiteralPath $resolvedTarget -Recurse -Force
    }
}
