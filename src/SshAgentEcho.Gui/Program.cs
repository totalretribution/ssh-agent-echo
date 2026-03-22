using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Threading;
using System.Threading.Tasks;

using SshAgentEcho.Core;

namespace SshAgentEcho.Gui;

public partial class App : Application {
    private static Mutex _mutex = new Mutex(true, "{7ec32d75-8085-425e-9a44-11dbba09deb5}");

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) {
        if (!_mutex.WaitOne(TimeSpan.Zero, true)) {
            return;
        }

        try {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        } catch (NullReferenceException ex) when (ex.StackTrace?.Contains("DBusMenu") == true || ex.StackTrace?.Contains("Tmds.DBus") == true) {
            Console.WriteLine($"Suppressing DBus shutdown error: {ex.Message}");
        } catch (Exception ex) {
            Console.WriteLine($"Unhandled exception: {ex}");
        } finally {
            _mutex.ReleaseMutex();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
