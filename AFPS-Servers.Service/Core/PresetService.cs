using AFPS_Servers.Data;
using AFPS_Servers.Data.Entities;
using AFPS_Servers.Service.DTO;
using AFPS_Servers.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AFPS_Servers.Service.Core
{
    public class PresetService : IPresetService
    {
        private readonly ServersDbContext _db;

        public PresetService(ServersDbContext db)
        {
            _db = db;
        }

        public async Task<IList<PresetDto>> GetAllPresetsAsync()
        {
            return await _db.Presets
                .OrderByDescending(p => p.UpdatedAt)
                .Select(p => new PresetDto { Id = p.Id, Name = p.Name, UpdatedAt = p.UpdatedAt })
                .ToListAsync();
        }

        public async Task<string?> GetPresetContentAsync(int id)
        {
            var preset = await _db.Presets.FindAsync(id);
            return preset?.Content;
        }

        public async Task SavePresetAsync(string name, string content)
        {
            var preset = new Preset
            {
                Name = name,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Presets.Add(preset);
            await _db.SaveChangesAsync();
        }

        public async Task DeletePresetAsync(int id)
        {
            var preset = await _db.Presets.FindAsync(id);
            if (preset != null)
            {
                _db.Presets.Remove(preset);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<string> GeneratePresetNameAsync(string baseName)
        {
            // Strip .cfg extension if present for base comparison
            var bare = baseName.EndsWith(".cfg", StringComparison.OrdinalIgnoreCase)
                ? baseName[..^4]
                : baseName;

            var candidate = $"{bare}.cfg";
            if (!await _db.Presets.AnyAsync(p => p.Name == candidate))
                return candidate;

            for (int i = 1; i < 1000; i++)
            {
                candidate = $"{bare}-{i}.cfg";
                if (!await _db.Presets.AnyAsync(p => p.Name == candidate))
                    return candidate;
            }

            return $"{bare}-{DateTime.UtcNow.Ticks}.cfg";
        }
    }
}
