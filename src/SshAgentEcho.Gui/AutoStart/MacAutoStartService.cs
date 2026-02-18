using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SshAgentEcho.Autostart {
    public class MacAutoStartService : IAutostartService {
        private readonly string _launchAgentsDirectory;
        private readonly string _appName;

        public MacAutoStartService(string appName) {
            _appName = appName;
            var home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            _launchAgentsDirectory = Path.Combine(home, "Library", "LaunchAgents");
        }

        private string PlistFilePath(string appName) => Path.Combine(_launchAgentsDirectory, $"com.{appName}.plist");

        public bool Install() {
            try {
                Directory.CreateDirectory(_launchAgentsDirectory);

                var exec = ((IAutostartService)this).GetExecutablePath();
                var plist = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
                                <!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
                                <plist version=""1.0"">
                                <dict>
                                    <key>Label</key>
                                    <string>com.{_appName}</string>
                                    <key>ProgramArguments</key>
                                    <array>
                                        <string>{exec}</string>
                                    </array>
                                    <key>RunAtLoad</key>
                                    <true/>
                                    <key>KeepAlive</key>
                                    <false/>
                                </dict>
                                </plist>";

                File.WriteAllText(PlistFilePath(_appName), plist);

                // load the launch agent (best-effort; swallow failures)
                RunLaunchctl($"load {PlistFilePath(_appName)}");

                return true;
            } catch {
                return false;
            }
        }

        public bool Uninstall() {
            try {
                var path = PlistFilePath(_appName);

                // try to unload the agent (best-effort)
                if (File.Exists(path)) {
                    RunLaunchctl($"unload {path}");
                    File.Delete(path);
                }

                return true;
            } catch {
                return false;
            }
        }

        public bool IsInstalled() {
            try {
                var path = PlistFilePath(_appName);
                if (!File.Exists(path)) return false;

                var content = File.ReadAllText(path);
                var exec = ((IAutostartService)this).GetExecutablePath();
                return content.Contains($"<string>{exec}</string>");
            } catch {
                return false;
            }
        }

        private static void RunLaunchctl(string args) {
            try {
                using var p = Process.Start(new ProcessStartInfo("launchctl", args) {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                p?.WaitForExit(2000);
            } catch {
                // ignore - launchctl may not be available in all environments
            }
        }
    }
}
