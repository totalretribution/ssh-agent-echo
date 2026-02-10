#!/usr/bin/env bash
set -euo pipefail

# Run the CLI or GUI project for development.
# Defaults to CLI when no target is selected.
# Behavior:
#  - Builds the project: dotnet build <proj> -c <Configuration>
#  - Prefers to run the built DLL via: dotnet <path-to-dll> -- <app-args>
#  - Falls back to: dotnet run --project <proj> -c <Configuration> -- <app-args>

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$SCRIPT_DIR"
CLI_PROJECT="$PROJECT_ROOT/src/SshAgentEcho.Cli/SshAgentEcho.Cli.csproj"
GUI_PROJECT="$PROJECT_ROOT/src/SshAgentEcho.Gui/SshAgentEcho.Gui.csproj"
CONFIGURATION="Debug"

usage() {
  cat <<'EOF'
Usage: ./run.sh [OPTION] [-- <app-args>]

Run the project. When no target is selected the script defaults to the CLI project.

Options:
  --cli           Run the CLI project (default)
  --gui           Run the GUI project
  -c, --config    Configuration (Debug|Release). Defaults to Debug
  -h, --help      Show this help message

Examples:
  ./run.sh --cli -- --print
  ./run.sh --gui
EOF
}

# Parse args (basic)
TARGET=""
POS_ARGS=()
while [[ $# -gt 0 ]]; do
  case "$1" in
    --cli)
      TARGET="cli"
      shift
      ;;
    --gui)
      TARGET="gui"
      shift
      ;;
    -c|--config)
      CONFIGURATION="${2:-}"
      if [[ -z "$CONFIGURATION" ]]; then
        echo "Error: missing value for $1" >&2
        exit 2
      fi
      shift 2
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    --)
      shift
      POS_ARGS+=("$@")
      break
      ;;
    *)
      # If the user provided a positional 'cli' or 'gui'
      if [[ -z "$TARGET" && ("$1" == "cli" || "$1" == "gui") ]]; then
        TARGET="$1"
        shift
      else
        POS_ARGS+=("$1")
        shift
      fi
      ;;
  esac
done

# If nothing selected, default to CLI
if [[ -z "$TARGET" ]]; then
  echo "ℹ️  No target selected; defaulting to CLI."
  TARGET="cli"
fi

run_project() {
  local project_path="$1"
  shift
  local args=("$@")

  echo "🔧 Building project: $project_path"
  dotnet build "$project_path" -c "$CONFIGURATION"

  # Try to locate the built DLL
  local project_dir
  project_dir=$(dirname "$project_path")
  local proj_name
  proj_name=$(basename "$project_path" .csproj)
  local search_dir="$project_dir/bin/$CONFIGURATION"

  local dll_path
  if [[ -d "$search_dir" ]]; then
    dll_path=$(find "$search_dir" -type f -name "$proj_name.dll" -print -quit || true)
  fi

  if [[ -n "$dll_path" ]]; then
    echo "🔧 Running DLL with dotnet: $dll_path"
    if [[ ${#args[@]} -gt 0 ]]; then
      dotnet "$dll_path" -- "${args[@]}"
    else
      dotnet "$dll_path"
    fi
    return $?
  fi

  # Fallback to dotnet run
  echo "🔧 Running project (fallback): $project_path"
  if [[ ${#args[@]} -gt 0 ]]; then
    dotnet run --project "$project_path" -c "$CONFIGURATION" -- "${args[@]}"
  else
    dotnet run --project "$project_path" -c "$CONFIGURATION"
  fi
  return $?
}

case "$TARGET" in
  cli)
    run_project "$CLI_PROJECT" "${POS_ARGS[@]:-}"
    ;;
  gui)
    run_project "$GUI_PROJECT" "${POS_ARGS[@]:-}"
    ;;
  *)
    echo "Unknown target: $TARGET" >&2
    exit 2
    ;;
esac
