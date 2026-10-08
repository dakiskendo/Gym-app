namespace GymTracker.Domain.Entities;

public class UserSettings
{
    public Guid UserId { get; set; }
    public Guid? DefaultGymId { get; set; }
}