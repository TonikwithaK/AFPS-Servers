using AFPS_Servers.Service.DTO;

public class FileUploadInfo
{
    public string Name { get; set; } = "";
    public Stream Content { get; set; } = null!;
    public long Size { get; set; }
}

public interface IServerService
{
    Task<List<ServerDto>> GetAllServersAsync();
    Task AddServerAsync(ServerDto server);
    Task DeleteServerAsync(int id);
    Task<ServerDto?> GetServerByIdAsync(int id);
    Task<Dictionary<int, string>> BulkUploadMapsAsync(List<int> serverIds, List<FileUploadInfo> mapFiles);
    Task<Dictionary<int, string>> BulkUploadGametypesAsync(List<int> serverIds, List<FileUploadInfo> gametypeFiles);
    Task<Dictionary<int, string>> BulkRestartServersAsync(List<int> serverIds);

}
