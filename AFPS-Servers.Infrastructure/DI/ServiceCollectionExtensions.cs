using AFPS_Servers.Data;
using AFPS_Servers.Service.Config;
using AFPS_Servers.Service.Core;
using AFPS_Servers.Service.Interfaces;
using AFPS_Servers.Service.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace AFPS_Servers.Infrastructure.DI
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAfpsServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<EncryptionConfig>(config.GetRequiredSection("Encryption"));
            services.AddScoped<IEncryptionService, EncryptionService>();
            services.AddTransient<ISftpService, SftpService>();
            services.AddTransient<ISshService, SshService>();
            services.AddScoped<IServerService, ServerService>();
            services.AddScoped<IWarforkServerConfigService, WarforkServerConfigService>();
            services.AddScoped<IServerConfigService, ServerConfigService>();
            services.AddScoped<IPresetService, PresetService>();
            services.AddDbContext<ServersDbContext>(options =>
                options.UseSqlite("Data Source=servers.db"));

            return services;
        }
    }
}
