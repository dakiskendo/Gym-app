namespace GymTracker.Domain.Entities;

public class UserProfile
{
    public Guid UserId { get; set; }
    public required string DisplayName { get; set; }

}