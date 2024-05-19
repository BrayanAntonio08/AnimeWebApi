using AnimeAPI.Data;
using AnimeAPI.IRepositories;
using AnimeAPI.Models;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AnimeAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private AnimeDBContext _context;
        public UserRepository(AnimeDBContext context)
        {
            _context = context;
        }

        public async Task<User> AddAsync(User entity)
        {
            if (_context.Users.Any(u => u.Username.Equals(entity.Username)))
            {
                throw new Exception("Username already used, try a new one");
            }

            //before adding the user password must be encrypt
            string encrypted = ComputeSha256Hash(entity.Password);
            entity.Password = encrypted;

            _context.Users.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<User>> AddRangeAsync(IEnumerable<User> entities)
        {
            _context.Users.AddRange(entities);
            await _context.SaveChangesAsync();
            return entities;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<bool> RemoveAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User> UpdateAsync(User entity)
        {
            var user = await _context.Users.FindAsync(entity.UserId);
            if(user.Password != entity.Password)
                entity.Password = ComputeSha256Hash(entity.Password);

            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Function made by ChatGPT to encrypt the given string by SHA256 algorithm
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256 object
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash returns byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public async Task<User> Login(User user)
        {
            string pass = ComputeSha256Hash(user.Password);

            User? result = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username && u.Password == pass);

            return result;
        }
    }
}
