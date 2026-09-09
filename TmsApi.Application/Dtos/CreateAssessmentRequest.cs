using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.Dtos;

public record CreateAssessmentRequest
{
    [Required, MaxLength(200)]
    public required string Title { get; init; }

    // MaxScore must be positive — 0-point assessment makes no sense
    [Range(1, 100, ErrorMessage = "MaxScore must be between 1 and 100.")]
    public decimal MaxScore { get; init; }

    // Weight is a percentage — total weights across all assessments should sum to 100
    // Validation here is per-assessment; sum validation lives in the service
    [Range(0.01, 100, ErrorMessage = "Weight must be between 0.01 and 100.")]
    public decimal Weight { get; init; }
}
