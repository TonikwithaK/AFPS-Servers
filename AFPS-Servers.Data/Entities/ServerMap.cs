using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFPS_Servers.Data.Entities
{
    public class ServerMap
    {
        [Key, Column(Order = 0)]
        public int ServerId { get; set; }
        [Required]
        public Server? Server { get; set; }
        public int MapId { get; set; }
        public Map? Map { get; set; }
    }
}
