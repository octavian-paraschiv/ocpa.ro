using Microsoft.EntityFrameworkCore;
using ocpa.ro.domain.Entities.Application;

namespace ocpa.ro.persistence.ApplicationDb.Configurations;

internal static class OneTimePasswordConfiguration
{
    internal static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OneTimePassword>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("onetimepassword");

            entity.HasIndex(e => e.UserId, "UserId");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)");

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
    }
}
