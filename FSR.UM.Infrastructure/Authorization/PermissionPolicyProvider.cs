using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace FSR.UM.Infrastructure.Authorization
{
    /// <summary>
    /// Custom authorization policy provider that creates policies dynamically based on permission names
    /// </summary>
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
        private const string PermissionPrefix = "Permission.";

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return _fallbackPolicyProvider.GetDefaultPolicyAsync();
        }

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return _fallbackPolicyProvider.GetFallbackPolicyAsync();
        }

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // Check if policy name starts with our permission prefix
            if (policyName.StartsWith(PermissionPrefix, StringComparison.OrdinalIgnoreCase))
            {
                // Extract permission name
                var permission = policyName.Substring(PermissionPrefix.Length);

                // Build policy
                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new FSR.UM.Core.Authorization.PermissionRequirement(permission))
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            // Fall back to default policy provider
            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}
