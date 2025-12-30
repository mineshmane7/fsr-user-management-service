using FSR.UM.Core.Interfaces;
using FSR.UM.Core.Models;
using FSR.UM.Infrastructure.SqlServer.Db.AuthDb;
using Microsoft.EntityFrameworkCore;

namespace FSR.UM.Infrastructure.SqlServer.Repositories
{
    public class RegisteredPingUserRepository : IRegisteredPingUserRepository
    {
        private readonly AuthDbContext _db;

        public RegisteredPingUserRepository(AuthDbContext db)
        {
            _db = db;
        }

        public async Task<List<RegisteredPingUser>> GetAllAsync()
        {
            return await _db.RegisteredPingUsers
                .OrderBy(rpu => rpu.Email)
                .ToListAsync();
        }

        public async Task<RegisteredPingUser?> GetByIdAsync(int id)
        {
            return await _db.RegisteredPingUsers.FindAsync(id);
        }

        public async Task<RegisteredPingUser?> GetByEmailAsync(string email)
        {
            return await _db.RegisteredPingUsers
                .FirstOrDefaultAsync(rpu => rpu.Email == email);
        }

        public async Task<bool> IsEmailRegisteredAsync(string email)
        {
            return await _db.RegisteredPingUsers
                .AnyAsync(rpu => rpu.Email == email && rpu.IsActive);
        }

        public async Task<RegisteredPingUser> AddAsync(RegisteredPingUser registeredPingUser)
        {
            _db.RegisteredPingUsers.Add(registeredPingUser);
            await _db.SaveChangesAsync();
            return registeredPingUser;
        }

        public async Task<RegisteredPingUser> UpdateAsync(RegisteredPingUser registeredPingUser)
        {
            _db.RegisteredPingUsers.Update(registeredPingUser);
            await _db.SaveChangesAsync();
            return registeredPingUser;
        }

        public async Task DeleteAsync(int id)
        {
            var registeredPingUser = await _db.RegisteredPingUsers.FindAsync(id);
            if (registeredPingUser != null)
            {
                _db.RegisteredPingUsers.Remove(registeredPingUser);
                await _db.SaveChangesAsync();
            }
        }
    }
}
