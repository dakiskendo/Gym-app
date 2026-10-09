using GymTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymTracker.Infrastructure.Data.Configurations;

public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.HasKey(s => s.UserId);

        builder.HasOne<Gym>()
            .WithMany()
            .HasForeignKey(s => s.DefaultGymId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
