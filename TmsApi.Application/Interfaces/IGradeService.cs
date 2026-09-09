using TmsApi.Application.Dtos;

namespace TmsApi.Application.Interfaces;
public interface IGradeService
{
    Task<GradeSubmissionResult> SubmitAsync(GradeSubmissionRequest request, CancellationToken ct);
}