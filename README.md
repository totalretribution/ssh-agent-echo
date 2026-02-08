# ssh-agent-echo

A tool to synchronize SSH keys from a running SSH agent to your SSH config file. This is useful when you are using an SSH agent that loads keys from a remote source, such as 1Password, and you want to use those keys with tools that read the SSH config file directly.

## The Problem

Some SSH servers have a limit on the number of authentication attempts (e.g., 6). If your SSH agent has more keys than this limit, the server may close the connection before the correct key is presented, leading to authentication failure. This tool solves this problem by creating specific `Host` entries in your SSH config file for each key, ensuring that only the correct key is used for each host.


## Features

-   Lists public keys available in the SSH agent, similar to `ssh-add -L`.
-   Syncs the keys from the SSH agent to your `~/.ssh/config` file.
-   Provides a command-line interface (`ssh-agent-echo`) for manual syncing.
-   Provides a graphical user interface (`ssh-agent-echo-gui`) that runs in the system tray for automatic syncing.

## Key Comment Format

For `ssh-agent-echo` to correctly identify and create `Host` entries, the comment associated with each SSH key in the agent must follow a specific format. The tool supports two formats for the key comment:

> [!NOTE]
> For Bitwarden users, the "name" of the SSH key is used as the comment.

### Simple `user@host`

The most basic format is `user@host`, with no spaces.

**Example:** `dev@githost.com`

This will generate a single `Host` entry in your SSH configuration:

```ssh-config
Host githost.com
    User dev
    IdentityFile /path/to/your/keys/githost_com.pub
    IdentitiesOnly yes
```

### Nickname `<user@host>`

You can also assign a nickname to a key, which is useful for creating aliases for hosts.

**Example:** `My-Server <dev@githost.com>`

This format will generate two `Host` entries: one for the nickname and one for the actual hostname. Any spaces in the nickname will be replaced with underscores.

```ssh-config
Host My-Server
    HostName githost.com
    User dev
    IdentityFile /path/to/your/keys/My-Server.pub
    IdentitiesOnly yes

Host githost.com
    User dev
    IdentityFile /path/to/your/keys/My-Server.pub
    IdentitiesOnly yes
```

## Binaries

This project provides two binaries:

### `ssh-agent-echo`

This is a command-line tool that allows you to manually sync your SSH keys.

**Usage:**

```bash
# Print keys from the agent
ssh-agent-echo --print

# Sync keys from the agent to the SSH config
ssh-agent-echo --sync

# Force sync even if keys haven't changed
ssh-agent-echo --sync --force
```

### `ssh-agent-echo-gui`

This is a graphical tool that runs in your system tray. It can be configured to automatically sync your keys in the background.

## Build

You can build the binaries using the .NET SDK:

```bash
# Build the whole solution (both CLI and GUI)
dotnet build ssh-agent-echo.slnx -c Release

# Or build them individually
# Build the CLI
dotnet build src/SshAgentEcho.Cli/SshAgentEcho.Cli.csproj -c Release

# Build the GUI
dotnet build src/SshAgentEcho.Gui/SshAgentEcho.Gui.csproj -c Release
```

To publish a self-contained single-file release (example for CLI):

```bash
# Linux (linux-x64)
dotnet publish src/SshAgentEcho.Cli/SshAgentEcho.Cli.csproj -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true -o publish/linux-x64

# Windows (win-x64)
dotnet publish src/SshAgentEcho.Cli/SshAgentEcho.Cli.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o publish/windows
```

Build outputs:
- Development builds (DLLs) are placed in `src/*/bin/Release/net10.0`.
- Published single-file outputs (self-contained) are placed in the `publish/<rid>` directories (for example `publish/linux-x64` or `publish/windows`).

## Dependencies

- .NET SDK (target framework: **net10.0**). Install the appropriate SDK from https://dotnet.microsoft.com/ (the project targets `net10.0`).

### Linux (GUI)

If you plan to build or run the GUI on Linux, you'll need common GUI/native development packages (Debian/Ubuntu example):

```bash
sudo apt-get install -y \
    pkg-config \
    libglib2.0-dev \
    libdbus-1-dev \
    libx11-dev \
    libxkbcommon-dev \
    libwayland-dev \
    libgtk-3-dev
```

These packages are used by Avalonia/native toolchains and may be required for the GUI to run or build correctly on Linux.

### Windows (GUI)

If you are building or debugging the GUI on Windows, having the Visual C++ build tools / Visual Studio Build Tools installed is recommended for native dependency support. You can install them via the Visual Studio Installer or with `winget` (example):

```powershell
winget install --id Microsoft.VisualStudio.2022.BuildTools --override "--add Microsoft.VisualStudio.Workload.VCTools --add Microsoft.VisualStudio.Component.VC.Tools.x86.x64 --add Microsoft.VisualStudio.Component.Windows11SDK.26100 --quiet --wait --norestart --nocache"
```

If you produce self-contained publishes, those artifacts include the runtime and typically don't require the SDK/runtime to be installed on the target machine.


## See Also

- [Using Bitwarden's SSH Agent with WSL](https://blog.jkwmoore.dev/bitwarden-desktop-client-as-ssh-agent-with-wsl.html) - A guide on how to link Bitwarden's SSH agent to WSL, which can be used in conjunction with this tool.

## License

This project is licensed under the terms of the LICENSE file.
