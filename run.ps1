<#
.SYNOPSIS
Run the CLI or GUI project for development.

.DESCRIPTION
Runs the requested project using `dotnet run`. Accepts `-Cli` or `-Gui`, or a positional `cli|gui` argument. Any remaining arguments are forwarded to the application.

.EXAMPLE
.\run.ps1 -Cli -- -v
.\run.ps1 gui
#>

[CmdletBinding()]
Param(
    [Alias('c')][switch]$Cli,
    [Alias('g')][switch]$Gui,
    [Alias('h')][switch]$Help,
    [Parameter(Position = 0)]
    [ValidateSet('cli','gui')]
    [string]$Target,
    [Parameter(ValueFromRemainingArguments=$true)]
    [string[]]$Args,
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'

# Ensure the console uses UTF-8 to avoid garbled Unicode output from `dotnet run` (e.g., box-drawing characters).
try {
    [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
    [Console]::InputEncoding  = [System.Text.Encoding]::UTF8
    $OutputEncoding = [System.Text.Encoding]::UTF8
} catch {
    Write-Host "⚠️ Could not set console encoding to UTF-8: $_" -ForegroundColor Yellow
}

function Show-Usage {
    @"
Usage: .\run.ps1 [OPTION] [-- <app-args>]

Run the project. When no target is selected the script defaults to the CLI project.

Options:
  -Cli, -c        Run the CLI project (default)
  -Gui, -g        Run the GUI project
  cli|gui         Positional equivalent to the switches above
  -Help, -h       Show this help message

Examples:
  .\run.ps1 -Cli -- -v        # run CLI with '-v'
  .\run.ps1 gui               # run GUI
"@
}

if ($Help) {
    Show-Usage
    exit 0
}

if (-not $Cli -and -not $Gui -and $null -ne $Target) {
    switch ($Target) {
        'cli'  { $Cli = $true }
        'gui'  { $Gui = $true }
    }
}

# If nothing selected, default to CLI
if (-not $Cli -and -not $Gui) {
    Write-Host "ℹ️ No target selected; defaulting to CLI." -ForegroundColor Yellow
    $Cli = $true
}

$projectRoot = $PSScriptRoot
$cliProject = Join-Path $projectRoot 'src/SshAgentEcho.Cli/SshAgentEcho.Cli.csproj'
$guiProject = Join-Path $projectRoot 'src/SshAgentEcho.Gui/SshAgentEcho.Gui.csproj'

function Run-Project {
    param(
        [string]$ProjectPath,
        [bool]$Wait = $true
    )

    if ($Wait) {
        Write-Host "🔧 Building project: $ProjectPath"
        & dotnet build $ProjectPath -c $Configuration | Out-Host

        # Try to locate the built DLL and run it via 'dotnet <dll>' for deterministic execution.
        $projectDir = Split-Path $ProjectPath -Parent
        $projName = [System.IO.Path]::GetFileNameWithoutExtension($ProjectPath)
        $buildDir = Join-Path $projectDir "bin\$Configuration"

        $dll = $null
        if (Test-Path $buildDir) {
            $dll = Get-ChildItem -Path $buildDir -Recurse -Filter "$projName.dll" -File -ErrorAction SilentlyContinue | Select-Object -First 1
        }

        if ($dll) {
            Write-Host "🔧 Running DLL with dotnet: $($dll.FullName)"
            $exeArgs = @($dll.FullName)
            if ($Args -and $Args.Length -gt 0) { $exeArgs += '--'; $exeArgs += $Args }
            & dotnet @exeArgs | Out-Host
            return $LASTEXITCODE
        }

        # Fallback to 'dotnet run' if DLL not found
        Write-Host "🔧 Running project (fallback): $ProjectPath"
        $argList = @('run','--project',$ProjectPath,'-c',$Configuration)
        if ($Args -and $Args.Length -gt 0) { $argList += '--'; $argList += $Args }
        & dotnet @argList | Out-Host
        return $LASTEXITCODE
    }
    else {
        # Background run: attempt to run the built DLL if available, otherwise start 'dotnet run'
        $projectDir = Split-Path $ProjectPath -Parent
        $projName = [System.IO.Path]::GetFileNameWithoutExtension($ProjectPath)
        $buildDir = Join-Path $projectDir "bin\$Configuration"

        $dll = $null
        if (Test-Path $buildDir) {
            $dll = Get-ChildItem -Path $buildDir -Recurse -Filter "$projName.dll" -File -ErrorAction SilentlyContinue | Select-Object -First 1
        }

        if ($dll) {
            $argList = @($dll.FullName)
            if ($Args -and $Args.Length -gt 0) { $argList += '--'; $argList += $Args }
            Write-Host "🔧 Starting DLL in background: $($dll.FullName)"
            Start-Process -FilePath dotnet -ArgumentList $argList -NoNewWindow -PassThru | Out-Null
            return 0
        }

        $argList = @('run','--project',$ProjectPath,'-c',$Configuration)
        if ($Args -and $Args.Length -gt 0) { $argList += '--'; $argList += $Args }
        Write-Host "🔧 Starting project in background: $ProjectPath"
        Start-Process -FilePath dotnet -ArgumentList $argList -NoNewWindow -PassThru | Out-Null
        return 0
    }
}

$exitCode = 0

if ($Cli) {
    $rc = Run-Project -ProjectPath $cliProject -Wait $true
    if ($rc -ne 0) { Write-Host "❌ CLI exited with code $rc" -ForegroundColor Red; $exitCode = $rc }
}

if ($Gui) {
    $rc = Run-Project -ProjectPath $guiProject -Wait $true
    if ($rc -ne 0) { Write-Host "❌ GUI exited with code $rc" -ForegroundColor Red; $exitCode = $rc }
}

exit $exitCode
