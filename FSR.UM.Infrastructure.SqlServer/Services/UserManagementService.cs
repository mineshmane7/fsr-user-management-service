using FSR.UM.Core.Interfaces;
using FSR.UM.Core.Models;
using FSR.UM.Core.Models.DTOs;
using FSR.UM.Infrastructure.SqlServer.Db.AuthDb;
using Microsoft.EntityFrameworkCore;

namespace FSR.UM.Infrastructure.SqlServer.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserRepository _userRepo;
        private readonly AuthDbContext _authDb;

        public UserManagementService(IUserRepository userRepo, AuthDbContext authDb)
        {
            _userRepo = userRepo;
            _authDb = authDb;
        }

        public async Task<UserWithRolesDto> CreateUserWithRoleAsync(CreateUserRequest request)
        {
            // Validate that the user doesn't already exist
            var existingUser = await _userRepo.GetByEmailAsync(request.Email);
            if (existingUser != null)
                throw new InvalidOperationException($"User with email '{request.Email}' already exists");

            var existingUserName = await _userRepo.GetByUserNameAsync(request.UserName);
            if (existingUserName != null)
                throw new InvalidOperationException($"User with username '{request.UserName}' already exists");

            // Validate that the role exists
            var role = await _authDb.Roles
                .Include(r => r.RolePermissionAssignments)
                    .ThenInclude(rpa => rpa.Permission)
                .FirstOrDefaultAsync(r => r.Name == request.RoleName);

            if (role == null)
                throw new InvalidOperationException($"Role '{request.RoleName}' not found");

            // Create the user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                UserName = request.UserName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            // Add user to database
            var createdUser = await _userRepo.AddAsync(user);

            // Create user-role assignment
            var userRoleAssignment = new UserRoleAssignment
            {
                UserId = createdUser.Id,
                RoleId = role.Id,
                AssignedDate = DateTime.UtcNow
            };

            _authDb.UserRoleAssignments.Add(userRoleAssignment);
            await _authDb.SaveChangesAsync();

            // Fetch the user with roles to return
            var userWithRoles = await _userRepo.GetByIdWithRolesAsync(createdUser.Id);
            
            if (userWithRoles == null)
                throw new InvalidOperationException("Failed to retrieve created user");

            return userWithRoles;
        }
    }
}
