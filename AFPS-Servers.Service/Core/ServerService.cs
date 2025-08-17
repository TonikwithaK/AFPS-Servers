using AFPS_Servers.Data;
using AFPS_Servers.Data.Entities;
using AFPS_Servers.Service.DTO;
using AFPS_Servers.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

public class ServerService : IServerService
{
    private readonly ServersDbContext _context;
    private readonly IEncryptionService _encryptionService;

    public ServerService(ServersDbContext context, IEncryptionService encryptionService)
    {
        _context = context;
        _encryptionService = encryptionService;
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

}
