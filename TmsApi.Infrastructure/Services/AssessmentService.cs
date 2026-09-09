using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class AssessmentService(
    TmsDbContext context,
    ILogger<AssessmentService> logger) : IAssessmentService
{
    // ── List assessments for a course ─────────────────────────
    // Scoped to courseId — mirrors EnrollmentService.GetByCourseAsync pattern
    public async Task<IReadOnlyList<AssessmentResponseDto>> GetByCourseAsync(
        int courseId, CancellationToken ct) =>
        await context.Assessments
            .AsNoTracking()
            .Where(a => a.CourseId == courseId)
            .OrderBy(a => a.Title)
            .Select(a => new AssessmentResponseDto(
                a.Id, a.Title, a.MaxScore, a.Weight, a.CourseId))
            .ToListAsync(ct);

    // ── Get single assessment ─────────────────────────────────
    // Scoped to courseId — /api/courses/99/assessments/1
    // never returns an assessment belonging to a different course
    public Task<AssessmentResponseDto?> GetByIdAsync(
        int courseId, int id, CancellationToken ct) =>
        context.Assessments
            .AsNoTracking()
            .Where(a => a.Id == id && a.CourseId == courseId)
            .Select(a => new AssessmentResponseDto(
                a.Id, a.Title, a.MaxScore, a.Weight, a.CourseId))
            .FirstOrDefaultAsync(ct);

    // ── Create assessment ─────────────────────────────────────
    // courseId comes from the route — client cannot override it in the body
    public async Task<AssessmentResponseDto> CreateAsync(
        int courseId, CreateAssessmentRequest request, CancellationToken ct)
    {
        var assessment = new Assessment
        {
            CourseId = courseId,
            Title    = request.Title,
            MaxScore = request.MaxScore,
            Weight   = request.Weight
        };

        context.Assessments.Add(assessment);
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created assessment {AssessmentId} ({Title}) for Course {CourseId}",
            assessment.Id, assessment.Title, courseId);

        return (await GetByIdAsync(courseId, assessment.Id, ct))!;
    }

    // ── Update assessment ─────────────────────────────────────
    public async Task<AssessmentResponseDto?> UpdateAsync(
        int courseId, int id, UpdateAssessmentRequest request, CancellationToken ct)
    {
        // Scoped find — WHERE Id = id AND CourseId = courseId
        var existing = await context.Assessments
            .FirstOrDefaultAsync(a => a.Id == id && a.CourseId == courseId, ct);

        if (existing is null) return null;

        existing.Title    = request.Title;
        existing.MaxScore = request.MaxScore;
        existing.Weight   = request.Weight;

        await context.SaveChangesAsync(ct);

        logger.LogInformation("Updated assessment {AssessmentId}", id);

        return (await GetByIdAsync(courseId, id, ct))!;
    }

    // ── Delete assessment ─────────────────────────────────────
    public async Task<bool> DeleteAsync(int courseId, int id, CancellationToken ct)
    {
        var assessment = await context.Assessments
            .FirstOrDefaultAsync(a => a.Id == id && a.CourseId == courseId, ct);

        if (assessment is null) return false;

        context.Assessments.Remove(assessment);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Deleted assessment {AssessmentId}", id);
        return true;
    }
}