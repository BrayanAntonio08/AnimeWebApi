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

        public async Task<Anime?> GetByIdAsync(int id)
        {
            return await _context.Animes.FindAsync(id);
        }

        public async Task<IEnumerable<Anime>> GetAllAsync()
        {
            return await _context.Animes.ToListAsync();
        }

        public async Task<Anime> AddAsync(Anime anime)
        {
            _context.Animes.Add(anime);
            await _context.SaveChangesAsync();
            return anime;
        }

        public async Task<IEnumerable<Anime>> AddRangeAsync(IEnumerable<Anime> animes)
        {
            _context.Animes.AddRange(animes);
            await _context.SaveChangesAsync();
            return animes;
        }

        public async Task<Anime> UpdateAsync(Anime anime)
        {
            _context.Entry(anime).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return anime;
        }

        public async Task<bool> RemoveAsync(int id)
        {
            var anime = await _context.Animes.FindAsync(id);
            if (anime == null)
                return false;

            _context.Animes.Remove(anime);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Anime>> FindByName(string name)
        {
            return await _context.Animes.
                Where(a => 
                a.EnglishTitle.StartsWith(name) || 
                (a.JapaneseTitle != null && a.JapaneseTitle.StartsWith(name))
                ).ToListAsync();
        }

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
            }catch (Exception ex)
            {
                throw;
            }
            
        }

        public async Task<bool> DeleteFavourite(int animeId, int userId)
        {
            Favourite? favourite = await _context.Favourites.FindAsync([animeId, userId]);
            if (favourite == null)
                return false;

            _context.Favourites.Remove(favourite);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsFavourite(int animeId, int userId)
        {
            return await _context.Favourites.FindAsync([animeId, userId]) != null;
        }

        public async Task<IEnumerable<Anime>> GetFavourites(int userId)
        {
            return await _context.Favourites.Where(f => f.UserId == userId).Select(f => f.Anime).ToListAsync();
        }
    }
}
