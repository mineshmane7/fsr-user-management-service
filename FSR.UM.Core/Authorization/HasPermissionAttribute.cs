using Microsoft.AspNetCore.Authorization;

namespace FSR.UM.Core.Authorization
{
    /// <summary>
    /// Custom authorize attribute for permission-based authorization
    /// Usage: [HasPermission("Create")]
    /// </summary>
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        private const string PermissionPrefix = "Permission.";

        public HasPermissionAttribute(string permission)
        {
            Policy = $"{PermissionPrefix}{permission}";
        }
    }
}
