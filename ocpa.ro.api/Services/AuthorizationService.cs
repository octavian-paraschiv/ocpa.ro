using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using ocpa.ro.domain.Abstractions.Access;
using ocpa.ro.domain.Abstractions.Services;
using ocpa.ro.domain.Models.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ocpa.ro.api.Services
{
    public class AuthorizationService : AuthorizationHandler<RolesAuthorizationRequirement>
    {
        private readonly IAccessService _accessService;
        private readonly IWebHostEnvironment _hostingEnvironment = null;
        private readonly AuthConfig _authConfig;

        public AuthorizationService(IAccessService accessService, IWebHostEnvironment hostingEnvironment, ISystemSettingsService settingsService)
        {
            _accessService = accessService;
            _hostingEnvironment = hostingEnvironment;
            _authConfig = settingsService.AuthenticationSettings;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RolesAuthorizationRequirement requirement)
        {
            bool allow = (_hostingEnvironment?.IsDevelopment() ?? false) || (_authConfig?.Disabled ?? false);

            if (!allow)
            {
                var loginId = context?.User.Claims.FirstOrDefault()?.Value;
                if (loginId?.Length > 0)
                {
                    var user = _accessService.GetUser(loginId);
                    var userType = _accessService.GetUserType(id: user.Type);

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
