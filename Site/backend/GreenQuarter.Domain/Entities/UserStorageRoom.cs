namespace GreenQuarter.Domain.Entities;

public class UserStorageRoom
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int StorageRoomId { get; set; }
    public DateTime AssignedSince { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public StorageRoom StorageRoom { get; set; } = null!;
}

