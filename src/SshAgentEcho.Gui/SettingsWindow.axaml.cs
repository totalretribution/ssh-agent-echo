using Avalonia.Controls;
using Avalonia.Interactivity;
using SshAgentEcho.Gui.Services;
using SshAgentEcho.Gui.ViewModels;

namespace SshAgentEcho.Gui;

public partial class SettingsWindow : Window {
    SyncService _syncService = new();

    public SettingsWindow() : this(new SyncService()) { }

    public SettingsWindow(SyncService syncService) {
        _syncService = syncService;
        InitializeComponent();

        var mainViewModel = new MainViewModel(_syncService);
        DataContext = mainViewModel;

    }

    private void Close_Click(object? sender, RoutedEventArgs e) {
        Close();
    }
}