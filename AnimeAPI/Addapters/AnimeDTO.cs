
using AnimeAPI.Models;

namespace AnimeAPI.Addapters
{
    /// <summary>
    /// The Anime DTO is just a simple object with the atributes of an anime, and in this case
    /// to avoid complexity the adapter functions are integrated as static methods in this class
    /// </summary>
    public class AnimeDTO
    {
        public int Id { get; set; }

        public string English_title { get; set; } = string.Empty;

        public string? Japanese_title { get; set; }

        public string? Trailer_url { get; set; }

        public string Image_url { get; set; } = string.Empty;

        public string Synopsis { get; set; } = string.Empty;

        public bool Airing { get; set; } = true;

        public int Episodes { get; set; }

        public decimal Score { get; set; }


        public static AnimeDTO Map(Anime data)
        {
            return new AnimeDTO()
            {
                Id = data.Id,
                Airing = data.Airing,
                English_title = data.EnglishTitle,
                Episodes = data.Episodes,
                Score = data.Score,
                Image_url = data.ImageUrl,
                Synopsis = data.Synopsis,
                Japanese_title = data.JapaneseTitle,
                Trailer_url = data.TrailerUrl,
            };
        }

        public static Anime Map(AnimeDTO data)
        {
            return new Anime()
            {
                Id = data.Id,
                Airing = data.Airing,
                EnglishTitle = data.English_title,
                Episodes = data.Episodes,
                Score = data.Score,
                ImageUrl = data.Image_url,
                Synopsis = data.Synopsis,
                JapaneseTitle = data.Japanese_title,
                TrailerUrl = data.Trailer_url,
            };
        }

        public static IEnumerable<Anime> Map(IEnumerable<AnimeDTO> data)
        {
            return data.Select(dto => Map(dto)).ToList();
        }

        public static IEnumerable<AnimeDTO> Map(IEnumerable<Anime> data)
        {
            return data.Select(dto => Map(dto)).ToList();
        }
    }
}
