using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ocpa.ro.api.Helpers.Authentication;
using ocpa.ro.api.Models.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ocpa.ro.api.Policies
{
    public class AuthorizePolicy : AuthorizationHandler<RolesAuthorizationRequirement>
    {
        private readonly IAuthHelper _authHelper;
        private readonly IWebHostEnvironment _hostingEnvironment = null;
        private readonly AuthConfig _authConfig;

        public AuthorizePolicy(IAuthHelper authHelper, IWebHostEnvironment hostingEnvironment, IOptions<AuthConfig> authConfigOptions)
        {
            _authHelper = authHelper;
            _hostingEnvironment = hostingEnvironment;
            _authConfig = authConfigOptions.Value;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RolesAuthorizationRequirement requirement)
        {
            bool allow = (_hostingEnvironment?.IsDevelopment() ?? false) && (_authConfig?.Disabled ?? false);

            if (!allow)
            {
                var loginId = context?.User.Claims.FirstOrDefault()?.Value;
                if (loginId?.Length > 0)
                {
                    var user = _authHelper.GetUser(loginId);
                    var userType = _authHelper.GetUserType(id: user.Type);

                    if (userType != null)
                    {
                        if (requirement?.AllowedRoles?.Any() ?? false)
                        {
                            // Specific roles requested, check if the user has a matching role
                            allow = requirement.AllowedRoles.Contains(userType.Code, StringComparer.OrdinalIgnoreCase);
                        }
                        else
                        {
                            // No specific role requested, just needs to be a registered user
                            allow = true;
                        }
                    }
                }
            }

            if (allow)
            {
                context?.Succeed(requirement);
                return Task.CompletedTask;
            }

            return requirement.HandleAsync(context);
        }

    }
}
