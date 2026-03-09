using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Threading;
using System.Threading.Tasks;

using SshAgentEcho.Core;
using System.Diagnostics;

namespace SshAgentEcho.Gui.Services {

    public class SyncServiceArgs : EventArgs {
        public enum SyncStatus { Stopped, Running, Syncing, Error }
        public SyncStatus Status { get; set; }
    }

    public sealed class SyncService : IDisposable {
        public event EventHandler<SyncServiceArgs>? StatusChanged;
        public SyncServiceArgs.SyncStatus Status = SyncServiceArgs.SyncStatus.Stopped;
        private CancellationTokenSource? _cancellationToken;
        private Task? _periodicTask;
        private TimeSpan _interval = TimeSpan.FromMinutes(30);

        public void Start() {
            if (_periodicTask != null)
                return;
            _cancellationToken = new CancellationTokenSource();
            _periodicTask = Task.Run(() => SyncTask(_cancellationToken.Token));
        }

        public void Start(int intervalMinutes) {
            SetInterval(intervalMinutes);
            Start();
        }

        public async Task StopAsync() {
            if (_periodicTask == null)
                return;
            Log.Info("SyncTask: Stopping periodic sync task");
            try {
                // Signal the background task to stop.
                _cancellationToken?.Cancel();

                // Wait up to 5 seconds for the background task to finish cleanly.
                await Task.WhenAny(_periodicTask ?? Task.CompletedTask, Task.Delay(TimeSpan.FromSeconds(5)));
            } catch {
                // Catch exceptions during shutdown and ignore to avoid messing with exit.
            } finally {
                // Dispose and clear the CancellationTokenSource to free resources.
                try {
                    _cancellationToken?.Dispose();
                    _cancellationToken = null;
                } catch {
                    // Ignore any exceptions during cancellation token disposal to keep shutdown clean.
                }
                _periodicTask = null;
                RaiseStatus(new SyncServiceArgs { Status = SyncServiceArgs.SyncStatus.Stopped });
            }
        }

        public void Stop() {
            if (_periodicTask == null)
                return;
            _ = Task.Run(async () => {
                try {
                    await StopAsync().ConfigureAwait(false);
                } catch {
                    // Swallow any exception to keep restart from bubbling up to caller.
                }
            });
        }

        public async Task RestartAsync() {
            await StopAsync().ConfigureAwait(false);
            Start();
        }

        public void Restart() {
            _ = Task.Run(async () => {
                try {
                    await RestartAsync().ConfigureAwait(false);
                } catch {
                    // Swallow any exception to keep restart from bubbling up to caller.
                }
            });
        }

        public void Restart(int intervalMinutes) {
            SetInterval(intervalMinutes);
            Restart();
        }

        public void Sync() {
            _ = Task.Run(async () => {
                var serviceWasRunning = _periodicTask != null;
                try {
                    if (serviceWasRunning) {
                        await StopAsync().ConfigureAwait(false);
                    }
                    RaiseStatus(new SyncServiceArgs { Status = SyncServiceArgs.SyncStatus.Syncing });
                    var syncAgent = new SyncAgent();
                    syncAgent.Sync(true);
                } catch {
                    RaiseStatus(new SyncServiceArgs { Status = SyncServiceArgs.SyncStatus.Error });
                } finally {
                    if (serviceWasRunning) {
                        Start();
                    } else {
                        RaiseStatus(new SyncServiceArgs { Status = SyncServiceArgs.SyncStatus.Stopped });
                    }
                }
            });
        }

        public void SetInterval(int interval) {
            _interval = TimeSpan.FromMinutes(interval);
        }

        private async Task SyncTask(CancellationToken ct) {
            Log.Info("SyncTask: Starting periodic sync task using interval of " + _interval.Minutes + " minutes");
            RaiseStatus(new SyncServiceArgs { Status = SyncServiceArgs.SyncStatus.Running });
            var timer = new PeriodicTimer(_interval);
            try {
                while (await timer.WaitForNextTickAsync(ct)) {
                    RaiseStatus(new SyncServiceArgs { Status = SyncServiceArgs.SyncStatus.Syncing });
                    Log.Info("SyncTask: Starting sync operation");
                    var syncAgent = new SyncAgent();
                    syncAgent.Sync();
                    Log.Info("SyncTask: Finished sync operation");
                    RaiseStatus(new SyncServiceArgs { Status = SyncServiceArgs.SyncStatus.Running });
                }
            } finally {
                Log.Info("SyncTask: Stopping periodic sync task");
                timer.Dispose();
                RaiseStatus(new SyncServiceArgs { Status = SyncServiceArgs.SyncStatus.Stopped });
            }
        }

        public void Dispose() {
            try {
                // Use the existing StopAsync() which signals cancellation, waits with a timeout,
                // and swallows shutdown exceptions. Call synchronously and ignore any errors so
                // Dispose never throws during application exit.
                StopAsync().GetAwaiter().GetResult();
            } catch {
                // Swallow any exception during shutdown to keep application exit clean.
            } finally {
                try {
                    _cancellationToken?.Dispose();
                    _cancellationToken = null;
                } catch {
                    // Ignore any errors
                }
                _periodicTask = null;
                // Unsubscribe all event listeners to avoid dangling references
                StatusChanged = null;
            }
        }

        private void RaiseStatus(SyncServiceArgs status) {
            Status = status.Status;
            StatusChanged?.Invoke(this, status);
        }
    }
}