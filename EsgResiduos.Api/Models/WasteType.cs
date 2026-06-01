namespace EsgResiduos.Api.Models;

public class WasteType
{
    public int Id { get; set; }
    public string WasteCategory { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<Collection> Collections { get; set; } = [];
}