using FSR.UM.Core.Models;
using FSR.UM.Core.Models.DTOs;

namespace FSR.UM.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync();
        Task<UserWithRolesDto?> GetByIdWithRolesAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUserNameAsync(string username);
        Task<User?> GetByEmailOrUserNameAsync(string emailOrUsername);
        Task<User> AddAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<User?> GetByIdAsync(Guid id);
        Task SoftDeleteAsync(Guid id);
       // Task DeleteAsync(Guid id);
        Task<List<string>> GetUserPermissionsAsync(Guid userId);
    }
}
