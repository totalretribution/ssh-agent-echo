using System.Diagnostics;

public static class Log {
    public static readonly TraceSource CoreSource =
        new TraceSource("SshAgentEcho.Core", SourceLevels.All);

    public static void Info(object? message) {
        CoreSource.TraceEvent(TraceEventType.Information, 0, message?.ToString() ?? "null");
        CoreSource.Flush();
    }

    public static void Warning(object? message) {
        CoreSource.TraceEvent(TraceEventType.Warning, 0, message?.ToString() ?? "null");
        CoreSource.Flush();
    }

    public static void Error(object? message) {
        CoreSource.TraceEvent(TraceEventType.Error, 0, message?.ToString() ?? "null");
        CoreSource.Flush();
    }
}

public class CleanStringListener : ConsoleTraceListener {
    public override void TraceEvent(TraceEventCache? eventCache, string source,
                                   TraceEventType eventType, int id, string? message) {
        if (message != null) WriteLine(message);
    }

    public override void TraceEvent(TraceEventCache? eventCache, string source,
                                   TraceEventType eventType, int id, string? format, params object?[]? args) {
        if (format == null) return;

        if (args != null && args.Length > 0) {
            WriteLine(string.Format(format, args));
        } else {
            WriteLine(format);
        }
    }
}
public class TimeStampedStringListener : ConsoleTraceListener {
    public override void TraceEvent(TraceEventCache? eventCache, string source,
                                   TraceEventType eventType, int id, string? message) {
        if (message != null) WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
    }

    public override void TraceEvent(TraceEventCache? eventCache, string source,
                                   TraceEventType eventType, int id, string? format, params object?[]? args) {
        if (format == null) return;

        if (args != null && args.Length > 0) {
            WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {string.Format(format, args)}");
        } else {
            WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {format}");
        }
    }
}