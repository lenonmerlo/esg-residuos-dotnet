namespace EsgResiduos.Api.DTOs.Response;

public class WasteTypeResponse
{
    public int Id { get; set; }
    public string WasteCategory { get; set; } = string.Empty;
    public string? Description { get; set; }
}