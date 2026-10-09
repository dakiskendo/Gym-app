using GymTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Infrastructure.Data;

public class GymTrackerDbContext : DbContext
{
    public GymTrackerDbContext(DbContextOptions<GymTrackerDbContext> options) : base(options)
    {
        
    }

    public DbSet<Gym> Gyms => Set<Gym>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
}
