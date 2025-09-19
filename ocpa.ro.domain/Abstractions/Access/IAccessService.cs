using ocpa.ro.domain.Entities.Application;
using ocpa.ro.domain.Models.Authentication;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;

namespace ocpa.ro.domain.Abstractions.Access;

public interface IAccessService
{
    (User user, bool useOTP) Authenticate(AuthenticateRequest req);

    User SaveUser(User user, out bool inserted);
    User GetUser(string loginId);
    int DeleteUser(string loginId);
    IEnumerable<User> AllUsers();
    UserType GetUserType(int id = -1, string code = null);

    // ----
    IEnumerable<PublicMenu> PublicMenus(string deviceId);
    IEnumerable<AppMenu> ApplicationMenus(string deviceId, IIdentity identity);

    // ----
    IEnumerable<RegisteredDevice> GetRegisteredDevices();
    RegisteredDevice GetRegisteredDevice(string deviceId);
    Task RegisterDevice(string deviceId, string ipAddress, string loginId);
    int DeleteRegisteredDevice(string deviceId);
    void GuardContentPath(IIdentity identity, string contentPath);
}