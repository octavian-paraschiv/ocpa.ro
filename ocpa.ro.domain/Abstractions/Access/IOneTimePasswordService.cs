using ocpa.ro.domain.Entities.Application;
using ocpa.ro.domain.Models.Authentication;
using System.Threading.Tasks;

namespace ocpa.ro.domain.Abstractions.Access;

public interface IOneTimePasswordService
{
    (string err, User user) ValidateOneTimePassword(AuthenticateRequest req);
    Task<bool> GenerateOneTimePassword(string loginId, string language);
}