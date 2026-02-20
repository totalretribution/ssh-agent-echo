using System.Diagnostics;

namespace SshAgentEcho.Gui.Services {

    public class DebugListener : TraceListener {
        public override void Write(string? message) { }

        public override void WriteLine(string? message) {
            if (message != null) {
                LogService.Instance.WriteLine(message);
            }
        }
    }

    public class CleanDebugListener : ConsoleTraceListener {
        public override void TraceEvent(TraceEventCache? eventCache, string source,
                                       TraceEventType eventType, int id, string? message) {
            if (message != null) LogService.Instance.WriteLine(message);
        }

        public override void TraceEvent(TraceEventCache? eventCache, string source,
                                       TraceEventType eventType, int id, string? format, params object?[]? args) {
            if (format == null) return;

            if (args != null && args.Length > 0) {
                LogService.Instance.WriteLine(string.Format(format, args));
            } else {
                LogService.Instance.WriteLine(format);
            }
        }
    }
}