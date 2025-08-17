using AFPS_Servers.Service.DTO;

public interface IServerService
{
    Task<List<ServerDto>> GetAllServersAsync();
    Task AddServerAsync(ServerDto server);
    Task DeleteServerAsync(int id);
    Task<ServerDto?> GetServerByIdAsync(int id);

}
