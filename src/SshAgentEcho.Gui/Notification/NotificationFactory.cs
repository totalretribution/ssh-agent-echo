using System;
using System.Runtime.InteropServices;

namespace SshAgentEcho.Gui.Notification {
    public static class NotificationFactory {
        public static INotification Create() {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new WindowsNotification();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return new LinuxNotification();

            // if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            //     return new MacNotification();

            throw new PlatformNotSupportedException();
        }
    }
}
