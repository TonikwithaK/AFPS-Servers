namespace AFPS_Servers.Service.Config
{
    public class SftpConfig
    {
        public string Host { get; set; } = default!;
        public int Port { get; set; } = 22;
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
