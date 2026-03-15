#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<EOF
Usage: $(basename "$0") [OPTION]

Build and publish the project.

Options:
  --linux, -l       Build Linux (linux-x64)
  --windows, -w     Build Windows (win-x64)
  linux             Same as --linux
  windows           Same as --windows
  --help, -h        Show this help message

If no option is provided, builds both Linux and Windows.
EOF
}

build_linux() {
  echo "🔧 Building Linux (linux-x64)..."
  dotnet publish -c Release -o publish -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true src/SshAgentEcho.Cli/SshAgentEcho.Cli.csproj
  dotnet publish -c Release -o publish -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true src/SshAgentEcho.Gui/SshAgentEcho.Gui.csproj
}

build_windows() {
  echo "🔧 Building Windows (win-x64)..."
  dotnet publish -c Release -o publish -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true src/SshAgentEcho.Cli/SshAgentEcho.Cli.csproj
  dotnet publish -c Release -o publish -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true src/SshAgentEcho.Gui/SshAgentEcho.Gui.csproj
}

if [ "$#" -eq 0 ]; then
  build_linux
  build_windows
  exit 0
fi

build_any=false
while [ "$#" -gt 0 ]; do
  case "$1" in
    -l|--linux|linux)
      build_linux
      build_any=true
      shift
      ;;
    -w|--windows|windows)
      build_windows
      build_any=true
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "⚠️ Unknown option: $1"
      usage
      exit 2
      ;;
  esac
done

if [ "$build_any" = false ]; then
  echo "⚠️ No valid build target selected."
  usage
  exit 2
fi

exit 0
