namespace SshAgentEcho.Autostart {
    public interface IAutostartService {
        bool Install();
        bool Uninstall();
        bool IsInstalled();
    }
}