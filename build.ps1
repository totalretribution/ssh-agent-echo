<#
.SYNOPSIS
Build and publish the project for Linux and/or Windows.

.DESCRIPTION
This script publishes self-contained single-file releases for linux-x64 and win-x64.
If no option is provided, both targets are built.

.EXAMPLE
.\build.ps1 -Linux
.\build.ps1 windows
.\build.ps1  # builds both
#>

[CmdletBinding()]
Param(
    [Alias('l')][switch]$Linux,
    [Alias('w')][switch]$Windows,
    [Alias('h')][switch]$Help,
    [Parameter(Position = 0)]
    [ValidateSet('linux', 'windows')]
    [string]$Target
)

$ErrorActionPreference = 'Stop'

function Show-Usage {
    @"
Usage: .\build.ps1 [OPTION]

Build and publish the project.

Options:
  -Linux, -l        Build Linux (linux-x64)
  -Windows, -w      Build Windows (win-x64)
  linux             Same as -Linux
  windows           Same as -Windows
  -Help, -h         Show this help message

If no option is provided, builds both Linux and Windows.
"@
}

function Build-Linux {
    Write-Host "🔧 Building Linux (linux-x64)..."
    & dotnet publish (Join-Path $PSScriptRoot 'src/SshAgentEcho.Cli/SshAgentEcho.Cli.csproj') -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
    & dotnet publish (Join-Path $PSScriptRoot 'src/SshAgentEcho.Gui/SshAgentEcho.Gui.csproj') -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true

    $destDir = Join-Path $PSScriptRoot ".publish/linux"
    # Use the published single-file executable from the `publish` folder
    $src = Join-Path $PSScriptRoot "src\SshAgentEcho.Cli\bin\Release\net10.0\linux-x64\publish\ssh-agent-echo"
    if (!(Test-Path $destDir)) { New-Item -ItemType Directory -Path $destDir | Out-Null }
    Copy-Item -Path $src -Destination $destDir -Force
    $src = Join-Path $PSScriptRoot "src\SshAgentEcho.Gui\bin\Release\net10.0\linux-x64\publish\ssh-agent-echo-gui"
    if (!(Test-Path $destDir)) { New-Item -ItemType Directory -Path $destDir | Out-Null }
    Copy-Item -Path $src -Destination $destDir -Force}

function Build-Windows {
    Write-Host "🔧 Building Windows (win-x64)..."
    & dotnet publish (Join-Path $PSScriptRoot 'src/SshAgentEcho.Cli/SshAgentEcho.Cli.csproj') -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
    & dotnet publish (Join-Path $PSScriptRoot 'src/SshAgentEcho.Gui/SshAgentEcho.Gui.csproj') -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

    $destDir = Join-Path $PSScriptRoot ".publish/windows"
    # Use the published single-file executable from the `publish` folder
    $src = Join-Path $PSScriptRoot "src\SshAgentEcho.Cli\bin\Release\net10.0\win-x64\publish\ssh-agent-echo.exe"
    if (!(Test-Path $destDir)) { New-Item -ItemType Directory -Path $destDir | Out-Null }
    Copy-Item -Path $src -Destination $destDir -Force
    $src = Join-Path $PSScriptRoot "src\SshAgentEcho.Gui\bin\Release\net10.0\win-x64\publish\ssh-agent-echo-gui.exe"
    if (!(Test-Path $destDir)) { New-Item -ItemType Directory -Path $destDir | Out-Null }
    Copy-Item -Path $src -Destination $destDir -Force
}

if ($Help) {
    Show-Usage
    exit 0
}

if (-not $Linux -and -not $Windows -and $null -ne $Target) {
    switch ($Target) {
        'linux' { $Linux = $true }
        'windows' { $Windows = $true }
    }
}

# If nothing selected, build both
if (-not $Linux -and -not $Windows) {
    $Linux = $true
    $Windows = $true
}

$built = $false
try {
    if ($Linux) { Build-Linux; $built = $true }
    if ($Windows) { Build-Windows; $built = $true }
}
catch {
    Write-Host "❌ Build failed: $_" -ForegroundColor Red
    exit 1
}

if (-not $built) {
    Write-Host "⚠️ No valid build target selected." -ForegroundColor Yellow
    Show-Usage
    exit 2
}

Write-Host "✅ Build completed." -ForegroundColor Green
exit 0
