using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Threading;

namespace SshAgentEcho.Gui.Services {
    public class Notifications {
        public void Show(string message) {
            // For simplicity, we're just logging the notification. In a real application, you would show a UI notification.
            Log.Info($"Notification: {message}");
        }

    }
}