using AnimeAPI.Models;

namespace AnimeAPI.IRepositories
{
    public interface IRoleRepository:IRepository<Role>
    {
        Task<bool> IsAdminRole(int RoleId);
        Task<bool> IsClientRole(int RoleId);
    }
}
