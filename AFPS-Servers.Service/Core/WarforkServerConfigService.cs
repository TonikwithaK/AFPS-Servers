using AFPS_Servers.Service.Config;
using AFPS_Servers.Service.DTO;
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
            var parsed = await ParseConfigFullAsync(configStream);
            return parsed.Cvars.ToDictionary(c => c.Name, c => c.Value);
        }

        public async Task<ParsedConfig> ParseConfigFullAsync(Stream stream)
        {
            var result = new ParsedConfig();
            using var reader = new StreamReader(stream);

            var execPattern   = new Regex(@"^\s*exec\s+(\S+)",                                        RegexOptions.IgnoreCase);
            var aliasPattern  = new Regex(@"^\s*aliasa?\s+(\S+)\s+""?(.*?)""?\s*$",                   RegexOptions.IgnoreCase);
            var bindPattern   = new Regex(@"^\s*bind\s+(\S+)\s+""?(.*?)""?\s*$",                      RegexOptions.IgnoreCase);
            var cvarPattern   = new Regex(@"^\s*(set[aus]{0,2})\s+(\S+)\s+""?(.*?)""?\s*$",           RegexOptions.IgnoreCase);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("//"))
                    continue;

                Match m;
                if ((m = execPattern.Match(line)).Success)
                {
                    result.Execs.Add(new ConfigEntry { Key = m.Groups[1].Value, Value = "" });
                }
                else if ((m = aliasPattern.Match(line)).Success)
                {
                    result.Aliases.Add(new ConfigEntry { Key = m.Groups[1].Value, Value = m.Groups[2].Value });
                }
                else if ((m = bindPattern.Match(line)).Success)
                {
                    result.Binds.Add(new ConfigEntry { Key = m.Groups[1].Value, Value = m.Groups[2].Value });
                }
                else if ((m = cvarPattern.Match(line)).Success)
                {
                    result.Cvars.Add(new CvarEntry
                    {
                        Directive = m.Groups[1].Value.ToLower(),
                        Name = m.Groups[2].Value,
                        Value = m.Groups[3].Value
                    });
                }
            }

            return result;
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

        public string GenerateConfigContent(ParsedConfig parsed)
        {
            var sb = new StringBuilder();

            if (parsed.Execs.Any())
            {
                sb.AppendLine("// exec");
                foreach (var e in parsed.Execs)
                    sb.AppendLine($"exec {e.Key}");
                sb.AppendLine();
            }

            if (parsed.Cvars.Any())
            {
                sb.AppendLine("// cvars");
                foreach (var c in parsed.Cvars)
                    sb.AppendLine($"{c.Directive} {c.Name} \"{c.Value}\"");
                sb.AppendLine();
            }

            if (parsed.Aliases.Any())
            {
                sb.AppendLine("// aliases");
                foreach (var a in parsed.Aliases)
                    sb.AppendLine($"alias {a.Key} \"{a.Value}\"");
                sb.AppendLine();
            }

            if (parsed.Binds.Any())
            {
                sb.AppendLine("// binds");
                foreach (var b in parsed.Binds)
                    sb.AppendLine($"bind {b.Key} \"{b.Value}\"");
                sb.AppendLine();
            }

            return sb.ToString();
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