using Microsoft.EntityFrameworkCore;
using ocpa.ro.domain.Entities.Application;

namespace ocpa.ro.persistence.ApplicationDb.Configurations;

internal static class UserConfiguration
{
    internal static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("user");

            entity.HasIndex(e => e.LoginId, "LoginId").IsUnique();

            entity.HasIndex(e => e.Type, "Type");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)");

            entity.Property(e => e.EmailAddress)
                .HasMaxLength(128)
                .HasDefaultValueSql("'NULL'");

            entity.Property(e => e.Enabled)
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0")
                .HasSentinel(false);

            entity.Property(e => e.LoginAttemptsRemaining)
                .HasColumnType("int(11)");

            entity.Property(e => e.LoginId)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(e => e.Type)
                .HasColumnType("int(11)");

            entity.Property(e => e.UseOtp)
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0")
                .HasSentinel(false)
                .HasColumnName("UseOTP");

            entity.HasOne(d => d.TypeNavigation)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.Type)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("user_ibfk_1");
        });
    }
}
