using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SshAgentEcho.Gui.Notification {
    public interface INotification {
        public void Notify(string title, string message);
        public void Notify(string title, string message, string uri_text, string uri);
        public Task NotifyAsync(string title, string message);
        public Task NotifyAsync(string title, string message, string uri_text, string uri);
    }
}