using Domain.Entities;

namespace Application.Interfaces.Repository
{
    public interface IAdminRepository : IGenericRepository<Admin>
    {
        Task<Admin?> GetByEmailAsync(string email);
        Task<Admin?> GetByUserNameAsync(string userName);
    }
}
