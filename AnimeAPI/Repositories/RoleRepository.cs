using AnimeAPI.Data;
using AnimeAPI.IRepositories;
using AnimeAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace AnimeAPI.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private AnimeDBContext _context;
        public RoleRepository(AnimeDBContext context) {
            _context = context;
        }

        public async Task<Role> AddAsync(Role entity)
        {
            _context.Roles.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<Role>> AddRangeAsync(IEnumerable<Role> entities)
        {
            _context.AddRange(entities);
            await _context.SaveChangesAsync();  
            return entities;
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _context.Roles.ToListAsync();  
        }

        public async Task<Role?> GetByIdAsync(int id)
        {
            return await _context.Roles.FindAsync(id);
        }

        public async Task<bool> IsAdminRole(int RoleId)
        {
            return await _context.Roles.FirstOrDefaultAsync(x => x.RoleId == RoleId && x.Name == "Admin") != null;
        }

        public async Task<bool> IsClientRole(int RoleId)
        {
            return await _context.Roles.FirstOrDefaultAsync(x => x.RoleId == RoleId && x.Name == "Client") != null;
        }

        public async Task<bool> RemoveAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if( role != null)
            {
                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<Role> UpdateAsync(Role entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}
