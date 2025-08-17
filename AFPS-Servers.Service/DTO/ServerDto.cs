namespace AFPS_Servers.Service.DTO;
using System.ComponentModel.DataAnnotations;

public class ServerDto
{
    public int Id { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "The Name field is required.")]
    public string? Name { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "The Name field is required.")]
    public string? Host { get; set; }
    public string? Username { get; set; }

    public string? Password { get; set; }
}