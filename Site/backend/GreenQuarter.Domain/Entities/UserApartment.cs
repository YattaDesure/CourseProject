namespace GreenQuarter.Domain.Entities;

public class UserApartment
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int ApartmentId { get; set; }
    public decimal SharePercentage { get; set; } = 100;
    public DateTime OwnedSince { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Apartment Apartment { get; set; } = null!;
}

