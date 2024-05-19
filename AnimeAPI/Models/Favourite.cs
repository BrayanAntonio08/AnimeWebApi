using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimeAPI.Models
{
    [Table("Favourites", Schema = "Domain")]
    [PrimaryKey(nameof(AnimeId), nameof(UserId))]
    public class Favourite
    {
        [ForeignKey(nameof(Anime))]
        public int AnimeId { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        public Anime Anime { get; set; }

        public User User { get; set; }
    }
}
