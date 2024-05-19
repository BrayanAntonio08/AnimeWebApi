using AnimeAPI.Models;

namespace AnimeAPI.IRepositories
{
    /// <summary>
    /// Defines a repository interface for performing operations specific to the <see cref="User"/> entity.
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>
        /// Logs in a user by verifying their credentials.
        /// </summary>
        /// <param name="user">The <see cref="User"/> entity containing the login credentials.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="User"/> entity if the login is successful; otherwise, null.</returns>
        Task<User> Login(User user);
    }

}
