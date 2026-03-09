using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

using SshAgentEcho.Gui.Services;
using SshAgentEcho.Autostart;
using System.Reflection;

namespace SshAgentEcho.Gui.ViewModels {

    public partial class MainViewModel : ObservableObject {
        private readonly SyncService _syncService;
        public SyncService SyncService => _syncService;

        public AppSettings SettingsService { get; }

        private readonly IAutostartService _autostartService;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SyncCommand))]
        private bool allowManualSync;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartCommand))]
        [NotifyCanExecuteChangedFor(nameof(StopCommand))]
        private bool running;

        [ObservableProperty]
        private string? statusText;

        public ObservableCollection<string> LogEntries => LogService.Instance.LogEntries;

        [ObservableProperty]
        private bool isStartAtBootEnabled;

        partial void OnIsStartAtBootEnabledChanged(bool value) {
            if (value) {
                if (_autostartService.Install()) {
                    SetProperty(ref isStartAtBootEnabled, true, nameof(IsStartAtBootEnabled));
                } else {
                    SetProperty(ref isStartAtBootEnabled, false, nameof(IsStartAtBootEnabled));
                }
            } else {
                if (_autostartService.Uninstall()) {
                    SetProperty(ref isStartAtBootEnabled, false, nameof(IsStartAtBootEnabled));
                } else {
                    SetProperty(ref isStartAtBootEnabled, true, nameof(IsStartAtBootEnabled));
                }
            }
        }

        public MainViewModel(SyncService syncService, AppSettings settingsService) {
            SettingsService = settingsService;
            _syncService = syncService;
            var appName = Assembly.GetExecutingAssembly().GetName().Name ?? "";
            _autostartService = AutostartServiceFactory.Create(appName);
            isStartAtBootEnabled = _autostartService.IsInstalled();

            // Subscribe to your service's event
            _syncService.StatusChanged += OnSyncStatusChanged;
            AllowManualSync = _syncService.Status == SyncServiceArgs.SyncStatus.Running;
            Running = _syncService.Status == SyncServiceArgs.SyncStatus.Running;
            StatusText = $"{_syncService.Status}";
        }

        /*
        ******************
        MARK: COMMANDS
        ******************
        */
        [RelayCommand(CanExecute = nameof(CanSync))]
        private void Sync() {
            // Start your thread/background work here
            Log.Info("Sync command executed");
            _syncService.Sync();
        }

        [RelayCommand(CanExecute = nameof(CanStart))]
        private void Start() {
            // Start your thread/background work here
            Log.Info("Start command executed");
            _syncService.Start();
        }

        [RelayCommand(CanExecute = nameof(CanStop))]
        private void Stop() {
            // Start your thread/background work here
            Log.Info("Stop command executed");
            _syncService.Stop();
        }

        /*
        ***********************
        MARK: CAN EXECUTE LOGIC
        ***********************
        */
        private bool CanSync() => AllowManualSync;

        private bool CanStart() => Running == false;

        private bool CanStop() => Running;

        /*
        *******************
        MARK:EVENT HANDLERS
        *******************
        */
        private void OnSyncStatusChanged(object? sender, SyncServiceArgs e) {
            // IMPORTANT: Events from separate threads must often be 
            // brought back to the UI thread in Avalonia.
            Avalonia.Threading.Dispatcher.UIThread.Post(() => {
                // Update your property based on the event args
                StatusText = $"{e.Status}";
                if (e.Status == SyncServiceArgs.SyncStatus.Running) {
                    AllowManualSync = true;
                    Running = true;
                } else if (e.Status == SyncServiceArgs.SyncStatus.Syncing) {
                    AllowManualSync = false;
                    Running = true;
                } else {
                    AllowManualSync = false;
                    Running = false;
                }
            });
        }

        /*
        *********************
        MARK:System Functions
        *********************
        */
        public void Dispose() {
            // Unsubscribe from events to prevent memory leaks
            _syncService.StatusChanged -= OnSyncStatusChanged;
        }
    }
}