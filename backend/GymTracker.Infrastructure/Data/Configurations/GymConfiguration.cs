using GymTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymTracker.Infrastructure.Data.Configurations;

public class GymConfiguration : IEntityTypeConfiguration<Gym>
{
    public void Configure(EntityTypeBuilder<Gym> builder)
    {
        builder.Property(g => g.Name).IsRequired().HasMaxLength(50);
        builder.Property(g => g.City).HasMaxLength(50);

        builder.HasIndex(g => new { g.UserId, g.Name })
            .IsUnique()
            .HasFilter("\"is_archived\" = false");
    }
}
