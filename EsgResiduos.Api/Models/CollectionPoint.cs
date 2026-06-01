namespace EsgResiduos.Api.Models;

public class CollectionPoint
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal CapacityKg { get; set; }
    public decimal AlertVolumeKg { get; set; }
    public decimal OccupiedVolumeKg { get; set; } = 0;
    public string Status { get; set; } = "AVAILABLE";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Collection> Collections { get; set; } = new List<Collection>();
    public ICollection<CollectionAlert> Alerts { get; set; } = new List<CollectionAlert>();
}