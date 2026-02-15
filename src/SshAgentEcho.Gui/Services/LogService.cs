using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Threading;

namespace SshAgentEcho.Gui.Services {

    public class LogService {
        private static readonly Lazy<LogService> _instance = new(() => new LogService());
        public static LogService Instance => _instance.Value;

        public ObservableCollection<string> LogEntries { get; } = new();

        private LogService() { }

        public void WriteLine(string message) {
            string timestampedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
            Dispatcher.UIThread.InvokeAsync(() => LogEntries.Add(timestampedMessage));
        }
    }
}