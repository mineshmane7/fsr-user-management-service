using Microsoft.AspNetCore.Authorization;

namespace FSR.UM.Core.Authorization
{
    /// <summary>
    /// Requirement that checks if user has a specific permission
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}
