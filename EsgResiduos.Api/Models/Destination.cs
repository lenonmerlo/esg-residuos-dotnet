namespace EsgResiduos.Api.Models;

public class Destination
{
    public int Id { get; set; }
    public int CollectionId { get; set; }
    public DateTime DestinatedAt { get; set; } = DateTime.UtcNow;
    public string DestinationName { get; set; } = string.Empty;
    public string ProcessingType { get; set; } = string.Empty;
    public decimal DestinatedVolumeKg { get; set; }
    public Collection Collection { get; set; } = null!;
}