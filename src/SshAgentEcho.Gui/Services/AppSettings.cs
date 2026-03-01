using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;

namespace SshAgentEcho.Gui.Services {
    public partial class Settings : ObservableObject {
        [ObservableProperty]
        private int _syncIntervalMinutes = 30;
    }

    public class AppSettings {
        private const string FileName = "config.json";
        private static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, FileName);
        public Settings Current { get; private set; } = new();

        public AppSettings() {
            Load();
        }

        public void Load() {
            IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile(FileName, optional: true, reloadOnChange: false)
            .Build();
            var settings = config.GetSection("Settings").Get<Settings>();
            if (settings == null) {
                Log.Info($"No settings file found.");
                return;
            }
            Current.SyncIntervalMinutes = settings.SyncIntervalMinutes;
            Log.Info($"Settings loaded from {ConfigPath}");
        }

        public void Save() {
            // Use a strongly typed holder so trimming can analyze the type.
            var holder = new SettingsHolder { Settings = Current };
            var json = System.Text.Json.JsonSerializer.Serialize(holder, AppSettingsJsonContext.Default.SettingsHolder);
            File.WriteAllText(ConfigPath, json);
            Log.Info($"Settings saved to {ConfigPath}");
        }
    }

    // helper type used for json serialization to help dotnet trim analaysis.
    internal class SettingsHolder {
        public Settings Settings { get; set; } = null!;
    }

    [System.Text.Json.Serialization.JsonSerializable(typeof(SettingsHolder))]
    [System.Text.Json.Serialization.JsonSourceGenerationOptions(WriteIndented = true)]
    internal partial class AppSettingsJsonContext : System.Text.Json.Serialization.JsonSerializerContext {
    }
}