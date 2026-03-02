namespace AFPS_Servers.Service.DTO
{
    public class ParsedConfig
    {
        public List<CvarEntry> Cvars { get; set; } = new();
        public List<ConfigEntry> Execs { get; set; } = new();
        public List<ConfigEntry> Aliases { get; set; } = new();
        public List<ConfigEntry> Binds { get; set; } = new();
    }

    public class CvarEntry
    {
        public string Directive { get; set; } = "set";
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class ConfigEntry
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }
}
