using AFPS_Servers.Data;
using AFPS_Servers.Data.Entities;
using AFPS_Servers.Service.DTO;
using AFPS_Servers.Service.Interfaces;
using AFPS_Servers.Service.Config;
using Microsoft.EntityFrameworkCore;

public class ServerService : IServerService
{
    private readonly ServersDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly ISftpService _sftpService;

    public ServerService(ServersDbContext context, IEncryptionService encryptionService, ISftpService sftpService)
    {
        _context = context;
        _encryptionService = encryptionService;
        _sftpService = sftpService;
    }

    public async Task<List<ServerDto>> GetAllServersAsync()
    {
        return await _context.Servers
            .Select(s => new ServerDto
            {
                Id = s.Id,
                Name = s.Name,
                Host = s.Host
            })
            .ToListAsync();
    }
    public async Task<ServerDto?> GetServerByIdAsync(int id)
    {
        var server = await _context.Servers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (server == null)
            return null;

        return new ServerDto
        {
            Id = server.Id,
            Name = server.Name,
            Host = server.Host,
            Username = server.Username,
            Password = !string.IsNullOrEmpty(server.EncryptedPassword) ? _encryptionService.Decrypt(server.EncryptedPassword) : null
        };
    }
    public async Task AddServerAsync(ServerDto serverDto)
    {
        if (serverDto == null)
            throw new ArgumentNullException(nameof(serverDto), "ServerDto cannot be null.");
        if (!string.IsNullOrEmpty(serverDto.Name) && !string.IsNullOrEmpty(serverDto.Host))
        {
            var server = new Server
            {
                Username = "root", // Default username, can be changed later
                Name = serverDto.Name,
                Host = serverDto.Host
            };
            if (!string.IsNullOrEmpty(serverDto.Password))
            {
                var encryptedPass = _encryptionService.Encrypt(serverDto.Password);
                server.EncryptedPassword = encryptedPass;
            }

            await _context.AddAsync(server);
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new ArgumentException("Name and Host are required fields.", nameof(serverDto));
        }

    }
    public async Task DeleteServerAsync(int id)
    {
        var server = await _context.Servers.FindAsync(id);
        if (server == null)
            throw new Exception("Server not found.");

        _context.Servers.Remove(server);
        await _context.SaveChangesAsync();
    }

    public async Task<Dictionary<int, string>> BulkUploadMapsAsync(List<int> serverIds, List<FileUploadInfo> mapFiles)
    {
        var results = new Dictionary<int, string>();
        
        foreach (var serverId in serverIds)
        {
            try
            {
                var server = await GetServerByIdAsync(serverId);
                if (server == null)
                {
                    results[serverId] = "Server not found";
                    continue;
                }

                var sftpConfig = new SftpConfig
                {
                    Host = server.Host ?? string.Empty,
                    Port = 22,
                    Username = server.Username ?? string.Empty,
                    Password = server.Password ?? string.Empty
                };

                var uploadedCount = 0;
                
                using var sftpClient = _sftpService.Connect(sftpConfig.Host, sftpConfig.Port, sftpConfig.Username, sftpConfig.Password);
                
                foreach (var mapFile in mapFiles)
                {
                    var remotePath = $"/root/.local/share/warfork-2.1/basewf/{mapFile.Name}";
                    await _sftpService.UploadFileAsync(sftpClient, mapFile.Content, remotePath);
                    uploadedCount++;
                }
                
                _sftpService.Disconnect(sftpClient);
                
                results[serverId] = $"Successfully uploaded {uploadedCount} map files";
            }
            catch (Exception ex)
            {
                results[serverId] = $"Error: {ex.Message}";
            }
        }
        
        return results;
    }

    public async Task<Dictionary<int, string>> BulkUploadGametypesAsync(List<int> serverIds, List<FileUploadInfo> gametypeFiles)
    {
        var results = new Dictionary<int, string>();
        
        foreach (var serverId in serverIds)
        {
            try
            {
                var server = await GetServerByIdAsync(serverId);
                if (server == null)
                {
                    results[serverId] = "Server not found";
                    continue;
                }

                var sftpConfig = new SftpConfig
                {
                    Host = server.Host ?? string.Empty,
                    Port = 22,
                    Username = server.Username ?? string.Empty,
                    Password = server.Password ?? string.Empty
                };

                var uploadedCount = 0;
                
                using var sftpClient = _sftpService.Connect(sftpConfig.Host, sftpConfig.Port, sftpConfig.Username, sftpConfig.Password);
                
                foreach (var gametypeFile in gametypeFiles)
                {
                    var remotePath = $"/root/.local/share/warfork-2.1/basewf/{gametypeFile.Name}";
                    await _sftpService.UploadFileAsync(sftpClient, gametypeFile.Content, remotePath);
                    uploadedCount++;
                }
                
                _sftpService.Disconnect(sftpClient);
                
                results[serverId] = $"Successfully uploaded {uploadedCount} gametype files";
            }
            catch (Exception ex)
            {
                results[serverId] = $"Error: {ex.Message}";
            }
        }
        
        return results;
    }

    public async Task<Dictionary<int, string>> BulkRestartServersAsync(List<int> serverIds)
    {
        var results = new Dictionary<int, string>();
        
        foreach (var serverId in serverIds)
        {
            try
            {
                var server = await GetServerByIdAsync(serverId);
                if (server == null)
                {
                    results[serverId] = "Server not found";
                    continue;
                }

                var sshConfig = new SshConfig 
                { 
                    Host = server.Host, 
                    Port = 22, 
                    Username = server.Username ?? "root", 
                    Password = server.Password ?? "" 
                };

                var sshService = new SshService();
                
                await sshService.RunCommandAsync(sshConfig, "cd /root/server && ./Warfork.sh restart");
                
                results[serverId] = "Server restarted successfully";
            }
            catch (Exception ex)
            {
                results[serverId] = $"Error: {ex.Message}";
            }
        }
        
        return results;
    }

}
