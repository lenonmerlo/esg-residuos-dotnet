namespace EsgResiduos.Api.DTOs.Response;

public class CollectionResponse
{
    public int Id { get; set; }
    public int CollectionPointId { get; set; }
    public string CollectionPointName { get; set; } = string.Empty;
    public int WasteTypeId { get; set; }
    public string WasteCategory { get; set; } = string.Empty;
    public DateTime CollectedAt { get; set; }
    public decimal VolumeKg { get; set; }
    public string Status { get; set; } = string.Empty;
}