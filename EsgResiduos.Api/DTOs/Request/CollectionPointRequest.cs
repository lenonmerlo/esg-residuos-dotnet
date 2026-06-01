using System.ComponentModel.DataAnnotations;

namespace EsgResiduos.Api.DTOs.Request;

public class CollectionPointRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal CapacityKg { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal AlertVolumeKg { get; set; }
}