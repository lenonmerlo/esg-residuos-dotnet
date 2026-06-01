using System.ComponentModel.DataAnnotations;

namespace EsgResiduos.Api.DTOs.Request;

public class DestinationRequest
{
    [Required]
    public int CollectionId { get; set; }

    [Required]
    [MaxLength(120)]
    public string DestinationName { get; set; } = string.Empty;

    [Required]
    [MaxLength(60)]
    public string ProcessingType { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal DestinatedVolumeKg { get; set; }
}