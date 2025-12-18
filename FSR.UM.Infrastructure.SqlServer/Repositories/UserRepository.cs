using FSR.UM.Core.Interfaces;
using FSR.UM.Core.Models;
using FSR.UM.Infrastructure.SqlServer.Db.AuthDb;
using Microsoft.EntityFrameworkCore;

namespace FSR.UM.Infrastructure.SqlServer.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _db;

        public UserRepository(AuthDbContext db)
        {
            _db = db;
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _db.Users.ToListAsync();
        }

        public async Task<User> AddAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }
    }
}
