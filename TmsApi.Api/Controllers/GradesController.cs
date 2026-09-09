using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/grades")]
public class GradesController(IGradeService gradeService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Submit(GradeSubmissionRequest request, CancellationToken ct)
    {
        var result = await gradeService.SubmitAsync(request, ct);
        return Ok(result);
    }
}