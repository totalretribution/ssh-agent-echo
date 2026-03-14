using System;
using System.Diagnostics;
using System.IO;

namespace SshAgentEcho.Gui.Autostart {
    public interface IAutostartService {
        bool Install();
        bool Uninstall();
        bool IsInstalled();

        // Default implementation: returns best-effort path to the running executable.
        public string GetExecutablePath() {
            try {
                if (!string.IsNullOrEmpty(Environment.ProcessPath)) return Environment.ProcessPath;
            } catch { }
            try {
                var module = Process.GetCurrentProcess().MainModule;
                if (module?.FileName != null) return module.FileName;
            } catch { }
            return Path.Combine(AppContext.BaseDirectory, AppDomain.CurrentDomain.FriendlyName);
        }
    }
}