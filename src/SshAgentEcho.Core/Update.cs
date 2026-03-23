using Octokit;
using System.Runtime.InteropServices;
using System.Reflection;

namespace SshAgentEcho.Core;

public class Update {
    public string? ReleaseNotes { get; private set; } = null;
    public ReleaseAsset? LatestAsset { get; private set; } = null;
    public Release? LatestRelease { get; private set; } = null;

    private Version _latestVersion = new Version(0, 0, 0);
    private Version _currentVersion = new Version(0, 0, 0);
    private string _repo;
    private string _owner;

    public Update() {
        _repo = "ssh-agent-echo";
        _owner = "totalretribution";
#if !DEBUG
        var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
        if (version == null) {
            _currentVersion = new Version(0, 0, 0);
        } else {
            _currentVersion = Version.Parse(version);
        }
#endif
    }

    public bool IsNewVersionAvailable() {
        if (_latestVersion == null) return false;
        try {
            return _latestVersion > _currentVersion;
        } catch (Exception) {
            return false;
        }
    }

    public string? GetDownloadUrl() {
        return LatestAsset?.BrowserDownloadUrl;
    }

    public string? GetReleaseURL() {
        return LatestRelease?.HtmlUrl;
    }

    public string? GetContentType() {
        return LatestAsset?.ContentType;
    }

    public DateTime? GetReleaseUTC() {
        return LatestRelease?.PublishedAt?.UtcDateTime;
    }

    public int? GetUpdateSize() {
        return LatestAsset?.Size;
    }

    public string GetCurrentVersion() {
        return _currentVersion.ToString(3);
    }

    public string GetLatestVersion() {
        return _latestVersion.ToString(3);
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
            _latestVersion = Version.Parse(latestRelease.TagName.TrimStart('v'));
            ReleaseNotes = latestRelease.Body;
            var asset = GetAsset(latestRelease.Assets);
            if (asset == null) {
                throw new Exception("No suitable asset found for the current platform.");
            }
            LatestAsset = asset;
            LatestRelease = latestRelease;

        } catch (Exception ex) {
            Log.Error($"An error occurred while checking for updates: {ex.Message}");
            _latestVersion = new Version(0, 0, 0);
            ReleaseNotes = null;
            LatestAsset = null;
            LatestRelease = null;
        }
    }
}