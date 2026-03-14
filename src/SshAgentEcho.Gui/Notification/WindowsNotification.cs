using Microsoft.Toolkit.Uwp.Notifications; // NuGet package
using Windows.UI.Notifications; // Windows Runtime API
using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SshAgentEcho.Gui.Notification {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class WindowsNotification : INotification {

        public WindowsNotification() { }

        public void Notify(string title, string message) {
            try {
                CreateToast(title, message, null, null);
            } catch (Exception ex) {
                Log.Error($"Failed to show notification: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        public void Notify(string title, string message, string uri_text, string uri) {
            try {
                CreateToast(title, message, uri_text, uri);
            } catch (Exception ex) {
                Log.Error($"Failed to show notification: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        public async Task NotifyAsync(string title, string message) {
            CreateToast(title, message);
        }

        public async Task NotifyAsync(string title, string message, string uri_text, string uri) {
            CreateToast(title, message, uri_text, uri);
        }

        private void CreateToast(string title, string message, string? uri_text = null, string? uri = null) {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10240)) {
                return;
            }

            try {
                var toastContent = new ToastContentBuilder()
                    .AddText(title)
                    .AddText(message)
                    .AddButton(new ToastButton()
                    .SetContent(uri_text)
                    .SetProtocolActivation(new Uri(uri)))
                    .GetToastContent();

                var toast = new ToastNotification(toastContent.GetXml());

                ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);
            } catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException or COMException) {
                throw new Exception($"{ex.InnerException?.Message ?? ex.Message}", ex);
            }
        }
    }
}