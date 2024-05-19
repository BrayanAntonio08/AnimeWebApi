using AnimeAPI.Data;
using AnimeAPI.IRepositories;
using AnimeAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace AnimeAPI.Repositories
{
    public class AnimeRepository : IAnimeRepository
    {
        public AnimeDBContext _context;

        public AnimeRepository(AnimeDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets an anime by its ID.
        /// </summary>
        /// <param name="id">The ID of the anime.</param>
        /// <returns>The anime with the specified ID, or null if not found.</returns>
        public async Task<Anime?> GetByIdAsync(int id)
        {
            return await _context.Animes.FindAsync(id);
        }

        /// <summary>
        /// Gets all animes.
        /// </summary>
        /// <returns>A list of all animes.</returns>
        public async Task<IEnumerable<Anime>> GetAllAsync()
        {
            return await _context.Animes.ToListAsync();
        }

        /// <summary>
        /// Adds a new anime to the database.
        /// </summary>
        /// <param name="anime">The anime to add.</param>
        /// <returns>The added anime.</returns>
        public async Task<Anime> AddAsync(Anime anime)
        {
            _context.Animes.Add(anime);
            await _context.SaveChangesAsync();
            return anime;
        }

        /// <summary>
        /// Updates an existing anime in the database.
        /// </summary>
        /// <param name="anime">The anime to update.</param>
        /// <returns>The updated anime.</returns>
        public async Task<Anime> UpdateAsync(Anime anime)
        {
            _context.Entry(anime).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return anime;
        }

        /// <summary>
        /// Removes an anime from the database by its ID.
        /// </summary>
        /// <param name="id">The ID of the anime to remove.</param>
        /// <returns>True if the anime was removed, false if not found.</returns>
        public async Task<bool> RemoveAsync(int id)
        {
            var anime = await _context.Animes.FindAsync(id);
            if (anime == null)
                return false;

            _context.Animes.Remove(anime);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Finds animes by their name, searching in both English and Japanese titles.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <returns>A list of animes that match the search criteria.</returns>
        public async Task<IEnumerable<Anime>> FindByName(string name)
        {
            return await _context.Animes
                .Where(a =>
                    a.EnglishTitle.StartsWith(name) ||
                    (a.JapaneseTitle != null && a.JapaneseTitle.StartsWith(name))
                ).ToListAsync();
        }

        /// <summary>
        /// Adds an anime to the user's list of favourite animes.
        /// </summary>
        /// <param name="animeId">The ID of the anime to add to favourites.</param>
        /// <param name="userId">The ID of the user adding the favourite.</param>
        /// <returns>True if the favourite was added successfully, false otherwise.</returns>
        public async Task<bool> AddFavourite(int animeId, int userId)
        {
            try
            {
                Favourite favourite = new Favourite()
                {
                    AnimeId = animeId,
                    UserId = userId
                };
                // validate user is client
                User? user = await _context.Users.FindAsync(userId);
                if (user == null) return false;
                if (_context.Roles.Find(user.RoleId).Name.Equals("Admin")) return false;

                _context.Favourites.Add(favourite);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Removes an anime from the user's list of favourite animes.
        /// </summary>
        /// <param name="animeId">The ID of the anime to remove from favourites.</param>
        /// <param name="userId">The ID of the user removing the favourite.</param>
        /// <returns>True if the favourite was removed successfully, false otherwise.</returns>
        public async Task<bool> DeleteFavourite(int animeId, int userId)
        {
            Favourite? favourite = await _context.Favourites.FindAsync(new object[] { animeId, userId });
            if (favourite == null)
                return false;

            _context.Favourites.Remove(favourite);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Checks if an anime is in the user's list of favourite animes.
        /// </summary>
        /// <param name="animeId">The ID of the anime.</param>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>True if the anime is a favourite, false otherwise.</returns>
        public async Task<bool> IsFavourite(int animeId, int userId)
        {
            return await _context.Favourites.FindAsync(new object[] { animeId, userId }) != null;
        }

        /// <summary>
        /// Gets the list of favourite animes for a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of the user's favourite animes.</returns>
        public async Task<IEnumerable<Anime>> GetFavourites(int userId)
        {
            return await _context.Favourites.Where(f => f.UserId == userId).Select(f => f.Anime).ToListAsync();
        }
    }
}
