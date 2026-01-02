using FSR.UM.Core.Interfaces;
using FSR.UM.Core.Models;
using FSR.UM.Core.Models.DTOs;
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
            // Return users without navigation properties to avoid circular references
            return await _db.Users
                .Where(u=>u.IsActive)
                .Select(u => new User
                {
                    Id = u.Id,
                    Email = u.Email,
                    UserName = u.UserName,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    PhoneNumber = u.PhoneNumber,
                    IsActive = u.IsActive,
                    CreatedDate = u.CreatedDate,
                    ModifiedDate = u.ModifiedDate,
                    PasswordHash = u.PasswordHash
                })
                .ToListAsync();
        }

        public async Task<UserWithRolesDto?> GetByIdWithRolesAsync(Guid id)
        {
            var user = await _db.Users
                .Include(u => u.UserRoleAssignments)
                    .ThenInclude(ura => ura.Role)
                        .ThenInclude(r => r.RolePermissionAssignments)
                            .ThenInclude(rpa => rpa.Permission)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return null;

            // Map to DTO
            var userDto = new UserWithRolesDto
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                ModifiedDate = user.ModifiedDate,
                Roles = user.UserRoleAssignments.Select(ura => new RoleDto
                {
                    Id = ura.Role.Id,
                    Name = ura.Role.Name,
                    Description = ura.Role.Description,
                    Permissions = ura.Role.RolePermissionAssignments.Select(rpa => new PermissionDto
                    {
                        Id = rpa.Permission.Id,
                        Name = rpa.Permission.Name,
                        Description = rpa.Permission.Description
                    }).ToList()
                }).ToList(),
                Permissions = user.UserRoleAssignments
                    .SelectMany(ura => ura.Role.RolePermissionAssignments.Select(rpa => rpa.Permission.Name))
                    .Distinct()
                    .ToList()
            };

            return userDto;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _db.Users
                .Include(u => u.UserRoleAssignments)
                    .ThenInclude(ura => ura.Role)
                        .ThenInclude(r => r.RolePermissionAssignments)
                            .ThenInclude(rpa => rpa.Permission)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByUserNameAsync(string username)
        {
            return await _db.Users
                .Include(u => u.UserRoleAssignments)
                    .ThenInclude(ura => ura.Role)
                        .ThenInclude(r => r.RolePermissionAssignments)
                            .ThenInclude(rpa => rpa.Permission)
                .FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<User?> GetByEmailOrUserNameAsync(string emailOrUsername)
        {
            return await _db.Users
                .Include(u => u.UserRoleAssignments)
                    .ThenInclude(ura => ura.Role)
                        .ThenInclude(r => r.RolePermissionAssignments)
                            .ThenInclude(rpa => rpa.Permission)
                .FirstOrDefaultAsync(u => u.Email == emailOrUsername || u.UserName == emailOrUsername);
        }

        public async Task<User> AddAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            user.ModifiedDate = DateTime.UtcNow;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        }


        public async Task SoftDeleteAsync(Guid id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
                throw new InvalidOperationException("User not found");

            user.IsActive = false;
            user.ModifiedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();

        }

        public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
        {
            var permissions = await _db.Users
                .Where(u => u.Id == userId)
                .SelectMany(u => u.UserRoleAssignments
                    .SelectMany(ura => ura.Role.RolePermissionAssignments
                        .Select(rpa => rpa.Permission.Name)))
                .Distinct()
                .ToListAsync();

            return permissions;
        }
    }
}
