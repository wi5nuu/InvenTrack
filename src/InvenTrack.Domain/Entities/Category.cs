namespace InvenTrack.Domain.Entities;

/// <summary>Product category for organizing inventory items.</summary>
public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
