using Microsoft.AspNetCore.Authorization;

namespace FSR.UM.Infrastructure.Authorization
{
    /// <summary>
    /// Authorization handler that checks if user has the required permission in their claims
    /// </summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<FSR.UM.Core.Authorization.PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            FSR.UM.Core.Authorization.PermissionRequirement requirement)
        {
            // Get all permission claims
            var permissions = context.User.Claims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .ToList();

            // Check if user has the required permission
            if (permissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
