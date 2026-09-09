using System.ComponentModel.DataAnnotations;
namespace TmsApi.Application.Dtos;

public record UpdateAssessmentRequest
{
    [Required, MaxLength(200)]
    public required string Title { get; init; }

    [Range(1, 100, ErrorMessage = "MaxScore must be between 1 and 100.")]
    public decimal MaxScore { get; init; }

    [Range(0.01, 100, ErrorMessage = "Weight must be between 0.01 and 100.")]
    public decimal Weight { get; init; }
}
