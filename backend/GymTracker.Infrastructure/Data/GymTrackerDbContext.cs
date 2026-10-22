using GymTracker.Domain.Entities;
using GymTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Infrastructure.Data;

public class GymTrackerDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public GymTrackerDbContext(DbContextOptions<GymTrackerDbContext> options) : base(options){}

    public DbSet<Gym> Gyms => Set<Gym>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GymTrackerDbContext).Assembly);
    }
}
