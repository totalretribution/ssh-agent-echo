using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System;

using SshAgentEcho.Core;

namespace SshAgentEcho.Gui;

public partial class App : Application {
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    private SettingsWindow? _settingsWindow;

    private void TrayIcon_Clicked(object? sender, EventArgs e) {
        // ShowWindow();
    }

    private void TrayIcon_DoubleClicked(object? sender, EventArgs e) {
        // ShowWindow();
    }

    private void Sync_Click(object? sender, EventArgs e) {
        var syncAgent = new SyncAgent();
        syncAgent.Sync();
    }

    private void Settings_Click(object? sender, EventArgs e) {
        if (_settingsWindow?.IsVisible == true) {
            _settingsWindow.Activate();
            return;
        }

        _settingsWindow = new SettingsWindow();
        _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        _settingsWindow.Show();
    }

    private void Exit_Click(object? sender, EventArgs e) {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }
}
