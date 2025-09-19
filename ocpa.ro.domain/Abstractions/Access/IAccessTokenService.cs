using ocpa.ro.domain.Entities.Application;
using ocpa.ro.domain.Models.Authentication;

namespace ocpa.ro.domain.Abstractions.Access;

public interface IAccessTokenService
{
    AuthenticationResponse GenerateAccessToken(User user);

}