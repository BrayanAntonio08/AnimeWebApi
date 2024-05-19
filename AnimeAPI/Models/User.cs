using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimeAPI.Models
{
    [Table("User", Schema = "Auth")]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Password { get; set; } = string.Empty;

        [ForeignKey("Role")]
        public int RoleId { get; set; }

        public Role? Role { get; set; }
    }
}
