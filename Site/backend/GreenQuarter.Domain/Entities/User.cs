using Microsoft.AspNetCore.Identity;

namespace GreenQuarter.Domain.Entities;

public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Patronymic { get; set; }
    public string Role { get; set; } = "User"; // User, Moderator, Admin
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<UserApartment> UserApartments { get; set; } = new List<UserApartment>();
    public ICollection<UserParkingSpace> UserParkingSpaces { get; set; } = new List<UserParkingSpace>();
    public ICollection<UserStorageRoom> UserStorageRooms { get; set; } = new List<UserStorageRoom>();
}

