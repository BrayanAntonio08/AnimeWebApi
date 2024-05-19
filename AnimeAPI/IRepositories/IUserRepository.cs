using AnimeAPI.Models;

namespace AnimeAPI.IRepositories
{
    public interface IUserRepository:IRepository<User>
    {
        Task<User> Login(User user);
    }
}
