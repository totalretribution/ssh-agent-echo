using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

using SshAgentEcho.Core;
using SshAgentEcho.Gui.ViewModels;

namespace SshAgentEcho.Gui;

public partial class App : Application {
    private SettingsWindow? _settingsWindow;
    private readonly SyncService _syncService = new();

    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {
        // Start without showing a main window so the app runs in the tray only.
        // Windows (like Settings) will be created on demand when the user opens them.
        base.OnFrameworkInitializationCompleted();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            Console.WriteLine("Application started");
            desktop.Exit += OnExit;
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var trayViewModel = new TrayViewModel(_syncService); // Pass the SyncService to the ViewModel.
            DataContext = trayViewModel;

            _syncService.Start();

#if DEBUG
            // For debugging, open the settings window immediately
            _settingsWindow = new SettingsWindow(_syncService);
            _settingsWindow.Closed += (_, _) => {
                _settingsWindow = null;
            };
            _settingsWindow.Show();
#endif

        }
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e) {
        try {
            _syncService.Dispose();
        } catch (Exception ex) {
            Console.WriteLine($"Error during OnExit disposing SyncService: {ex}");
        }
        Console.WriteLine("Application is exiting");
    }

    private void Settings_Click(object? sender, EventArgs e) {
        if (_settingsWindow?.IsVisible == true) {
            _settingsWindow.Activate();
            return;
        }

        _settingsWindow = new SettingsWindow(_syncService);
        _settingsWindow.Closed += (_, _) => {
            _settingsWindow = null;
        };
        _settingsWindow.Show();
    }

    private void Exit_Click(object? sender, EventArgs e) {
        Console.WriteLine("Exit_Click called");
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }
}