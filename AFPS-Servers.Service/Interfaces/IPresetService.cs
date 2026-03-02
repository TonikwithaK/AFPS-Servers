using AFPS_Servers.Service.DTO;

namespace AFPS_Servers.Service.Interfaces
{
    public interface IPresetService
    {
        Task<IList<PresetDto>> GetAllPresetsAsync();
        Task<string?> GetPresetContentAsync(int id);
        Task SavePresetAsync(string name, string content);
        Task DeletePresetAsync(int id);
        Task<string> GeneratePresetNameAsync(string baseName);
    }
}
