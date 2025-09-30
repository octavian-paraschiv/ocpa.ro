using Microsoft.EntityFrameworkCore;
using ocpa.ro.domain.Entities.Application;

namespace ocpa.ro.persistence.ApplicationDb.Configurations;

internal static class ApplicationConfiguration
{
    internal static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("application");

            entity.HasIndex(e => e.Code, "Code").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("int(11)");

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
    }
}
