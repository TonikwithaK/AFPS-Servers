using AFPS_Servers.Service.Config;
using AFPS_Servers.Service.DTO;

namespace AFPS_Servers.Service.Interfaces
{
    public interface IWarforkServerConfigService
    {
        Task<string?> FindAutoexecPathAsync(ISshService sshService, SshConfig config, string rootDir = "/root/server");
        Task<Dictionary<string, string>> ParseConfigAsync(Stream configStream);
        Task<ParsedConfig> ParseConfigFullAsync(Stream stream);
        string GenerateConfigContent(Dictionary<string, string> config);
        string GenerateConfigContent(ParsedConfig parsed);
        Task<string> GetRawConfigAsync(ISshService sshService, SshConfig config, string configPath);
        Task SaveRawConfigAsync(ISshService sshService, ISftpService sftpService, SshConfig config, SftpConfig sftpConfig, string configPath, string content);
    }
}
