namespace AFPS_Servers.Service.Config;
public class SshConfig
{
    public string Host { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // or use SecureString
    public int Port { get; set; } = 22; // default SSH port
}