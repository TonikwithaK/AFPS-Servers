using AFPS_Servers.Service.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFPS_Servers.Service.Interfaces
{
    public interface IWarforkServerConfigService
    {
        Task<string?> FindAutoexecPathAsync(ISshService sshService, SshConfig config, string rootDir = "/root/server");
        Task<Dictionary<string, string>> ParseConfigAsync(Stream configStream);
        string GenerateConfigContent(Dictionary<string, string> config);

    }
}
