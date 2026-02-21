using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SshAgentEcho.Autostart {
    public class LinuxAutoStartService : IAutostartService {
        private readonly string _unitDirectory;
        private readonly string _appName;

        // factory passes appName in codebase; keep constructor to match that usage even though methods take appName.
        public LinuxAutoStartService(string appName) {
            _appName = appName;
            var home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            _unitDirectory = Path.Combine(home, ".config", "systemd", "user");
        }

        private string UnitFilePath(string appName) => Path.Combine(_unitDirectory, $"{appName}.service");

        public bool Install() {
            try {
                Directory.CreateDirectory(_unitDirectory);

                var exec = ((IAutostartService)this).GetExecutablePath();
                var quotedExec = exec.Contains(" ") ? $"\"{exec}\"" : exec;

                var unit = $"[Unit]\nDescription={_appName}\n\n[Service]\nType=simple\nExecStart={quotedExec}\nRestart=no\n\n[Install]\nWantedBy=default.target\n";

                File.WriteAllText(UnitFilePath(_appName), unit);

                // reload and enable the user unit (best-effort; swallow failures)
                RunSystemctl("--user daemon-reload");
                RunSystemctl($"--user enable {_appName}.service");

                return true;
            } catch {
                return false;
            }
        }

        public bool Uninstall() {
            try {
                // try to disable/remove the unit (best-effort)
                RunSystemctl($"--user disable --now {_appName}.service");

                var path = UnitFilePath(_appName);
                if (File.Exists(path)) File.Delete(path);

                RunSystemctl("--user daemon-reload");
                return true;
            } catch {
                return false;
            }
        }

        public bool IsInstalled() {
            try {
                var path = UnitFilePath(_appName);
                if (!File.Exists(path)) return false;

                var content = File.ReadAllText(path);
                var exec = ((IAutostartService)this).GetExecutablePath();
                return content.Contains($"ExecStart={exec}") || content.Contains($"ExecStart=\"{exec}\"");
            } catch {
                return false;
            }
        }

        private static void RunSystemctl(string args) {
            try {
                using var p = Process.Start(new ProcessStartInfo("systemctl", args) {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                p?.WaitForExit(2000);
            } catch {
                // ignore - systemctl may not be available in all environments
            }
        }
    }
}
