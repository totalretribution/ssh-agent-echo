using System;
using System.Runtime.InteropServices;

namespace SshAgentEcho.Gui.Autostart {
    public static class AutostartServiceFactory {
        public static IAutostartService Create(string appName) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new WindowsAutoStartService(appName);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return new LinuxAutoStartService(appName);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return new MacAutoStartService(appName);

            throw new PlatformNotSupportedException();
        }
    }
}
