using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Threading;
using System.Threading;
using System.Threading.Tasks;
using SshAgentEcho.Gui.Notification;

using SshAgentEcho.Core;

namespace SshAgentEcho.Gui.Services {

    public class Updater {

        public void Run() {
            _ = Task.Run(async () => {
                Update update = new Update();
                try {
                    await update.Check();
                    if (update.IsNewVersionAvailable()) {
                        string? url = update.GetReleaseURL();
                        if (!string.IsNullOrEmpty(url)) {
                            string message = $"A new version of ssh-agent-echo is available: v{update.LatestVersion}";
                            INotification _notificationService = NotificationFactory.Create();
                            await _notificationService.NotifyAsync("ssh-agent-echo", message, "Download", url);
                            Log.Info($"New version available: {update.LatestVersion}");
                        }
                    } else {
                        Log.Info("No new version available.");
                    }
                } catch (Exception ex) {
                    Log.Error($"Error checking for updates: {ex.Message}");
                }
            });
        }
    }
}