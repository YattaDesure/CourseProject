namespace GreenQuarter.Domain.Entities;

public class Apartment
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public string Entrance { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal Area { get; set; }
    public string Status { get; set; } = "Vacant";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<UserApartment> UserApartments { get; set; } = new List<UserApartment>();
}

