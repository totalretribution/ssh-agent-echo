using Octokit;
using System.Runtime.InteropServices;
using System.Reflection;

namespace SshAgentEcho.Core;

public class Update {
    public string? LatestVersion { get; private set; } = null;
    public string? ReleaseNotes { get; private set; } = null;
    public ReleaseAsset? LatestAsset { get; private set; } = null;

    private string _currentVersion;
    private string _repo;
    private string _owner;

    public Update() {
        var version = Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (version == null) {
            return;
        }

        _currentVersion = version;
        _repo = "ssh-agent-echo";
        _owner = "totalretribution";
    }

    public bool IsNewVersionAvailable() {
        if (LatestVersion == null) return false;
        try {
            var current = Version.Parse(_currentVersion);
            var latest = Version.Parse(LatestVersion);
            return latest > current;
        } catch (Exception) {
            return false;
        }
    }

    public string? GetDownloadUrl() {
        return LatestAsset?.BrowserDownloadUrl;
    }

    public string? GetReleaseURL() {
        return LatestAsset?.Url;
    }

    public string? GetContentType() {
        return LatestAsset?.ContentType;
    }

    public DateTime? GetReleaseUTC() {
        return LatestAsset?.CreatedAt.UtcDateTime;
    }

    public int? GetUpdateSize() {
        return LatestAsset?.Size;
    }

    private ReleaseAsset? GetAsset(IReadOnlyList<ReleaseAsset>? assets = null) {
        if (assets == null) return null;
        string rid = RuntimeInformation.RuntimeIdentifier;
        var asset = assets.FirstOrDefault(a => a.Name.Contains(rid, StringComparison.OrdinalIgnoreCase));
        return asset;
    }

    public async Task Check() {
        if (_repo == null || _owner == null || _currentVersion == null) {
            throw new InvalidOperationException("Repo, owner, and current version must be set.");
        }
        var client = new GitHubClient(new Octokit.ProductHeaderValue(_repo));

        try {
            Log.Info("Checking for updates...");
            var latestRelease = await client.Repository.Release.GetLatest(_owner, _repo);
            var latestVersion = Version.Parse(latestRelease.TagName.TrimStart('v'));
            LatestVersion = latestVersion.ToString();
            ReleaseNotes = latestRelease.Body;
            var asset = GetAsset(latestRelease.Assets);
            if (asset == null) {
                throw new Exception("No suitable asset found for the current platform.");
            }
            LatestAsset = asset;

        } catch (Exception ex) {
            Log.Error($"An error occurred while checking for updates: {ex.Message}");
            LatestVersion = null;
            ReleaseNotes = null;
            LatestAsset = null;
        }
    }
}