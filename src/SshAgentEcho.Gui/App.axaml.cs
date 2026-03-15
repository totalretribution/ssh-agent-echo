using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

using SshAgentEcho.Core;
using SshAgentEcho.Gui.Services;
using SshAgentEcho.Gui.ViewModels;
using SshAgentEcho.Gui.Notification;

namespace SshAgentEcho.Gui;

public partial class App : Application {

    private SettingsWindow? _settingsWindow;
    private readonly SyncService _syncService = new();
    private TrayViewModel? _trayViewModel;
    private MainViewModel? _mainViewModel;

    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {
        Trace.Listeners.Clear();
        Trace.Listeners.Add(new DefaultTraceListener()); // Optional: keep default listener for Debug output

        Log.CoreSource.Listeners.Clear();
        Log.CoreSource.Listeners.Add(new CleanStringListener());
        Log.CoreSource.Listeners.Add(new CleanDebugListener()); // Custom listener writes to LogService
        Log.CoreSource.Switch = new SourceSwitch("coreSwitch", "All");

        // Start without showing a main window so the app runs in the tray only.
        // Windows (like Settings) will be created on demand when the user opens them.
        base.OnFrameworkInitializationCompleted();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            Log.Info("Application started");
            desktop.Exit += OnExit;
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            _trayViewModel = new TrayViewModel(_syncService); // Pass the SyncService to the ViewModel.
            DataContext = _trayViewModel;

            AppSettings settingsService = new AppSettings();
            _mainViewModel = new MainViewModel(_syncService, settingsService);

            _syncService.SetInterval(settingsService.Current.SyncIntervalMinutes);
            _syncService.Start();

#if TEST_NOTIFICATION
            INotification _notificationService = NotificationFactory.Create();
            string message = $"A new version of ssh-agent-echo is available: v9.9.9";
            _notificationService.Notify("ssh-agent-echo", message, "Download", "https://www.google.com");
#endif

            Updater updater = new Updater();
            if (settingsService.Current.IsCheckForUpdatesEnabled) {
                updater.Run();
            }


#if DEBUG
            // For debugging, open the settings window immediately
            _settingsWindow = new SettingsWindow(_mainViewModel);
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
            Log.Error($"Error during OnExit disposing SyncService: {ex}");
        }
        Log.Info("Application is exiting");
    }

    private void Settings_Click(object? sender, EventArgs e) {
        if (_settingsWindow?.IsVisible == true) {
            _settingsWindow.Activate();
            return;
        }

        if (_mainViewModel != null) {
            _settingsWindow = new SettingsWindow(_mainViewModel);
            _settingsWindow.Closed += (_, _) => {
                _settingsWindow = null;
            };
            _settingsWindow.Show();
        }
    }

    private void Exit_Click(object? sender, EventArgs e) {
        Console.WriteLine("Exit_Click called");
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }
}