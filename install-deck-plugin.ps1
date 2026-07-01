#Requires -Version 5.1
<#
.SYNOPSIS
  Build (optional) and install RGBJunkie-Deck into Elgato Stream Deck.

.DESCRIPTION
  Stops Stream Deck and the plugin host, copies com.rgbjunkie.deck.sdPlugin to
  %APPDATA%\Elgato\StreamDeck\Plugins\, then restarts Stream Deck.

.PARAMETER SkipBuild
  Copy the last Release build without running dotnet build.

.PARAMETER Configuration
  MSBuild configuration (default Release).

.PARAMETER NoRestart
  Install only — do not start Stream Deck afterward.

.PARAMETER StreamDeckExe
  Path to StreamDeck.exe. Defaults to Program Files, then $env:RGBJUNKIE_STREAMDECK_EXE.

.EXAMPLE
  .\install-deck-plugin.ps1

.EXAMPLE
  .\install-deck-plugin.ps1 -SkipBuild -NoRestart
#>
[CmdletBinding()]
param(
    [switch] $SkipBuild,
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
    [switch] $NoRestart,
    [string] $StreamDeckExe = ''
)

$ErrorActionPreference = 'Stop'

$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = Join-Path $RepoRoot 'RGBJunkieDeckPlugin'
$PluginFolderName = 'com.rgbjunkie.deck.sdPlugin'
$BundledPlugin = Join-Path $RepoRoot $PluginFolderName
$BuildOut = Join-Path $ProjectDir "bin\$Configuration\$PluginFolderName"
$ProjectFile = Join-Path $ProjectDir 'RGBJunkieDeckPlugin.csproj'
$HasSourceTree = Test-Path -LiteralPath $ProjectFile
$HasBundledPlugin = Test-Path -LiteralPath (Join-Path $BundledPlugin 'manifest.json')
$PluginsRoot = Join-Path $env:APPDATA 'Elgato\StreamDeck\Plugins'
$InstallDest = Join-Path $PluginsRoot $PluginFolderName

function Resolve-StreamDeckExe {
    param([string] $Override)
    if ($Override -and (Test-Path -LiteralPath $Override)) {
        return (Resolve-Path -LiteralPath $Override).Path
    }
    if ($env:RGBJUNKIE_STREAMDECK_EXE -and (Test-Path -LiteralPath $env:RGBJUNKIE_STREAMDECK_EXE)) {
        return (Resolve-Path -LiteralPath $env:RGBJUNKIE_STREAMDECK_EXE).Path
    }
    $candidates = @(
        "${env:ProgramFiles}\Elgato\StreamDeck\StreamDeck.exe",
        "${env:ProgramFiles(x86)}\Elgato\StreamDeck\StreamDeck.exe"
    )
    foreach ($c in $candidates) {
        if ($c -and (Test-Path -LiteralPath $c)) {
            return (Resolve-Path -LiteralPath $c).Path
        }
    }
    return $null
}

function Stop-RgbJunkieDeckProcesses {
    $names = @('StreamDeck', 'com.rgbjunkie.deck')
    foreach ($name in $names) {
        Get-Process -Name $name -ErrorAction SilentlyContinue | ForEach-Object {
            Write-Host "Stopping $($_.ProcessName) (PID $($_.Id))..."
            Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
        }
    }
    Start-Sleep -Seconds 2
}

Write-Host "RGBJunkie-Deck install"
Write-Host "  Project:  $ProjectDir"
Write-Host "  Install:  $InstallDest"
Write-Host ""

if (-not $SkipBuild -and $HasSourceTree) {
    Write-Host "Building ($Configuration)..."
    Push-Location $ProjectDir
    try {
        & dotnet build -c $Configuration
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet build failed with exit code $LASTEXITCODE"
        }
    }
    finally {
        Pop-Location
    }
}
elseif ($HasBundledPlugin) {
    Write-Host "Using bundled plugin folder (release install)."
    $BuildOut = $BundledPlugin
}
else {
    Write-Host "Skipping build (-SkipBuild)."
}

if (-not (Test-Path -LiteralPath $BuildOut)) {
    throw "Build output not found: $BuildOut`nRun without -SkipBuild or build manually first."
}

$manifestPath = Join-Path $BuildOut 'manifest.json'
if (Test-Path -LiteralPath $manifestPath) {
    try {
        $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
        Write-Host "Plugin version: $($manifest.Version)"
    }
    catch {
        Write-Warning "Could not read version from manifest.json"
    }
}

Stop-RgbJunkieDeckProcesses

if (Test-Path -LiteralPath $InstallDest) {
    Write-Host "Removing previous install..."
    Remove-Item -LiteralPath $InstallDest -Recurse -Force
}

Write-Host "Copying plugin files..."
New-Item -ItemType Directory -Path $PluginsRoot -Force | Out-Null
Copy-Item -LiteralPath $BuildOut -Destination $InstallDest -Recurse -Force

Write-Host "Installed to $InstallDest"

if ($NoRestart) {
    Write-Host "Done. Start Stream Deck manually when ready."
    exit 0
}

$sdExe = Resolve-StreamDeckExe -Override $StreamDeckExe
if (-not $sdExe) {
    Write-Warning "StreamDeck.exe not found. Set -StreamDeckExe or `$env:RGBJUNKIE_STREAMDECK_EXE, then start Stream Deck manually."
    exit 0
}

Write-Host "Starting Stream Deck: $sdExe"
Start-Process -FilePath $sdExe
Write-Host "Done."
