using AFPS_Servers.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AFPS_Servers.Data
{
    public class ServersDbContext : DbContext
    {
        public DbSet<Server> Servers { get; set; }
        public ServersDbContext(DbContextOptions<ServersDbContext> options)
            : base(options)
        {
        }
    }
}