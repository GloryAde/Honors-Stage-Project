using System.ComponentModel.DataAnnotations;

namespace HSP.Models
{
    public class Asset
    {
        [Key]  // This tells EF Core this is the PK
        public int ItemId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;
    }
}
