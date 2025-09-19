using ocpa.ro.domain.Entities.Application;
using System.Linq;

namespace ocpa.ro.domain.Abstractions.Database;

public interface IApplicationDbContext : IBaseDbContext
{
    IQueryable<Application> Applications { get; }

    IQueryable<ApplicationMenu> ApplicationMenus { get; }

    IQueryable<ApplicationUser> ApplicationUsers { get; }

    IQueryable<AppMenu> AppMenus { get; }

    IQueryable<City> Cities { get; }

    IQueryable<Menu> Menus { get; }

    IQueryable<MenuDisplayMode> MenuDisplayModes { get; }

    IQueryable<OneTimePassword> OneTimePasswords { get; }

    IQueryable<PublicMenu> PublicMenus { get; }

    IQueryable<Region> Regions { get; }

    IQueryable<RegisteredDevice> RegisteredDevices { get; }

    IQueryable<User> Users { get; }

    IQueryable<UserType> UserTypes { get; }

    IQueryable<SystemSetting> SystemSettings { get; }
}