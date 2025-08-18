using AFPS_Servers.Service.Config;
using AFPS_Servers.Service.Interfaces;
using Renci.SshNet;
using System.Text;

public class SshService : ISshService
{
    public async Task<string> RunCommandAsync(SshConfig config, string command)
    {
        return await Task.Run(() =>
        {
            using var client = new SshClient(config.Host, config.Port, config.Username, config.Password);
            client.Connect();
            var result = client.RunCommand(command).Result;
            client.Disconnect();
            return result;
        });
    }

    public async Task StreamCommandAsync(SshConfig config, string command, Action<string> onOutput, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            using var client = new SshClient(config.Host, config.Port, config.Username, config.Password);
            client.Connect();
            using var cmd = client.CreateCommand(command);
            var asyncResult = cmd.BeginExecute();

            var buffer = new byte[4096];
            var outputStream = cmd.OutputStream;
            while (!asyncResult.IsCompleted)
            {
                while (outputStream.CanRead && outputStream.Length > 0)
                {
                    int bytesRead = outputStream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        var output = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        onOutput(output);
                    }
                }
                Thread.Sleep(100);
                if (cancellationToken.IsCancellationRequested)
                    break;
            }
            cmd.EndExecute(asyncResult);
            client.Disconnect();
        }, cancellationToken);
    }

    public async Task<string> RunInteractiveCommandAsync(SshConfig config, string command, string? input = null)
    {
        return await Task.Run(() =>
        {
            using var client = new SshClient(config.Host, config.Port, config.Username, config.Password);
            client.Connect();
            
            using var shell = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024);
            
            shell.WriteLine(command);
            
            if (!string.IsNullOrEmpty(input))
            {
                shell.WriteLine(input);
            }
            
            Thread.Sleep(1000);
            
            var result = shell.Read();
            client.Disconnect();
            return result;
        });
    }
}
