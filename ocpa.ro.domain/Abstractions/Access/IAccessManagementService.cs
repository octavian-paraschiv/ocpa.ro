using ocpa.ro.domain.Entities;
using System.Collections.Generic;

namespace ocpa.ro.domain.Abstractions.Access;

public interface IAccessManagementService
{
    //------------------------
    IEnumerable<UserType> AllUserTypes();

    IEnumerable<Application> GetApplications();
    Application SaveApplication(Application app, out bool inserted);
    int DeleteApplication(int appId);

    //------------------------

    IEnumerable<Menu> GetMenus();
    Menu SaveMenu(Menu menu, out bool inserted);
    int DeleteMenu(int menuId);

    //------------------------

    IEnumerable<ApplicationMenu> GetApplicationMenus();
    ApplicationMenu SaveApplicationMenu(int appId, int menuId, out bool inserted);
    int DeleteApplicationMenu(int appId, int menuId);

    //------------------------

    IEnumerable<ApplicationUser> GetAppsForUser(int userId);
    void SaveAppsForUser(int userId, IEnumerable<ApplicationUser> appsForUser);
    void DeleteAppsForUser(int userId, bool isSaving);
}