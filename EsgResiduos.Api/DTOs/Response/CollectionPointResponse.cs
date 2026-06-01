namespace EsgResiduos.Api.DTOs.Response;

public class CollectionPointResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal CapacityKg { get; set; }
    public decimal AlertVolumeKg { get; set; }
    public decimal OccupiedVolumeKg { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}