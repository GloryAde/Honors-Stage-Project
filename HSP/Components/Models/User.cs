using System.ComponentModel.DataAnnotations;

namespace HSP.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
