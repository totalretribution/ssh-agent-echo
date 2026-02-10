using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using SshAgentEcho.Core;
using System.Reflection;

class Program {
    static int Main(string[] args) {
        Option<bool> printOption = new("--print") {
            Description = "Print keys like ssh-add -L"
        };

        Option<bool> syncOption = new("--sync") {
            Description = "Sync ssh agent keys to ssh config"
        };

        Option<bool> forceOption = new("--force") {
            Description = "Force sync even if CRC matches"
        };

        RootCommand rootCommand = new("ssh-agent-echo - A tool to sync SSH agent public keys to ssh config");
        rootCommand.Options.Add(printOption);
        rootCommand.Options.Add(syncOption);
        rootCommand.Options.Add(forceOption);

        var version = Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
        Console.WriteLine("───────────────────────");
        Console.WriteLine($" ssh-agent-echo v{version}");
        Console.WriteLine("───────────────────────\n");

        rootCommand.SetAction(parseResult => {
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