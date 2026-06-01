namespace EsgResiduos.Api.Models;

public class CollectionAlert
{
    public int Id { get; set; }
    public int CollectionPointId { get; set; }
    public int? CollectionId { get; set; }
    public DateTime AlertedAt { get; set; } = DateTime.UtcNow;
    public string AlertType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public CollectionPoint CollectionPoint { get; set; } = null!;
    public Collection? Collection { get; set; }
}