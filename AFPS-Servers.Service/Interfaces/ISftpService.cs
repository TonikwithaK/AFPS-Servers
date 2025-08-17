// Interface
using Renci.SshNet;
using Renci.SshNet.Sftp;

public interface ISftpService
{
    SftpClient Connect(string host, int port, string username, string password);
    void Disconnect(SftpClient client);
    void UploadFile(SftpClient client, string localPath, string remotePath);
    Task UploadFileAsync(SftpClient client, string localPath, string remotePath, CancellationToken cancellationToken = default);
    Task UploadFileAsync(SftpClient client, Stream inputStream, string remotePath, CancellationToken cancellationToken = default);

    Task<Stream> DownloadFileAsync(SftpClient client, string remotePath, CancellationToken cancellationToken = default);
    IEnumerable<ISftpFile> ListDirectory(SftpClient client, string remotePath);
}
