using AnimeAPI.Models;

namespace AnimeAPI.IRepositories
{
    public interface IAnimeRepository:IRepository<Anime>
    {
        Task<IEnumerable<Anime>> FindByName(string name);
        Task<bool> AddFavourite(int animeId, int userId);
        Task<bool> DeleteFavourite(int animeId, int userId);
        Task<bool> IsFavourite(int animeId, int userId);
        Task<IEnumerable<Anime>> GetFavourites(int userId);
    }
}
