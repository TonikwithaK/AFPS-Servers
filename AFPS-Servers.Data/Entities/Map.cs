using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AFPS_Servers.Data.Entities
{
    public class Map
    {
        [Key]
        public int MapId { get; set; }

        [Required, MaxLength(100)]
        public string? Name { get; set; }

        public ICollection<ServerMap>? ServerMaps { get; set; }
    }
}