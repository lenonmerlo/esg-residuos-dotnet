namespace EsgResiduos.Api.Models;

public class Collection
{
    public int Id { get; set; }
    public int CollectionPointId { get; set; }
    public int WasteTypeId { get; set; }
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
    public decimal VolumeKg { get; set; }
    public string Status { get; set; } = "OPEN";
    public DateTime? DestinatedAt { get; set; }
    public string? DestinationHistory { get; set; }
    public CollectionPoint CollectionPoint { get; set; } = null!;
    public WasteType WasteType { get; set; } = null!;
    public ICollection<Destination> Destinations { get; set; } = new List<Destination>();
    public ICollection<CollectionAlert> Alerts { get; set; } = new List<CollectionAlert>();
}