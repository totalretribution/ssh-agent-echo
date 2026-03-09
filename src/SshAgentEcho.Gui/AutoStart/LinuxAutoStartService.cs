using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SshAgentEcho.Autostart {
    public class LinuxAutoStartService : IAutostartService {
        private readonly string _autostartDirectory;
        private readonly string _appName;

        // factory passes appName in codebase; keep constructor to match that usage even though methods take appName.
        public LinuxAutoStartService(string appName) {
            _appName = appName;
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            _autostartDirectory = Path.Combine(home, ".config", "autostart");
        }

        private string DesktopFilePath(string appName) => Path.Combine(_autostartDirectory, $"{appName}.desktop");

        public bool Install() {
            try {
                Directory.CreateDirectory(_autostartDirectory);

                var exec = ((IAutostartService)this).GetExecutablePath();
                var quotedExec = exec.Contains(" ") ? $"\"{exec}\"" : exec;

                var desktopFile = $@"
[Desktop Entry]
Type=Application
Name={_appName}
Exec=sleep 10 &&{quotedExec}
Hidden=false
NoDisplay=false
X-GNOME-Autostart-enabled=true";

                File.WriteAllText(DesktopFilePath(_appName), desktopFile);

                return true;
            } catch {
                return false;
            }
        }

        public bool Uninstall() {
            try {
                var path = DesktopFilePath(_appName);
                if (File.Exists(path)) File.Delete(path);
                return true;
            } catch {
                return false;
            }
        }

        public bool IsInstalled() {
            try {
                var path = DesktopFilePath(_appName);
                return File.Exists(path);
            } catch {
                return false;
            }
        }
    }
}
