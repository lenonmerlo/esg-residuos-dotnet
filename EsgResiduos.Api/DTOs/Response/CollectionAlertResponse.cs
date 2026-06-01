namespace EsgResiduos.Api.DTOs.Response;

public class CollectionAlertResponse
{
    public int Id { get; set; }
    public int CollectionPointId { get; set; }
    public string CollectionPointName { get; set; } = string.Empty;
    public int? CollectionId { get; set; }
    public DateTime AlertedAt { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}