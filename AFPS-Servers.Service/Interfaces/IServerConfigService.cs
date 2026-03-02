namespace AFPS_Servers.Service.Interfaces
{
    public interface IServerConfigService
    {
        /// <summary>Gets the stored config content. Pass null for the global default preset.</summary>
        Task<string?> GetConfigAsync(int? serverId);

        /// <summary>Upserts the config content. Pass null serverId to save the global default preset.</summary>
        Task SaveConfigAsync(int? serverId, string content);

        /// <summary>Deletes the server-specific config preset when a server is removed.</summary>
        Task DeleteConfigAsync(int serverId);
    }
}
