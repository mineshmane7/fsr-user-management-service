using FSR.UM.Core.Interfaces;
using FSR.UM.Core.Interfaces.Auth;
using FSR.UM.Core.Models.Auth;
using Microsoft.Extensions.Configuration;

namespace FSR.UM.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository userRepo, ITokenService tokenService, IConfiguration config)
        {
            _userRepo = userRepo;
            _tokenService = tokenService;
            _config = config;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            // Support login with email or username
            var user = await _userRepo.GetByEmailOrUserNameAsync(request.Email);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid credentials");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("User account is inactive");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");

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
