namespace SshAgentEcho.Gui.Services {
    public class Settings {
        public int SyncIntervalMinutes { get; set; } = 30;
    }
    public class Config {
        private Settings _settings { get; set; } = new Settings();

        public Settings Load() {
            // In a real application, load settings from file or other storage here.
            return _settings;
        }

        public void Save(Settings settings) {
            // In a real application, save settings to file or other storage here.
            _settings = settings;
        }

    }
}