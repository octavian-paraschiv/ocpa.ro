using Microsoft.AspNetCore.Authorization;
using ocpa.ro.api.Helpers.Authentication;
using ocpa.ro.api.Models.Authentication;
using System.Linq;
using System.Threading.Tasks;

namespace ocpa.ro.api.Policies
{
    public class AdminRequirement : IAuthorizationRequirement
    {
    }

    public class AdminHandler : AuthorizationHandler<AdminRequirement>
    {
        private readonly IAuthHelper _authHelper;

        public AdminHandler(IAuthHelper authHelper)
        {
            _authHelper = authHelper;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
        {
            var loginId = context?.User.Claims.FirstOrDefault()?.Value;
            if (context != null && string.IsNullOrEmpty(loginId))
                return Task.CompletedTask;

            var user = _authHelper.GetUser(loginId);
            if (user == null)
                return Task.CompletedTask;

            if (user.Type != UserType.Admin) 
                return Task.CompletedTask;

            context?.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
