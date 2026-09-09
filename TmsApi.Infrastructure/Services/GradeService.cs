using Microsoft.Extensions.Logging;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class GradeService(TmsDbContext context, ILogger<GradeService> logger) : IGradeService
{
    public async Task<GradeSubmissionResult> SubmitAsync(GradeSubmissionRequest request, CancellationToken ct)
    {
        var grade = new GradeSubmission
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            Score = request.Score
        };

        context.Grades.Add(grade);
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Grade {GradeId} submitted — student {StudentId}, course {CourseId}, score {Score}",
            grade.Id, grade.StudentId, grade.CourseId, grade.Score);

        return new GradeSubmissionResult(grade.Id.ToString(), true);
    }
}