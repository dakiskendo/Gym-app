using GymTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymTracker.Infrastructure.Data.Configurations;

public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.HasKey(s => s.UserId);

        builder.Property(s => s.Unit).HasMaxLength(2).HasDefaultValue("kg");

        builder.HasOne<Gym>()
            .WithMany()
            .HasForeignKey(s => s.DefaultGymId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
