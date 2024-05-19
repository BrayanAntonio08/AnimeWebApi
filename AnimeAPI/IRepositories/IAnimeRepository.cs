using AnimeAPI.Models;

namespace AnimeAPI.IRepositories
{
    /// <summary>
    /// Defines a repository interface for performing operations specific to the <see cref="Anime"/> entity.
    /// </summary>
    public interface IAnimeRepository : IRepository<Anime>
    {
        /// <summary>
        /// Finds animes by their name.
        /// </summary>
        /// <param name="name">The name of the anime to search for.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumeration of animes that match the given name.</returns>
        Task<IEnumerable<Anime>> FindByName(string name);

        /// <summary>
        /// Adds an anime to a user's favourites.
        /// </summary>
        /// <param name="animeId">The identifier of the anime to add to favourites.</param>
        /// <param name="userId">The identifier of the user who is adding the favourite.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the anime was successfully added to favourites.</returns>
        Task<bool> AddFavourite(int animeId, int userId);

        /// <summary>
        /// Deletes an anime from a user's favourites.
        /// </summary>
        /// <param name="animeId">The identifier of the anime to remove from favourites.</param>
        /// <param name="userId">The identifier of the user who is removing the favourite.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the anime was successfully removed from favourites.</returns>
        Task<bool> DeleteFavourite(int animeId, int userId);

        /// <summary>
        /// Checks if an anime is in a user's favourites.
        /// </summary>
        /// <param name="animeId">The identifier of the anime to check.</param>
        /// <param name="userId">The identifier of the user to check for.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the anime is in the user's favourites.</returns>
        Task<bool> IsFavourite(int animeId, int userId);

        /// <summary>
        /// Retrieves all favourite animes of a user.
        /// </summary>
        /// <param name="userId">The identifier of the user whose favourites are to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumeration of the user's favourite animes.</returns>
        Task<IEnumerable<Anime>> GetFavourites(int userId);
    }

}
