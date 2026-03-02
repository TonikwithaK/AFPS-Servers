using AFPS_Servers.Data;
using AFPS_Servers.Data.Entities;
using AFPS_Servers.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AFPS_Servers.Service.Core
{
    public class ServerConfigService : IServerConfigService
    {
        private readonly ServersDbContext _db;

        public ServerConfigService(ServersDbContext db)
        {
            _db = db;
        }

        public async Task<string?> GetConfigAsync(int? serverId)
        {
            var config = await _db.Configs
                .FirstOrDefaultAsync(c => c.ServerId == serverId);
            return config?.Content;
        }

        public async Task SaveConfigAsync(int? serverId, string content)
        {
            var config = await _db.Configs
                .FirstOrDefaultAsync(c => c.ServerId == serverId);

            if (config == null)
            {
                _db.Configs.Add(new AFPS_Servers.Data.Entities.Config
                {
                    ServerId = serverId,
                    Content = content
                });
            }
            else
            {
                config.Content = content;
                config.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteConfigAsync(int serverId)
        {
            var config = await _db.Configs
                .FirstOrDefaultAsync(c => c.ServerId == serverId);

            if (config != null)
            {
                _db.Configs.Remove(config);
                await _db.SaveChangesAsync();
            }
        }
    }
}
