using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFPS_Servers.Data.Entities
{
    public class Server
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public required string Name { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string Host { get; set; } = string.Empty;

        [Required]
        public int Port { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, Column("Password")]
        [MaxLength(512)]
        public string EncryptedPassword { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
