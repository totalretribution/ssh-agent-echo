using System;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SshAgentEcho.Gui.Services;
using SshAgentEcho.Gui.ViewModels;

namespace SshAgentEcho.Gui;

public partial class SettingsWindow : Window {
    public MainViewModel? _mainViewModel;

    public SettingsWindow() {
        InitializeComponent();
    }

    public SettingsWindow(MainViewModel mainViewModel) {
        InitializeComponent();
        Version version = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0, 0);
        this.Title = "Settings - ssh-agent-echo v" + version.ToString(3);
        _mainViewModel = mainViewModel;
        DataContext = _mainViewModel;
    }

    private void Close_Click(object? sender, RoutedEventArgs e) {
        Close();
    }

    private void SaveSettings_Click(object? sender, RoutedEventArgs e) {
        if (_mainViewModel == null) return;
        _mainViewModel.SettingsService.Save();
        _mainViewModel.SyncService.Restart(_mainViewModel.SettingsService.Current.SyncIntervalMinutes);
    }

    private void ResetSettings_Click(object? sender, RoutedEventArgs e) {
        _mainViewModel?.SettingsService.Load();
    }
}