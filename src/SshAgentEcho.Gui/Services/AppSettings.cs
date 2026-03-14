using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;

namespace SshAgentEcho.Gui.Services {
    public partial class Settings : ObservableObject {
        [ObservableProperty]
        [property: System.Text.Json.Serialization.JsonInclude]
        private int _syncIntervalMinutes = 30;
        [ObservableProperty]
        [property: System.Text.Json.Serialization.JsonInclude]
        private bool _isCheckForUpdatesEnabled = true;

        public string ToJson() {
            return new System.Text.Json.Nodes.JsonObject {
                ["Settings"] = new System.Text.Json.Nodes.JsonObject {
                    ["syncIntervalMinutes"] = SyncIntervalMinutes,
                    ["isCheckForUpdatesEnabled"] = IsCheckForUpdatesEnabled
                }
            }.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }

        public void FromJson(string json) {
            System.Text.Json.Nodes.JsonNode? rootNode = System.Text.Json.Nodes.JsonNode.Parse(json);
            if (rootNode is System.Text.Json.Nodes.JsonObject root) {
                if (root["Settings"] is System.Text.Json.Nodes.JsonObject settingsObj) {
                    SyncIntervalMinutes = settingsObj["syncIntervalMinutes"]?.GetValue<int>() ?? SyncIntervalMinutes;
                    IsCheckForUpdatesEnabled = settingsObj["isCheckForUpdatesEnabled"]?.GetValue<bool>() ?? IsCheckForUpdatesEnabled;
                }
            }
        }
    }

    public class AppSettings {
        private const string FileName = "config.json";
        private static readonly string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ssh-agent-echo", FileName);
        public Settings Current { get; private set; } = new();

        public AppSettings() {
            // ensure we can persist setting changes automatically
            Current.PropertyChanged += Current_PropertyChanged;
            Load();
        }

        private void Current_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(Settings.IsCheckForUpdatesEnabled)) {
                SaveValue(e.PropertyName, Current.IsCheckForUpdatesEnabled);
            }
        }

        private Settings? LoadConfigFile() {
            Settings setting = new();
            if (File.Exists(ConfigPath)) {
                try {
                    string json = File.ReadAllText(ConfigPath);
                    setting.FromJson(json);
                    return setting;
                } catch (Exception) {
                    return null;
                }
            } else {
                return null;
            }
        }

        private void SaveConfigFile(Settings settings) {
            try {
                var json = settings.ToJson();
                var dir = Path.GetDirectoryName(ConfigPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(ConfigPath, json);
            } catch (Exception ex) {
                throw new IOException($"Failed to save settings to {ConfigPath}", ex);
            }
        }

        public void Load() {
            var settings = LoadConfigFile();
            if (settings == null) {
                Log.Info($"No settings file found.");
                return;
            }
            Current.SyncIntervalMinutes = settings.SyncIntervalMinutes;
            Current.IsCheckForUpdatesEnabled = settings.IsCheckForUpdatesEnabled;
            Log.Info($"Settings loaded from {ConfigPath}");
        }

        public void Save() {
            try {
                SaveConfigFile(Current);
                Log.Info($"Settings saved to {ConfigPath}");
            } catch (Exception ex) {
                Log.Error(ex);
            }
        }

        public void SaveValue<T>(string propertyName, T value) {
            var property = typeof(Settings).GetProperty(propertyName);
            if (property == null || !property.CanWrite) {
                throw new ArgumentException($"Property '{propertyName}' not found or is not writable on Settings.");
            }
            Settings loadedSettings = LoadConfigFile() ?? new Settings();
            property.SetValue(loadedSettings, value);

            try {
                SaveConfigFile(loadedSettings);
                Log.Info($"Settings saved to {ConfigPath}");
            } catch (Exception ex) {
                Log.Error(ex);
            }
        }
    }
}