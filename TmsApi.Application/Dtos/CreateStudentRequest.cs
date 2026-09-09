using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.Dtos;

public record CreateStudentRequest
{
    // Registration number must follow pattern: TMS-YYYY-NNNN e.g. TMS-2026-0001
    [Required]
    [RegularExpression(@"^TMS-\d{4}-\d{4}$",
        ErrorMessage = "RegistrationNumber must follow the pattern TMS-YYYY-NNNN (e.g. TMS-2026-0001).")]
    public required string RegistrationNumber { get; init; }

    [Required, MaxLength(100)]
    public required string Name { get; init; }

    [Range(16, 100, ErrorMessage = "Age must be between 16 and 100.")]
    public int Age { get; init; }

    [Range(0.0, 4.0, ErrorMessage = "GPA must be between 0.0 and 4.0.")]
    public decimal GPA { get; init; }
    public bool IsActive { get; init; } = true;
}
