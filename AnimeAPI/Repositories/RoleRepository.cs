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

        /// <summary>
        /// Adds a new role to the database.
        /// </summary>
        /// <param name="entity">The role to add.</param>
        /// <returns>The added role.</returns>
        public async Task<Role> AddAsync(Role entity)
        {
            _context.Roles.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Adds a range of new roles to the database.
        /// </summary>
        /// <param name="entities">The roles to add.</param>
        /// <returns>The added roles.</returns>
        public async Task<IEnumerable<Role>> AddRangeAsync(IEnumerable<Role> entities)
        {
            _context.AddRange(entities);
            await _context.SaveChangesAsync();
            return entities;
        }

        /// <summary>
        /// Gets all roles.
        /// </summary>
        /// <returns>A list of all roles.</returns>
        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        /// <summary>
        /// Gets a role by its ID.
        /// </summary>
        /// <param name="id">The ID of the role.</param>
        /// <returns>The role with the specified ID, or null if not found.</returns>
        public async Task<Role?> GetByIdAsync(int id)
        {
            return await _context.Roles.FindAsync(id);
        }

        /// <summary>
        /// Checks if a role is an admin role.
        /// </summary>
        /// <param name="RoleId">The ID of the role.</param>
        /// <returns>True if the role is an admin role, false otherwise.</returns>
        public async Task<bool> IsAdminRole(int RoleId)
        {
            return await _context.Roles.FirstOrDefaultAsync(x => x.RoleId == RoleId && x.Name == "Admin") != null;
        }

        /// <summary>
        /// Checks if a role is a client role.
        /// </summary>
        /// <param name="RoleId">The ID of the role.</param>
        /// <returns>True if the role is a client role, false otherwise.</returns>
        public async Task<bool> IsClientRole(int RoleId)
        {
            return await _context.Roles.FirstOrDefaultAsync(x => x.RoleId == RoleId && x.Name == "Client") != null;
        }

        /// <summary>
        /// Removes a role from the database by its ID.
        /// </summary>
        /// <param name="id">The ID of the role to remove.</param>
        /// <returns>True if the role was removed, false if not found.</returns>
        public async Task<bool> RemoveAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role != null)
            {
                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates an existing role in the database.
        /// </summary>
        /// <param name="entity">The role to update.</param>
        /// <returns>The updated role.</returns>
        public async Task<Role> UpdateAsync(Role entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}
