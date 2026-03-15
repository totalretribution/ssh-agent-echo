using System;
using System.Runtime.InteropServices;

namespace SshAgentEcho.Gui.Notification {
    public static class NotificationFactory {
        public static INotification Create() {
#if WINDOWS
                return new WindowsNotification();
#elif LINUX
                return new LinuxNotification();
#elif MACOS
            //     return new MacNotification();
#else
            throw new PlatformNotSupportedException();
#endif

        }
    }
}
