using AnimeAPI.Models;

namespace AnimeAPI.IRepositories
{
    /// <summary>
    /// Defines a repository interface for performing operations specific to the <see cref="Role"/> entity.
    /// </summary>
    public interface IRoleRepository : IRepository<Role>
    {
        /// <summary>
        /// Checks if a role is an admin role.
        /// </summary>
        /// <param name="roleId">The identifier of the role to check.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the role is an admin role.</returns>
        Task<bool> IsAdminRole(int roleId);

        /// <summary>
        /// Checks if a role is a client role.
        /// </summary>
        /// <param name="roleId">The identifier of the role to check.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the role is a client role.</returns>
        Task<bool> IsClientRole(int roleId);
    }

}
