#Requires -Version 5.1
<#
.SYNOPSIS
  Build Release and zip the plugin + install scripts for GitHub Releases.
#>
[CmdletBinding()]
param(
    [string] $Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

$RepoRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$ProjectDir = Join-Path $RepoRoot 'RGBJunkieDeckPlugin'
$PluginFolderName = 'com.rgbjunkie.deck.sdPlugin'
$BuildOut = Join-Path $ProjectDir "bin\$Configuration\$PluginFolderName"
$DistDir = Join-Path $RepoRoot 'dist'

Push-Location $ProjectDir
try {
    & dotnet build -c $Configuration
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed with exit code $LASTEXITCODE" }
}
finally {
    Pop-Location
}

if (-not (Test-Path -LiteralPath (Join-Path $BuildOut 'manifest.json'))) {
    throw "Build output missing: $BuildOut"
}

$manifest = Get-Content -LiteralPath (Join-Path $BuildOut 'manifest.json') -Raw | ConvertFrom-Json
$version = [string]$manifest.Version
if ([string]::IsNullOrWhiteSpace($version)) { throw 'manifest.json has no Version' }

$stageName = "RGBJunkie-Deck-$version-win"
$stageDir = Join-Path $DistDir $stageName
$zipPath = Join-Path $DistDir "$stageName.zip"

if (Test-Path -LiteralPath $stageDir) { Remove-Item -LiteralPath $stageDir -Recurse -Force }
New-Item -ItemType Directory -Path $stageDir -Force | Out-Null

Copy-Item -LiteralPath $BuildOut -Destination (Join-Path $stageDir $PluginFolderName) -Recurse -Force
Copy-Item -LiteralPath (Join-Path $RepoRoot 'install-deck-plugin.ps1') -Destination $stageDir
Copy-Item -LiteralPath (Join-Path $RepoRoot 'install-deck-plugin.bat') -Destination $stageDir

$installTxt = @"
RGBJunkie-Deck $version — Windows install
============================================

1. Quit Stream Deck (tray icon → Quit).
2. Double-click install-deck-plugin.bat
   (or run install-deck-plugin.ps1 in PowerShell).
3. Stream Deck restarts with the RGBJunkie plugin installed.

Requires RGBJunkie (rgbjunkie:// URL scheme) and Stream Deck 6.4+.
"@
Set-Content -LiteralPath (Join-Path $stageDir 'INSTALL.txt') -Value $installTxt -Encoding UTF8

if (Test-Path -LiteralPath $zipPath) { Remove-Item -LiteralPath $zipPath -Force }
Compress-Archive -LiteralPath $stageDir -DestinationPath $zipPath -Force

Write-Host "Packaged $zipPath (v$version)"
Write-Output $zipPath
