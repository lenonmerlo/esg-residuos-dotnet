namespace EsgResiduos.Api.DTOs.Response;

public class DestinationResponse
{
    public int Id { get; set; }
    public int CollectionId { get; set; }
    public DateTime DestinatedAt { get; set; }
    public string DestinationName { get; set; } = string.Empty;
    public string ProcessingType { get; set; } = string.Empty;
    public decimal DestinatedVolumeKg { get; set; }
}