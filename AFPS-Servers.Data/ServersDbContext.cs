using AFPS_Servers.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AFPS_Servers.Data
{
    public class ServersDbContext : DbContext
    {
        public DbSet<Server> Servers { get; set; }
        public DbSet<Config> Configs { get; set; }
        public DbSet<Preset> Presets { get; set; }

        public ServersDbContext(DbContextOptions<ServersDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Config>()
                .HasOne(c => c.Server)
                .WithMany()
                .HasForeignKey(c => c.ServerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Config>()
                .HasIndex(c => c.ServerId)
                .IsUnique();
        }
    }
}
