using FSR.UM.Core.Interfaces;
using FSR.UM.Core.Interfaces.Auth;
using FSR.UM.Core.Models.Auth;
using FSR.UM.Infrastructure.SqlServer.Db.AuthDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FSR.UM.Infrastructure.SqlServer.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _config;
        private readonly AuthDbContext _authDb;

        public AuthService(IUserRepository userRepo, ITokenService tokenService, IConfiguration config, AuthDbContext authDb)
        {
            _userRepo = userRepo;
            _tokenService = tokenService;
            _config = config;
            _authDb = authDb;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            // ? STEP 1: Check if the email is registered with Ping Identity
            var registeredPingUser = await _authDb.RegisteredPingUsers
                .FirstOrDefaultAsync(rpu => rpu.Email == request.Email && rpu.IsActive);

            if (registeredPingUser == null)
            {
                // Check if email exists but is inactive
                var inactivePingUser = await _authDb.RegisteredPingUsers
                    .FirstOrDefaultAsync(rpu => rpu.Email == request.Email && !rpu.IsActive);
                
                if (inactivePingUser != null)
                {
                    throw new UnauthorizedAccessException(
                        "Your Ping Identity registration has been deactivated. Please contact your administrator for assistance.");
                }

                throw new UnauthorizedAccessException(
                    "Access denied. Your email is not registered with Ping Identity. Please contact your administrator to register your email.");
            }

            // ? STEP 2: Check if user exists in the system (support login with email or username)
            var user = await _userRepo.GetByEmailOrUserNameAsync(request.Email);

            if (user == null)
            {
                throw new UnauthorizedAccessException(
                    "No account found with this email. Please contact your administrator to create your user account.");
            }

            // ? STEP 3: Check if user account is active
            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException(
                    "Your user account has been deactivated. Please contact your administrator for assistance.");
            }

            // ? STEP 4: Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException(
                    "Invalid password. Please check your credentials and try again.");
            }

            // ? STEP 5: Generate JWT token
            var token = _tokenService.GenerateToken(user);

            // Get token expiration from config
            var expirationMinutes = int.Parse(_config["Jwt:ExpirationInMinutes"] ?? "60");

            // Get roles and permissions from UserRoleAssignments
            var roles = user.UserRoleAssignments.Select(ura => ura.Role.Name).ToList();
            var permissions = user.UserRoleAssignments
                .SelectMany(ura => ura.Role.RolePermissionAssignments
                    .Select(rpa => rpa.Permission.Name))
                .Distinct()
                .ToList();

            return new LoginResponse
            {
                AccessToken = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles,
                Permissions = permissions
            };
        }
    }
}
