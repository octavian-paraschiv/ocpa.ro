using System.Linq;
using System.Security.Claims;

namespace ocpa.ro.application.Extensions;

public static class ExtensionMethods
{
    public static int GetClaimsUid(this ClaimsIdentity claimsIdentity)
    {
        var uidValue = claimsIdentity.Claims
                   .AsEnumerable()
                   .FirstOrDefault(c => c.Type == "uid")?.Value;

        return int.TryParse(uidValue, out int v) ? v : 0;
    }
}
