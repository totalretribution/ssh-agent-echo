using System.Diagnostics;
using Microsoft.Win32;

namespace SshAgentEcho.Autostart {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class WindowsAutoStartService : IAutostartService {
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private readonly string _appName;
        public WindowsAutoStartService(string appName) {
            _appName = appName;
        }

        public bool Install() {
            Debug.WriteLine("Attempting to install autostart registry key");
            try {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                if (key == null) return false;
                var executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                key.SetValue(_appName, $"\"{executablePath}\"");
                return true;
            } catch {
                return false;
            }
        }

        public bool Uninstall() {
            Debug.WriteLine("Attempting to uninstall autostart registry key");
            try {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                if (key == null) return false;
                key.DeleteValue(_appName, false);
                return true;
            } catch {
                return false;
            }
        }

        public bool IsInstalled() {
            try {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
                if (key == null) return false;
                var value = key.GetValue(_appName) as string;
                if (string.IsNullOrEmpty(value)) return false;
                var executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                return value.Trim('"') == executablePath;
            } catch {
                return false;
            }
        }
    }
}