using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimeAPI.Models
{
    [Table("Role", Schema = "Auth")]
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(30)]
        public string Name { get; set; } = string.Empty;

        public ICollection<User> Users { get; set; }
    }
}
