using FSR.UM.Core.Models;

namespace FSR.UM.Core.Interfaces
{
    /// <summary>
    /// Repository interface for managing Ping registered users
    /// </summary>
    public interface IRegisteredPingUserRepository
    {
        Task<List<RegisteredPingUser>> GetAllAsync();
        Task<RegisteredPingUser?> GetByIdAsync(int id);
        Task<RegisteredPingUser?> GetByEmailAsync(string email);
        Task<bool> IsEmailRegisteredAsync(string email);
        Task<RegisteredPingUser> AddAsync(RegisteredPingUser registeredPingUser);
        Task<RegisteredPingUser> UpdateAsync(RegisteredPingUser registeredPingUser);
        Task DeleteAsync(int id);
    }
}
