using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.Dtos;

public record CreateCertificateRequest
{
    // SerialNumber uniquely identifies the certificate
    // Pattern: CERT-YYYY-NNNNNN e.g. CERT-2026-000001
    [Required]
    [RegularExpression(@"^CERT-\d{4}-\d{6}$",
        ErrorMessage = "SerialNumber must follow the pattern CERT-YYYY-NNNNNN (e.g. CERT-2026-000001).")]
    public required string SerialNumber { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "StudentId must be a positive integer.")]
    public required int StudentId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "CourseId must be a positive integer.")]
    public required int CourseId { get; init; }
    public DateTime? IssuedAt { get; init; }
}
