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
}