using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Threading;
using System.Threading.Tasks;

using SshAgentEcho.Core;
using System.Diagnostics;

namespace SshAgentEcho.Gui;

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

    // private volatile bool _enabled = true;

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

    public void Enable() {
        // _enabled = true;
    }

    public void Disable() {
        // _enabled = false;
    }


    public async Task StopAsync() {
        if (_periodicTask == null)
            return;
        try {
            // Signal the periodic background sync task to stop.
            _cancellationToken?.Cancel();

            // Wait up to 5 seconds for the background task to finish cleanly.
            await Task.WhenAny(_periodicTask ?? Task.CompletedTask, Task.Delay(TimeSpan.FromSeconds(5)));
        } catch {
            // Catch exceptions during shutdown  and ignore to avoid messing with exit.
        } finally {
            // Dispose and clear the CancellationTokenSource to free resources.
            _cancellationToken?.Dispose();
            _cancellationToken = null;
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
        _ = Task.Run(async () => {
            try {
                await RestartAsync().ConfigureAwait(false);
            } catch {
                // Swallow any exception to keep restart from bubbling up to caller.
            }
        });
    }

    public void Sync() {

    }

    public void SetInterval(int interval) {
        _interval = TimeSpan.FromMinutes(interval);
    }

    private async Task SyncTask(CancellationToken ct) {
        Debug.WriteLine("SyncTask: Starting periodic sync task");
        RaiseStatus(new SyncServiceArgs { Status = SyncServiceArgs.SyncStatus.Running });
        using var timer = new PeriodicTimer(_interval);
        while (await timer.WaitForNextTickAsync(ct)) {
            RaiseStatus(new SyncServiceArgs { Status = SyncServiceArgs.SyncStatus.Syncing });
            Debug.WriteLine("SyncTask: Starting sync operation");
            var syncAgent = new SyncAgent();
            syncAgent.Sync();
            Debug.WriteLine("SyncTask: Finished sync operation");
            RaiseStatus(new SyncServiceArgs { Status = SyncServiceArgs.SyncStatus.Running });
        }
        Debug.WriteLine("SyncTask: Exiting periodic sync task");
        RaiseStatus(new SyncServiceArgs { Status = SyncServiceArgs.SyncStatus.Stopped });
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
            _cancellationToken?.Dispose();
            _cancellationToken = null;
            _periodicTask = null;
            RaiseStatus(new SyncServiceArgs { Status = SyncServiceArgs.SyncStatus.Stopped });
        }
    }

    private void RaiseStatus(SyncServiceArgs status) {
        Status = status.Status;
        StatusChanged?.Invoke(this, status);
    }
}
