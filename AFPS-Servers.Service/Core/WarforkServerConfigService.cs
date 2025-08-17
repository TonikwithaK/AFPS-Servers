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

    }
}