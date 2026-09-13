#requires -Version 5.1
# Run from the repository: .\deploy-locally.ps1
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
if ($env:OS -ne 'Windows_NT') { throw 'This script deploys the native Windows application.' }
$installDirectory = Join-Path $env:ProgramFiles 'Local Notion'
$logPath = Join-Path $PSScriptRoot 'publish\deploy-locally.log'
$principal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    $shellPath = (Get-Process -Id $PID).Path
    $child = Start-Process -FilePath $shellPath -Verb RunAs -WindowStyle Hidden -PassThru -ArgumentList @('-NoProfile', '-File', ('"' + $PSCommandPath + '"'))
    $child.WaitForExit()
    if ($child.ExitCode -ne 0) { throw "Local deployment failed. See $logPath" }
    Write-Host "Deployed to $installDirectory"
    & (Join-Path $installDirectory 'localnotion.exe') --version
    if ($LASTEXITCODE -ne 0) { throw 'The installed executable did not start.' }
    return
}

# A fresh output folder prevents old publish files from being copied into the installation.
$publishDirectory = Join-Path $PSScriptRoot ('publish\local\' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $publishDirectory -Force
try {
    Push-Location -LiteralPath $PSScriptRoot
    try {
        & dotnet publish LocalNotion.CLI/LocalNotion.CLI.csproj -c Release -r win-x64 --self-contained true -o $publishDirectory '-p:PublishSingleFile=true' '-p:PublishTrimmed=false' '-p:PublishReadyToRun=false' '-p:DebugSymbols=false' '-p:DebugType=None' *> $logPath
        if ($LASTEXITCODE -ne 0) { throw "Build failed. See $logPath" }
    } finally {
        Pop-Location
    }

    $null = New-Item -ItemType Directory -Path $installDirectory -Force
    $installedExe = Join-Path $installDirectory 'localnotion.exe'
    if (Test-Path -LiteralPath $installedExe) {
        # Renaming the old executable also works while an existing command is still running.
        $previousExe = Join-Path $installDirectory ('localnotion.previous-' + [guid]::NewGuid().ToString('N') + '.exe')
        $installRoot = [IO.Path]::GetFullPath($installDirectory).TrimEnd('\') + '\'
        if (-not [IO.Path]::GetFullPath($previousExe).StartsWith($installRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'Invalid backup path.' }
        Move-Item -LiteralPath $installedExe -Destination $previousExe
    }
    try {
        Get-ChildItem -LiteralPath $publishDirectory | Copy-Item -Destination $installDirectory -Recurse -Force
    } catch {
        if ($previousExe -and (Test-Path -LiteralPath $previousExe)) {
            if (Test-Path -LiteralPath $installedExe) { Remove-Item -LiteralPath $installedExe -Force }
            Move-Item -LiteralPath $previousExe -Destination $installedExe
        }
        throw
    }
    $expectedHash = (Get-FileHash -LiteralPath (Join-Path $publishDirectory 'localnotion.exe') -Algorithm SHA256).Hash
    if ((Get-FileHash -LiteralPath $installedExe -Algorithm SHA256).Hash -ne $expectedHash) { throw 'Installed executable does not match the build.' }

    $manifestPath = Join-Path $installDirectory 'localnotion-install.json'
    if (Test-Path -LiteralPath $manifestPath) {
        $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
        [xml]$versionProps = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Version.props') -Raw
        $manifest.version = $versionProps.Project.PropertyGroup.ReleaseVersion
        $manifest.installedAt = [DateTime]::UtcNow.ToString('o')
        $manifest.executableSha256 = $expectedHash
        $manifest.sourceHead = (& git -C $PSScriptRoot rev-parse HEAD | Out-String).Trim()
        $manifest.workingTree = if (@(& git -C $PSScriptRoot status --porcelain).Count) { 'uncommitted' } else { 'clean' }
        [IO.File]::WriteAllText($manifestPath, ($manifest | ConvertTo-Json -Depth 10), (New-Object Text.UTF8Encoding($false)))
    }
    Write-Host "Deployed to $installDirectory"
} catch {
    $_ | Out-String | Add-Content -LiteralPath $logPath
    Write-Error $_
    exit 1
}