using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class CertificateService(
    TmsDbContext context,
    ILogger<CertificateService> logger) : ICertificateService
{
    // ── Shared projection ─────────────────────────────────────
    // Defined once — used by every read method
    // EF translates this Select into SQL — no navigation properties loaded
    private static CertificateResponseDto Project(Certificate c) =>
        new(c.Id, c.SerialNumber, c.IssuedAt, c.StudentId, c.CourseId);

    // ── Get single certificate ────────────────────────────────
    public Task<CertificateResponseDto?> GetByIdAsync(int id, CancellationToken ct) =>
        context.Certificates
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CertificateResponseDto(
                c.Id, c.SerialNumber, c.IssuedAt, c.StudentId, c.CourseId))
            .FirstOrDefaultAsync(ct);

    // ── List by student ───────────────────────────────────────
    // Scoped to studentId — /api/students/{studentId}/certificates
    // never returns certificates belonging to a different student
    public async Task<IReadOnlyList<CertificateResponseDto>> GetByStudentAsync(
        int studentId, CancellationToken ct) =>
        await context.Certificates
            .AsNoTracking()
            .Where(c => c.StudentId == studentId)
            .OrderByDescending(c => c.IssuedAt)
            .Select(c => new CertificateResponseDto(
                c.Id, c.SerialNumber, c.IssuedAt, c.StudentId, c.CourseId))
            .ToListAsync(ct);

    // ── List by course ────────────────────────────────────────
    public async Task<IReadOnlyList<CertificateResponseDto>> GetByCourseAsync(
        int courseId, CancellationToken ct) =>
        await context.Certificates
            .AsNoTracking()
            .Where(c => c.CourseId == courseId)
            .OrderByDescending(c => c.IssuedAt)
            .Select(c => new CertificateResponseDto(
                c.Id, c.SerialNumber, c.IssuedAt, c.StudentId, c.CourseId))
            .ToListAsync(ct);

    // ── Create certificate ────────────────────────────────────
    // IssuedAt defaults to UtcNow if client didn't provide one
    public async Task<CertificateResponseDto> CreateAsync(
        CreateCertificateRequest request, CancellationToken ct)
    {
        var certificate = new Certificate
        {
            SerialNumber = request.SerialNumber,
            StudentId    = request.StudentId,
            CourseId     = request.CourseId,
            IssuedAt     = request.IssuedAt ?? DateTime.UtcNow
        };

        context.Certificates.Add(certificate);
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Issued certificate {CertificateId} ({SerialNumber}) to Student {StudentId} for Course {CourseId}",
            certificate.Id, certificate.SerialNumber,
            certificate.StudentId, certificate.CourseId);

        return (await GetByIdAsync(certificate.Id, ct))!;
    }

    // ── Delete certificate ────────────────────────────────────
    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var certificate = await context.Certificates.FindAsync([id], ct);
        if (certificate is null) return false;

        context.Certificates.Remove(certificate);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Deleted certificate {CertificateId}", id);
        return true;
    }

    // ── Duplicate serial number check ─────────────────────────
    // AnyAsync → SELECT EXISTS (SELECT 1 LIMIT 1) — fastest existence check
    public Task<bool> SerialNumberExistsAsync(string serialNumber, CancellationToken ct) =>
        context.Certificates
            .AsNoTracking()
            .AnyAsync(c => c.SerialNumber == serialNumber, ct);
}