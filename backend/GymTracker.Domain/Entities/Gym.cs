namespace GymTracker.Domain.Entities;

public class Gym
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? City { get; set; }
    public Guid UserId { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
}

