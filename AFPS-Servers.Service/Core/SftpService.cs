using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace AFPS_Servers.Service.Core;
public class SftpService : ISftpService
{

    public SftpClient Connect(string host, int port, string username, string password)
    {
        var client = new SftpClient(host, port, username, password);
        client.Connect();
        return client;
    }

    public void Disconnect(SftpClient client)
    {
        if (client?.IsConnected ?? false)
            client.Disconnect();
        client?.Dispose();
    }

    public void UploadFile(SftpClient client, string localPath, string remotePath)
    {
        using var fileStream = File.OpenRead(localPath);
        client.UploadFile(fileStream, remotePath, true);
    }

    public async Task UploadFileAsync(SftpClient client, string localPath, string remotePath, CancellationToken cancellationToken = default)
    {
        await using var fileStream = File.OpenRead(localPath);
        await Task.Run(() => client.UploadFile(fileStream, remotePath, true), cancellationToken);
    }

    public async Task UploadFileAsync(SftpClient client, Stream inputStream, string remotePath, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await inputStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0; // Reset position for reading
        await Task.Run(() => client.UploadFile(memoryStream, remotePath, true), cancellationToken);
    }

    public async Task<Stream> DownloadFileAsync(SftpClient client, string remotePath, CancellationToken cancellationToken = default)
    {
        var memoryStream = new MemoryStream();
        await Task.Run(() => client.DownloadFile(remotePath, memoryStream), cancellationToken);
        memoryStream.Position = 0;
        return memoryStream;
    }

    public IEnumerable<ISftpFile> ListDirectory(SftpClient client, string remotePath)
    {
        return client.ListDirectory(remotePath);
    }
}
