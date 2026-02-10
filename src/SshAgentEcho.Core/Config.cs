using System.IO;
using System.IO.Enumeration;
using System.Security.Cryptography;
using System.Text;

namespace SshAgentEcho.Core;

public class Config {
    private const string SSH_AGENT_SYNC_CRC_PREFIX = "### SSH_AGENT_SYNC_CRC=";
    private const string SSH_CONFIG_FILE_NAME = "config.ssh_agent_sync";
    private const string SSH_DIR_NAME = ".ssh";
    private const string SSH_CONFIG_KEY_FOLDER = "ssh_agent_sync";
    private const string SSH_BASE_CONFIG_FILE_NAME = "config";

    private static readonly string _homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    private static readonly string _sshDirectory = Path.Combine(_homeDirectory, SSH_DIR_NAME);
    private static readonly string _sshBaseConfigFilePath = Path.Combine(_sshDirectory, SSH_BASE_CONFIG_FILE_NAME);
    private static readonly string _sshConfigFilePath = Path.Combine(_sshDirectory, SSH_CONFIG_FILE_NAME);
    private static readonly string _sshConfigKeyFolderPath = Path.Combine(_sshDirectory, SSH_CONFIG_KEY_FOLDER);
    private static readonly string _sshConfigIncludeDirective = $"Include {_sshConfigFilePath}";

    public Config() {
        Console.WriteLine("Using the following ssh config paths:");
        Console.WriteLine($"  -  SSH Directory: {_sshDirectory}");
        Console.WriteLine($"  -  SSH Base Config File Path: {_sshBaseConfigFilePath}");
        Console.WriteLine($"  -  SSH Config File Path: {_sshConfigFilePath}");
        Console.WriteLine($"  -  SSH Config Key Folder Path: {_sshConfigKeyFolderPath}");
        Console.WriteLine($"  -  SSH Config Include Directive: {_sshConfigIncludeDirective}");
    }

    public string? GetCrcConfigHash() {
        Console.WriteLine($"Checking for existing CRC hash in config file at {_sshConfigFilePath}");
        if (!File.Exists(_sshConfigFilePath)) return null;

        foreach (var line in File.ReadLines(_sshConfigFilePath)) {
            if (line.StartsWith(SSH_AGENT_SYNC_CRC_PREFIX, StringComparison.Ordinal)) {
                string crc = line;
                crc = crc.Substring(SSH_AGENT_SYNC_CRC_PREFIX.Length).Trim();
                Console.WriteLine($"Found existing CRC hash in config: {crc}");
                return crc;
            }
        }
        return null;
    }

    public string GenerateCrcHash(List<Agent.Identity> identities) {
        // Create the hasher
        using IncrementalHash sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        foreach (var id in identities) {
            // Convert string to bytes and append to the hash calculation
            byte[] comment_data = Encoding.UTF8.GetBytes(id.Comment);
            sha256.AppendData(comment_data);
            byte[] hash_data = Encoding.UTF8.GetBytes(id.Hash);
            sha256.AppendData(hash_data);
        }

        byte[] hashBytes = sha256.GetHashAndReset();
        string hashString = Convert.ToHexString(hashBytes);
        string hash = hashString.ToUpper();
        Console.WriteLine($"Generated CRC hash: {hash}");
        return hash;
    }

    public string GenerateConfigEntry(Agent.Identity identity) {
        string key_path = Path.Combine(_sshConfigKeyFolderPath, identity.Filename);
        string entry = "";

        if (!string.IsNullOrEmpty(identity.Name)) {
            entry += $"Host {identity.Name}\n";
            entry += $"    HostName {identity.Host}\n";
            entry += $"    User {identity.User}\n";
            entry += $"    IdentityFile {key_path}\n";
            entry += "    IdentitiesOnly yes\n\n";
        }

        entry += $"Host {identity.Host}\n";
        entry += $"    User {identity.User}\n";
        entry += $"    IdentityFile {key_path}\n";
        entry += "    IdentitiesOnly yes\n\n";
        return entry;
    }

    public string GenerateConfigFileContent(List<Agent.Identity> identities, string crc_hash) {
        Console.WriteLine($"Generating config file.");
        String config = SSH_AGENT_SYNC_CRC_PREFIX + crc_hash + "\n\n";
        config += "Host *\n    IdentitiesOnly yes\n\n";

        foreach (var identity in identities) {
            config += GenerateConfigEntry(identity);
        }
        return config;
    }

    public void CreateConfigFile(List<Agent.Identity> identities, string crc_hash) {
        string configFile = GenerateConfigFileContent(identities, crc_hash);

        if (File.Exists(_sshConfigFilePath)) {
            Console.WriteLine($"Deleting existing config file at {_sshConfigFilePath}");
            File.Delete(_sshConfigFilePath);
        }
        Console.WriteLine($"Writing new config file to {_sshConfigFilePath}");
        File.WriteAllText(_sshConfigFilePath, configFile);
    }

    public void DeleteKeyFolder() {
        if (Directory.Exists(_sshConfigKeyFolderPath)) {
            Console.WriteLine($"Deleting existing key folder at {_sshConfigKeyFolderPath}");
            Directory.Delete(_sshConfigKeyFolderPath, true);
        }
    }

    public void GenerateKeyFiles(List<Agent.Identity> identities) {
        Console.WriteLine($"Generating key files in {_sshConfigKeyFolderPath}");
        DeleteKeyFolder();
        Directory.CreateDirectory(_sshConfigKeyFolderPath);

        foreach (var identity in identities) {
            string key_path = Path.Combine(_sshConfigKeyFolderPath, identity.Filename);
            string key_content = $"{identity.Type} {identity.Hash} {identity.Comment}";
            if (!File.Exists(key_path)) {
                File.WriteAllText(key_path, key_content);
                Console.WriteLine($"Created key file: {key_path}");
            }
        }
    }

    public bool CheckBaseConfigNeedsEditing() {
        Console.WriteLine($"Checking base config file {_sshBaseConfigFilePath}");

        if (!File.Exists(_sshBaseConfigFilePath)) {
            Console.WriteLine($"Base config file does not exist, will need to create new one.");
            return true;
        }

        foreach (var line in File.ReadLines(_sshBaseConfigFilePath)) {
            if (line.Trim() == _sshConfigIncludeDirective) {
                Console.WriteLine($"Base config file already includes the directive, no edit needed.");
                return false;
            }
        }

        return true;
    }

    public bool EditBaseConfigFile() {
        if (!CheckBaseConfigNeedsEditing()) {
            return false;
        }
        Console.WriteLine($"Adding include directive to base config file at {_sshBaseConfigFilePath}");
        var current_config_file = _sshConfigIncludeDirective + "\n\n";
        current_config_file += File.Exists(_sshBaseConfigFilePath) ? File.ReadAllText(_sshBaseConfigFilePath) : "";
        File.WriteAllText(_sshBaseConfigFilePath, current_config_file);
        return true;
    }
}