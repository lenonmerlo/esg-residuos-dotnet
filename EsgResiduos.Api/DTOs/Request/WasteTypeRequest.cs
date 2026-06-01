using System.ComponentModel.DataAnnotations;

namespace EsgResiduos.Api.DTOs.Request;

public class WasteTypeRequest
{
    [Required]
    [MaxLength(30)]
    public string WasteCategory { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }
}