using Microsoft.EntityFrameworkCore;
using ocpa.ro.domain.Abstractions;
using ocpa.ro.domain.Entities;

namespace ocpa.ro.Persistence;

public partial class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext()
    {
    }

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

    int IApplicationDbContext.ExecuteSqlRaw(string query, params object[] args) => Database.ExecuteSqlRaw(query, args);

    int IApplicationDbContext.Delete<T>(T entity)
    {
        var dbSet = GetDbContext<T>();
        if (dbSet != null)
        {
            dbSet.Remove(entity);
            return SaveChanges();
        }

        return 0;
    }

    int IApplicationDbContext.Insert<T>(T entity)
    {
        var dbSet = GetDbContext<T>();
        if (dbSet != null)
        {
            dbSet.Add(entity);
            return SaveChanges();
        }

        return 0;
    }


    int IApplicationDbContext.Update<T>(T entity)
    {
        var dbSet = GetDbContext<T>();
        if (dbSet != null)
        {
            dbSet.Update(entity);
            return SaveChanges();
        }

        return 0;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("application");

            entity.HasIndex(e => e.Code, "Code").IsUnique();

            entity.Property(e => e.Id).HasColumnType("int(11)");

            entity.Property(e => e.AdminMode)
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0")
                .HasSentinel(false);

            entity.Property(e => e.Builtin)
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0")
                .HasSentinel(false);

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(5);

            entity.Property(e => e.LoginRequired)
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0")
                .HasSentinel(false);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(16);
        });

        modelBuilder.Entity<ApplicationMenu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("applicationmenu");

            entity.HasIndex(e => e.ApplicationId, "ApplicationId");

            entity.HasIndex(e => e.MenuId, "MenuId");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.ApplicationId)
                .HasDefaultValueSql("'NULL'")
                .HasColumnType("int(11)");
            entity.Property(e => e.MenuId).HasColumnType("int(11)");

            entity.HasOne(d => d.Application).WithMany(p => p.ApplicationMenus)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("applicationmenu_ibfk_3");

            entity.HasOne(d => d.Menu).WithMany(p => p.ApplicationMenus)
                .HasForeignKey(d => d.MenuId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("applicationmenu_ibfk_2");
        });

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("applicationuser");

            entity.HasIndex(e => e.ApplicationId, "ApplicationId");

            entity.HasIndex(e => e.UserId, "UserId");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.ApplicationId).HasColumnType("int(11)");
            entity.Property(e => e.UserId).HasColumnType("int(11)");

            entity.HasOne(d => d.Application).WithMany(p => p.ApplicationUsers)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("applicationuser_ibfk_1");

            entity.HasOne(d => d.User).WithMany(p => p.Applicationusers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("applicationuser_ibfk_2");
        });

        modelBuilder.Entity<AppMenu>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("appmenu");

            entity.Property(e => e.AdminMode)
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0")
                .HasSentinel(false);

            entity.Property(e => e.AppName).HasMaxLength(16);
            entity.Property(e => e.DisplayMode)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.MenuIcon)
                .HasMaxLength(64)
                .HasDefaultValueSql("'NULL'");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(64);
            entity.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.UserId)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("city");

            entity.HasIndex(e => e.RegionId, "RegionId");

            entity.Property(e => e.Id).HasColumnType("int(11)");

            entity.Property(e => e.IsDefault)
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0")
                .HasSentinel(false);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.RegionId).HasColumnType("int(11)");
            entity.Property(e => e.Subregion)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasOne(d => d.Region).WithMany(p => p.Cities)
                .HasForeignKey(d => d.RegionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("city_ibfk_1");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("menu");

            entity.HasIndex(e => e.DisplayModeId, "DisplayModeId");

            entity.Property(e => e.Id).HasColumnType("int(11)");

            entity.Property(e => e.Builtin)
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0")
                .HasSentinel(false);

            entity.Property(e => e.DisplayModeId)
                .HasDefaultValueSql("'NULL'")
                .HasColumnType("int(11)");
            entity.Property(e => e.MenuIcon)
                .HasMaxLength(64)
                .HasDefaultValueSql("'NULL'");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(64);
            entity.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(128);

            entity.HasOne(d => d.DisplayMode).WithMany(p => p.Menus)
                .HasForeignKey(d => d.DisplayModeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("menu_ibfk_1");
        });

        modelBuilder.Entity<MenuDisplayMode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("menudisplaymode");

            entity.HasIndex(e => e.Code, "Code").IsUnique();

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(5);
            entity.Property(e => e.Description)
                .HasMaxLength(16)
                .HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<OneTimePassword>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("onetimepassword");

            entity.HasIndex(e => e.UserId, "UserId");

            entity.Property(e => e.Id).HasColumnType("int(11)");

            entity.Property(e => e.Expiration)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.Hash)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(e => e.UserId).HasColumnType("int(11)");

            entity.HasOne(d => d.User).WithMany(p => p.OneTimePasswords)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("onetimepassword_ibfk_1");
        });

        modelBuilder.Entity<PublicMenu>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("publicmenu");

            entity.Property(e => e.DisplayMode)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.MenuIcon)
                .HasMaxLength(64)
                .HasDefaultValueSql("'NULL'");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(64);
            entity.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(128);
        });

        modelBuilder.Entity<Region>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("region");

            entity.HasIndex(e => e.Code, "Code").IsUnique();

            entity.HasIndex(e => e.Name, "Name").IsUnique();

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(5);
            entity.Property(e => e.Name).IsRequired();
        });

        modelBuilder.Entity<RegisteredDevice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("registereddevice");

            entity.HasIndex(e => e.DeviceId, "DeviceId").IsUnique();

            entity.Property(e => e.DeviceId)
                .IsRequired()
                .HasMaxLength(64);
            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.LastLoginGeoLocation)
                .HasMaxLength(1024)
                .HasDefaultValueSql("'NULL'");
            entity.Property(e => e.LastLoginId)
                .IsRequired()
                .HasMaxLength(64);
            entity.Property(e => e.LastLoginIpAddress)
                .HasMaxLength(64)
                .HasDefaultValueSql("'NULL'");
            entity.Property(e => e.LastLoginTimestamp).HasColumnType("datetime");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("user");

            entity.HasIndex(e => e.LoginId, "LoginId").IsUnique();

            entity.HasIndex(e => e.Type, "Type");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.EmailAddress)
                .HasMaxLength(128)
                .HasDefaultValueSql("'NULL'");

            entity.Property(e => e.Enabled)
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0")
                .HasSentinel(false);

            entity.Property(e => e.LoginAttemptsRemaining).HasColumnType("int(11)");
            entity.Property(e => e.LoginId)
                .IsRequired()
                .HasMaxLength(64);
            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(64);
            entity.Property(e => e.Type).HasColumnType("int(11)");

            entity.Property(e => e.UseOtp)
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0")
                .HasSentinel(false)
                .HasColumnName("UseOTP");

            entity.HasOne(d => d.TypeNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.Type)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("user_ibfk_1");
        });

        modelBuilder.Entity<UserType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("usertype");

            entity.HasIndex(e => e.Code, "Code").IsUnique();

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(5);
            entity.Property(e => e.Description)
                .HasMaxLength(16)
                .HasDefaultValueSql("'NULL'");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    private DbSet<T> GetDbContext<T>() where T : class, IDbEntity
    {
        var dbSetType = typeof(DbSet<T>);
        var pi = GetType().GetProperties().Where(p => p.PropertyType == dbSetType).FirstOrDefault();
        return pi?.GetValue(this) as DbSet<T>;
    }

    void IApplicationDbContext.BeginTransaction() => Database.BeginTransaction();

    void IApplicationDbContext.CommitTransaction() => Database.CommitTransaction();

    void IApplicationDbContext.RollbackTransaction() => Database.RollbackTransaction();
}
