namespace TmsApi.Application.Dtos;
public record StudentResponseDto(
    int Id,
    string RegistrationNumber,
    string Name,
    int Age,
    decimal GPA,
    bool IsActive,
    int EnrollmentCount);
