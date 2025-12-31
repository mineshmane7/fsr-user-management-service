using FSR.UM.Core.Models;
using FSR.UM.Core.Models.DTOs;

namespace FSR.UM.Core.Interfaces
{
    public interface IUserManagementService
    {
        /// <summary>
        /// Create a new user with basic details and assign a role
        /// </summary>
        Task<UserWithRolesDto> CreateUserWithRoleAsync(CreateUserRequest request);
        Task<BulkCreateUserResult> BulkCreateUsersAsync(List<CreateUserRequest> users);
    }
}
