using System.ComponentModel.DataAnnotations;

namespace EsgResiduos.Api.DTOs.Request;

public class CollectionRequest
{
    [Required]
    public int CollectionPointId { get; set; }

    [Required]
    public int WasteTypeId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal VolumeKg { get; set; }
}