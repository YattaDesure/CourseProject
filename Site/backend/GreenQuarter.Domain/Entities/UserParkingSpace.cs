namespace GreenQuarter.Domain.Entities;

public class UserParkingSpace
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int ParkingSpaceId { get; set; }
    public DateTime AssignedSince { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public ParkingSpace ParkingSpace { get; set; } = null!;
}

