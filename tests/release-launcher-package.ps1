#requires -Version 7.4

<#
.SYNOPSIS
Checks that the actual Windows Docker launcher bundle matches the publisher contract.
.DESCRIPTION
Packages the launcher sources without building or running Local Notion, Docker,
or a compiler. Reads the publisher's exhaustive payload allowlist and checks the
generated ZIP, release identity, and bundled icon. No external service is used.
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$testRoot = [IO.Path]::GetFullPath((Join-Path $repoRoot 'publish/.launcher-tests'))
$testDirectory = Join-Path $testRoot ([guid]::NewGuid().ToString('N'))
$version = '1.7.0'
$buildNumber = 17
$revision = '0123456789abcdef0123456789abcdef01234567'

$tokens = $null
$errors = $null
$ast = [Management.Automation.Language.Parser]::ParseFile(
    (Join-Path $repoRoot 'build/publish-github-release.ps1'), [ref]$tokens, [ref]$errors)
if ($errors.Count) { throw 'Cannot parse the release publisher.' }
$assignments = @($ast.FindAll({
    param($node)
    $node -is [Management.Automation.Language.AssignmentStatementAst] -and
    $node.Left -is [Management.Automation.Language.VariableExpressionAst] -and
    $node.Left.VariablePath.UserPath -ceq 'launcherFiles'
}, $true))
if ($assignments.Count -ne 1) { throw 'Expected one publisher launcher payload allowlist.' }
$allowedFiles = @(& ([scriptblock]::Create($assignments[0].Right.Extent.Text)))
if (-not $allowedFiles.Count -or @($allowedFiles | Select-Object -Unique).Count -ne $allowedFiles.Count) {
    throw 'The publisher launcher payload allowlist must contain unique file names.'
}

try {
    & (Join-Path $repoRoot 'build/package-docker-launcher.ps1') -Version $version -BuildNumber $buildNumber -SourceRevisionId $revision -OutputDirectory $testDirectory
    $archive = [IO.Compression.ZipFile]::OpenRead((Join-Path $testDirectory 'localnotion-docker-windows.zip'))
    try {
        $entries = @($archive.Entries | Where-Object { -not $_.FullName.EndsWith('/') })
        $names = @($entries.FullName)
        $missing = @($allowedFiles | Where-Object { $_ -cnotin $names })
        $unexpected = @($names | Where-Object { $_ -cnotin $allowedFiles })
        if ($names.Count -ne $allowedFiles.Count -or $missing.Count -or $unexpected.Count) {
            throw "The generated launcher ZIP does not match the publisher payload contract. Missing: $($missing -join ', '). Unexpected: $($unexpected -join ', ')."
        }

        $metadataEntry = $archive.GetEntry('release.json')
        $reader = [IO.StreamReader]::new($metadataEntry.Open())
        try { $metadata = $reader.ReadToEnd() | ConvertFrom-Json }
        finally { $reader.Dispose() }
        if ($metadata.version -cne $version -or $metadata.buildNumber -ne $buildNumber -or
            $metadata.commit -cne $revision -or $metadata.image -cne "ghcr.io/sphere10/local-notion:$version") {
            throw 'Launcher release metadata does not match the requested identity.'
        }

        $iconEntry = $archive.GetEntry('assets/branding/local-notion.ico')
        if ($null -eq $iconEntry) { throw 'The launcher ZIP is missing its application icon.' }
        $iconStream = $iconEntry.Open()
        try { $actualIconHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($iconStream)) }
        finally { $iconStream.Dispose() }
        $expectedIconHash = (Get-FileHash -LiteralPath (Join-Path $repoRoot 'assets/branding/local-notion.ico') -Algorithm SHA256).Hash
        if ($actualIconHash -cne $expectedIconHash) { throw 'The launcher ZIP icon differs from the product icon.' }
    } finally { $archive.Dispose() }
    Write-Host "Passed launcher package: $($allowedFiles.Count) publisher-compatible payload files, release identity, and exact product icon."
} finally {
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
