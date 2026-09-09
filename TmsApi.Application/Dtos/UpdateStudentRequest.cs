using System.ComponentModel.DataAnnotations;
namespace TmsApi.Application.Dtos;

public record UpdateStudentRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    [Range(16, 100, ErrorMessage = "Age must be between 16 and 100.")]
    public int Age { get; init; }

    [Range(0.0, 4.0, ErrorMessage = "GPA must be between 0.0 and 4.0.")]
    public decimal GPA { get; init; }

    public bool IsActive { get; init; }
    public int Version { get; init; }
}
