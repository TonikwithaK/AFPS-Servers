using AFPS_Servers.Service.Config;
using AFPS_Servers.Service.Interfaces;
using System.Text;
using System.Text.RegularExpressions;

namespace AFPS_Servers.Service.Core
{

    public class WarforkServerConfigService : IWarforkServerConfigService
    {
        public async Task<string?> FindAutoexecPathAsync(ISshService sshService, SshConfig config, string rootDir = "/root/server")
        {
            var command = $"find {rootDir} -type f -name dedicated_autoexec.cfg";
            var output = await sshService.RunCommandAsync(config, command);

            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            return lines.FirstOrDefault();
        }

        public async Task<Dictionary<string, string>> ParseConfigAsync(Stream configStream)
        {   
            var config = new Dictionary<string, string>();
            using var reader = new StreamReader(configStream);
            var setPattern = new Regex(@"^\s*set\s+(\S+)\s+""?(.*?)""?\s*$");

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("//"))
                    continue;

                var match = setPattern.Match(line);
                if (match.Success)
                {
                    var key = match.Groups[1].Value;
                    var value = match.Groups[2].Value;
                    config[key] = value;
                }
            }

            return config;
        }

        public string GenerateConfigContent(Dictionary<string, string> config)
        {
            var builder = new StringBuilder();
            foreach (var kvp in config)
            {
                builder.AppendLine($"set {kvp.Key} \"{kvp.Value}\"");
            }
            return builder.ToString();
        }

        public async Task<string> GetRawConfigAsync(ISshService sshService, SshConfig config, string configPath)
        {
            var command = $"cat {configPath}";
            var result = await sshService.RunCommandAsync(config, command);
            return result;
        }

        public async Task SaveRawConfigAsync(ISshService sshService, ISftpService sftpService, SshConfig config, SftpConfig sftpConfig, string configPath, string content)
        {
            var tempPath = $"/tmp/config_{DateTime.Now.Ticks}.cfg";
            
            await sshService.RunCommandAsync(config, $"cat > {tempPath} << 'EOF'\n{content}\nEOF");
            
            await sshService.RunCommandAsync(config, $"cp {tempPath} {configPath}");
            
            await sshService.RunCommandAsync(config, $"rm {tempPath}");
        }

    }
}