#if LINUX
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus;

namespace SshAgentEcho.Gui.Notification {
    public class LinuxNotification : INotification {

        [DBusInterface("org.freedesktop.Notifications")]
        public interface INotificationService : IDBusObject {
            Task<uint> NotifyAsync(
                string appName,
                uint replacesId,
                string appIcon,
                string summary,
                string body,
                string[] actions,
                IDictionary<string, object> hints,
                int expireTimeout);
        }

        public LinuxNotification() { }

        public void Notify(string title, string message) {
            Task.Run(() => CreateToast("ssh-agent-echo", "dialog-information", title, message));
        }

        public void Notify(string title, string message, string uri_text, string uri) {
            message += $"\n<a href=\"{uri}\">{uri_text}</a>";
            Task.Run(() => CreateToast("ssh-agent-echo", "dialog-information", title, message));
        }

        public async Task NotifyAsync(string title, string message) {
            try {
                await CreateToast("ssh-agent-echo", "dialog-information", title, message);
            } catch (Exception ex) {
                Log.Error($"Failed to send notification: {ex.Message}");
            }
        }

        public async Task NotifyAsync(string title, string message, string uri_text, string uri) {
            message += $"\n<a href=\"{uri}\">{uri_text}</a>";
            try {
                await CreateToast("ssh-agent-echo", "dialog-information", title, message);
            } catch (Exception ex) {
                Log.Error($"Failed to send notification: {ex.Message}");
            }
        }

        private async Task CreateToast(string appName, string appIcon, string summary, string body, int timeout = 5000) {
            try {
                // 1. Create a connection to the Session Bus
                using var connection = new Connection(Address.Session);
                await connection.ConnectAsync();

                // 2. Create a proxy for the notification service
                var proxy = connection.CreateProxy<INotificationService>(
                    "org.freedesktop.Notifications",
                    "/org/freedesktop/Notifications");

                var hints = new Dictionary<string, object> {
                    { "urgency", (byte)1 },        // normal
                    { "category", "message" },     // KDE treats this as message
                    { "desktop-entry", appName }   // optional
                };

                await proxy.NotifyAsync(appName, 0, appIcon, summary, body, Array.Empty<string>(), hints, timeout);

            } catch (Exception ex) {
                throw new Exception($"{ex.Message}", ex);
            }
        }
    }
}
#endif