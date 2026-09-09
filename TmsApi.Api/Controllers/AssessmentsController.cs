using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;

// Assessment is a nested resource under Course — mirrors EnrollmentsController exactly
// A course owns its assessments — they cannot exist without a parent course
[ApiController]
[Route("api/courses/{courseId:int}/assessments")]
[Tags("Assessments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class AssessmentsController(
    ICourseService courseService,
    IAssessmentService assessmentService,
    LinkGenerator linkGenerator) : ControllerBase
{
    // ── GET list of assessments for a course ──────────────────
    [HttpGet(Name = "ListCourseAssessments")]
    [ProducesResponseType(typeof(IReadOnlyList<AssessmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List assessments for a course")]
    [EndpointDescription("Returns all assessments for the given course. Returns 404 if the course does not exist.")]
    public async Task<IActionResult> GetAssessments(int courseId, CancellationToken ct)
    {
        // 404 first — confirm parent course exists
        var course = await courseService.GetByIdAsync(courseId, ct);
        if (course is null) return NotFound();

        var assessments = await assessmentService.GetByCourseAsync(courseId, ct);
        return Ok(assessments);
    }

    // ── GET single assessment with HATEOAS links ──────────────
    [HttpGet("{id:int}", Name = nameof(GetAssessment))]
    [ProducesResponseType(typeof(AssessmentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get one assessment for a course")]
    [EndpointDescription("Returns a single assessment scoped to the given course. Returns 404 if either does not exist.")]
    public async Task<IActionResult> GetAssessment(
        int courseId, int id, CancellationToken ct)
    {
        var assessment = await assessmentService.GetByIdAsync(courseId, id, ct);
        if (assessment is null) return NotFound();

        // Self href — scoped to the course
        var selfHref = linkGenerator.GetPathByName(
            HttpContext, nameof(GetAssessment), new { courseId, id })!;

        // Parent course href
        var courseHref = linkGenerator.GetPathByName(
            HttpContext, "GetCourseById", new { id = courseId });

        var links = new List<LinkDto>
        {
            new(selfHref,                   "self",   "GET"),
            new(selfHref,                   "update", "PUT"),
            new(selfHref,                   "delete", "DELETE"),
            new(courseHref ?? string.Empty, "course", "GET"),
        };

        var detail = new AssessmentDetailDto
        {
            Id       = assessment.Id,
            Title    = assessment.Title,
            MaxScore = assessment.MaxScore,
            Weight   = assessment.Weight,
            CourseId = assessment.CourseId,
            Links    = links.AsReadOnly()
        };

        return Ok(detail);
    }

    // ── POST create assessment ────────────────────────────────
    // courseId from route — cannot be overridden in body
    [HttpPost]
    [ProducesResponseType(typeof(AssessmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Create an assessment for a course")]
    [EndpointDescription("Creates an assessment under the given course. Returns 404 if the course does not exist.")]
    public async Task<IActionResult> CreateAssessment(
        int courseId, CreateAssessmentRequest request, CancellationToken ct)
    {
        // 404 first — confirm parent course exists
        var course = await courseService.GetByIdAsync(courseId, ct);
        if (course is null) return NotFound();

        var created = await assessmentService.CreateAsync(courseId, request, ct);

        return CreatedAtAction(
            nameof(GetAssessment),
            new { courseId, id = created.Id },
            created);
    }

    // ── PUT update assessment ─────────────────────────────────
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(AssessmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Update an assessment")]
    [EndpointDescription("Updates the title, max score, and weight of an assessment.")]
    public async Task<IActionResult> UpdateAssessment(
        int courseId, int id, UpdateAssessmentRequest request, CancellationToken ct)
    {
        var updated = await assessmentService.UpdateAsync(courseId, id, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    // ── DELETE assessment ─────────────────────────────────────
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete an assessment")]
    [EndpointDescription("Permanently removes the assessment from the course.")]
    public async Task<IActionResult> DeleteAssessment(
        int courseId, int id, CancellationToken ct)
        => await assessmentService.DeleteAsync(courseId, id, ct) ? NoContent() : NotFound();
}
