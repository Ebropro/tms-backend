using TmsApi.Application.Dtos;

namespace TmsApi.Application.Interfaces;

public interface ICertificateService
{

    Task<CertificateResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    // List all certificates for a specific student
    // GET /api/students/{studentId}/certificates
    Task<IReadOnlyList<CertificateResponseDto>> GetByStudentAsync(int studentId, CancellationToken ct);
    // List all certificates for a specific course
    // GET /api/courses/{courseId}/certificates
    Task<IReadOnlyList<CertificateResponseDto>> GetByCourseAsync(int courseId, CancellationToken ct);
    Task<CertificateResponseDto> CreateAsync(CreateCertificateRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task<bool> SerialNumberExistsAsync(string serialNumber, CancellationToken ct);
}