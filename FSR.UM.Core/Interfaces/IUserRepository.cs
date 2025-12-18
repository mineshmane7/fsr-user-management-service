using FSR.UM.Core.Models;

namespace FSR.UM.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync();
        Task<User> AddAsync(User user);
    }
}
