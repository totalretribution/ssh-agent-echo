namespace SshAgentEcho.Core
{
    public class SyncAgent
    {
        public void Sync(bool force = false)
        {
            var agent = new Agent();
            foreach (var identity in agent)
            {
                Console.WriteLine(identity);
            }
        }
    }
}
