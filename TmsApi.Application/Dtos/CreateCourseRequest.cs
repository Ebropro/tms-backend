using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.Dtos;

public record CreateCourseRequest
{
    [Required]
    [RegularExpression(@"^[A-Z]{3}-\d{3}$",
        ErrorMessage = "Code must follow the pattern XXX-000 (e.g., CSE-101).")]
    public string Code { get; init; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    [Range(1, 200)]
    public int MaxCapacity { get; init; }
}