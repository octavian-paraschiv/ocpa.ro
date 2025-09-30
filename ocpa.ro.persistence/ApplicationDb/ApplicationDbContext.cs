using Microsoft.EntityFrameworkCore;
using ocpa.ro.domain.Abstractions.Database;
using ocpa.ro.domain.Entities.Application;
using ocpa.ro.persistence.ApplicationDb.Configurations;
using System.Linq;

namespace ocpa.ro.persistence.ApplicationDb;

public class ApplicationDbContext : BaseDbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }


    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<ApplicationMenu> ApplicationMenus { get; set; }

    public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }

    public virtual DbSet<AppMenu> AppMenus { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<MenuDisplayMode> MenuDisplayModes { get; set; }

    public virtual DbSet<OneTimePassword> OneTimePasswords { get; set; }

    public virtual DbSet<PublicMenu> PublicMenus { get; set; }

    public virtual DbSet<Region> Regions { get; set; }

    public virtual DbSet<RegisteredDevice> RegisteredDevices { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserType> UserTypes { get; set; }

    public virtual DbSet<SystemSetting> SystemSettings { get; set; }

    IQueryable<Application> IApplicationDbContext.Applications => Applications;

    IQueryable<ApplicationMenu> IApplicationDbContext.ApplicationMenus => ApplicationMenus;

    IQueryable<ApplicationUser> IApplicationDbContext.ApplicationUsers => ApplicationUsers;

    IQueryable<AppMenu> IApplicationDbContext.AppMenus => AppMenus;

    IQueryable<City> IApplicationDbContext.Cities => Cities;

    IQueryable<Menu> IApplicationDbContext.Menus => Menus;

    IQueryable<MenuDisplayMode> IApplicationDbContext.MenuDisplayModes => MenuDisplayModes;

    IQueryable<OneTimePassword> IApplicationDbContext.OneTimePasswords => OneTimePasswords;

    IQueryable<PublicMenu> IApplicationDbContext.PublicMenus => PublicMenus;

    IQueryable<Region> IApplicationDbContext.Regions => Regions;

    IQueryable<RegisteredDevice> IApplicationDbContext.RegisteredDevices => RegisteredDevices;

    IQueryable<User> IApplicationDbContext.Users => Users;

    IQueryable<UserType> IApplicationDbContext.UserTypes => UserTypes;

    IQueryable<SystemSetting> IApplicationDbContext.SystemSettings => SystemSettings;



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Tables
        ApplicationConfiguration.Build(modelBuilder);
        ApplicationMenuConfiguration.Build(modelBuilder);
        ApplicationUserConfiguration.Build(modelBuilder);
        CityConfiguration.Build(modelBuilder);
        MenuConfiguration.Build(modelBuilder);
        MenuDisplayModeConfiguration.Build(modelBuilder);
        OneTimePasswordConfiguration.Build(modelBuilder);
        RegionConfiguration.Build(modelBuilder);
        RegisteredDeviceConfiguration.Build(modelBuilder);
        UserConfiguration.Build(modelBuilder);
        UserTypeConfiguration.Build(modelBuilder);
        SystemSettingConfiguration.Build(modelBuilder);

        // Views
        AppMenuConfiguration.Build(modelBuilder);
        PublicMenuConfiguration.Build(modelBuilder);
    }
}
