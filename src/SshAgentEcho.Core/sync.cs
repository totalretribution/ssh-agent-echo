namespace SshAgentEcho.Core {
    public class SyncAgent {
        public void Sync(bool force = false) {
            var agent = new Agent();
            var identities = agent.GetIdentities();
            var identitiesList = identities.ToList();
            var config = new Config();
            string? saved_crc_hash = config.GetCrcConfigHash();
            string new_crc_hash = config.GenerateCrcHash(identitiesList);

            if (!force && saved_crc_hash != null) {
                if (saved_crc_hash == new_crc_hash) {
                    Console.WriteLine("No changes detected, skipping sync.");
                    return;
                }
            }

            config.GenerateKeyFiles(identitiesList);
            config.CreateConfigFile(identitiesList, new_crc_hash);
            config.EditBaseConfigFile();
        }
    }
}
