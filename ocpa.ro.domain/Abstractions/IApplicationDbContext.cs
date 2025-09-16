using ocpa.ro.domain.Entities;
using System.Linq;

namespace ocpa.ro.domain.Abstractions;

public interface IApplicationDbContext
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


    int Insert<T>(T entity) where T : class, IDbEntity;
    int Update<T>(T entity) where T : class, IDbEntity;
    int Delete<T>(T entity) where T : class, IDbEntity;

    int ExecuteSqlRaw(string query, params object[] args);

    void BeginTransaction();
    void CommitTransaction();
    void RollbackTransaction();
}