using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using SshAgentEcho.Core;
using System.Reflection;
using System.Diagnostics;

class Program {
    static int Main(string[] args) {
        Trace.Listeners.Clear();
        Trace.Listeners.Add(new ConsoleTraceListener());

        Log.CoreSource.Listeners.Clear();
        Log.CoreSource.Listeners.Add(new CleanStringListener());
        Log.CoreSource.Switch = new SourceSwitch("coreSwitch", "All");

        Option<bool> printOption = new("--print") {
            Description = "Print keys like ssh-add -L"
        };

        Option<bool> syncOption = new("--sync") {
            Description = "Sync ssh agent keys to ssh config"
        };

        Option<bool> forceOption = new("--force") {
            Description = "Force sync even if CRC matches"
        };

        Option<bool> updateOption = new("--update") {
            Description = "Check for updates"
        };

        RootCommand rootCommand = new("ssh-agent-echo - A tool to sync SSH agent public keys to ssh config");
        rootCommand.Options.Add(printOption);
        rootCommand.Options.Add(syncOption);
        rootCommand.Options.Add(forceOption);
        rootCommand.Options.Add(updateOption);

        var version = Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";

        var title = $"ssh-agent-echo v{version}";
        var bar = new string('─', title.Length + 2);

        Console.WriteLine(bar);
        Console.WriteLine($" {title}");
        Console.WriteLine(bar + "\n");

        rootCommand.SetAction(parseResult => {
            bool isUpdate = parseResult.GetValue(updateOption);
            if (isUpdate) {
                var updateChecker = new Update();
                updateChecker.Check().Wait();
                if (updateChecker.IsNewVersionAvailable()) {
                    Log.Info("A new version is available!");
                    Log.Info($"Current version: {updateChecker.GetCurrentVersion()}");
                    Log.Info($"Latest version: {updateChecker.GetLatestVersion()}");
                    Log.Info($"Release URL: {updateChecker.GetReleaseURL()}");
                    Log.Info($"Download URL: {updateChecker.GetDownloadUrl()}");
                    // Log.Info($"Release notes: {updateChecker.ReleaseNotes}");
                } else {
                    Log.Info("You are using the latest version.");
                }
                return;
            }

            bool isVerbose = parseResult.GetValue(printOption);
            if (isVerbose) {
                var agent = new Agent();
                agent.PrintIdentities();
                Console.WriteLine($"Total identities: {agent.GetIdentities().Count}\n");
            }

            bool isSync = parseResult.GetValue(syncOption);
            bool isForce = parseResult.GetValue(forceOption);
            if (isSync || isForce) {
                var syncAgent = new SyncAgent();
                syncAgent.Sync(isForce);
            }
        });

        // If no arguments provided, show help and exit with the help exit code.
        if (args.Length == 0) {
            return rootCommand.Parse(new string[] { "--help" }).Invoke();
        }

        return rootCommand.Parse(args).Invoke();
    }
}