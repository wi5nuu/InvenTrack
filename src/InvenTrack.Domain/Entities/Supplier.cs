namespace InvenTrack.Domain.Entities;

/// <summary>Supplier/vendor that provides products to the inventory.</summary>
public class Supplier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
