namespace GreenQuarter.Domain.Entities;

public class ParkingSpace
{
    public int Id { get; set; }
    public string SlotNumber { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string Size { get; set; } = "Standard";
    public bool IsCovered { get; set; }
    public bool IsEVReady { get; set; }
    public string Status { get; set; } = "Available";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<UserParkingSpace> UserParkingSpaces { get; set; } = new List<UserParkingSpace>();
}

