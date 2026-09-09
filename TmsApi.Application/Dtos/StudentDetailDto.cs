namespace TmsApi.Application.Dtos;

// Detail shape — returned by GET /api/students/{id} only
// Includes everything in StudentResponseDto PLUS HATEOAS links

public record StudentDetailDto
{
    public required int Id { get; init; }
    public required string RegistrationNumber { get; init; }
    public required string Name { get; init; }
    public required int Age { get; init; }
    public required decimal GPA { get; init; }
    public required bool IsActive { get; init; }
    public required int EnrollmentCount { get; init; }

    // HATEOAS links — what the client can do with this student
    public required IReadOnlyList<LinkDto> Links { get; init; }
}
