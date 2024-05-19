using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimeAPI.Models
{
    [Table("Anime", Schema = "Domain")]
    public class Anime
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string EnglishTitle { get; set; } = string.Empty;

        public string? JapaneseTitle { get; set; }

        public string? TrailerUrl { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        public string Synopsis { get; set; } = string.Empty;

        public bool Airing { get; set; } = true;

        public int Episodes { get; set; }

        public decimal Score { get; set; }

    }
}
