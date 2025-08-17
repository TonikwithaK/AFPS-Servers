using AFPS_Servers.Service.Config;

namespace AFPS_Servers.Service.Interfaces;

public interface ISshService
{
    Task<string> RunCommandAsync(SshConfig config, string command);
    Task StreamCommandAsync(SshConfig config, string command, Action<string> onOutput, CancellationToken cancellationToken = default);

}