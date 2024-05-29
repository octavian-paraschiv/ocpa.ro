using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using ocpa.ro.api.Helpers.Authentication;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ocpa.ro.api.Policies
{
    public class AdminHandler : AuthorizationHandler<RolesAuthorizationRequirement>
    {
        private readonly IAuthHelper _authHelper;

        public AdminHandler(IAuthHelper authHelper)
        {
            _authHelper = authHelper;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RolesAuthorizationRequirement requirement)
        {
            bool ok = false;

            var loginId = context?.User.Claims.FirstOrDefault()?.Value;
            if (loginId?.Length > 0)
            {
                var user = _authHelper.GetUser(loginId);
                var userType = user?.Type.ToString();

                if (userType?.Length > 0)
                {
                    if (requirement?.AllowedRoles?.Any() ?? false)
                    {
                        // Specific roles requested, check if the user has a matching role
                        ok = requirement.AllowedRoles.Contains(userType, StringComparer.OrdinalIgnoreCase);
                    }
                    else
                    {
                        // No specific role requested, just needs to be a registered user
                        ok = true;
                    }
                }
            }

            if (ok)
            {
                context?.Succeed(requirement);
                return Task.CompletedTask;
            }

            return requirement.HandleAsync(context);
        }

    }
}
