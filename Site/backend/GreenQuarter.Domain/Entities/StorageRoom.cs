namespace GreenQuarter.Domain.Entities;

public class StorageRoom
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public decimal Area { get; set; }
    public string Status { get; set; } = "Available";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<UserStorageRoom> UserStorageRooms { get; set; } = new List<UserStorageRoom>();
}

