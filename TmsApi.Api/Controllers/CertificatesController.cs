using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/certificates")]
[Tags("Certificates")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CertificatesController(
    ICertificateService certificateService,
    IStudentService studentService,
    ICourseService courseService,
    LinkGenerator linkGenerator) : ControllerBase
{
    // ── GET single certificate with HATEOAS links ─────────────
    [HttpGet("{id:int}", Name = nameof(GetCertificateById))]
    [ProducesResponseType(typeof(CertificateDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a certificate by ID")]
    [EndpointDescription("Returns certificate details with HATEOAS links to the student and course it belongs to.")]
    public async Task<IActionResult> GetCertificateById(int id, CancellationToken ct)
    {
        var certificate = await certificateService.GetByIdAsync(id, ct);
        if (certificate is null) return NotFound();

        // Self href
        var selfHref = linkGenerator.GetPathByName(
            HttpContext, nameof(GetCertificateById), new { id })!;

        // Link back to the student who received this certificate
        var studentHref = linkGenerator.GetPathByName(
            HttpContext, "GetStudentById", new { id = certificate.StudentId });

        // Link back to the course this certificate is for
        var courseHref = linkGenerator.GetPathByName(
            HttpContext, "GetCourseById", new { id = certificate.CourseId });

        var links = new List<LinkDto>
        {
            new(selfHref,                    "self",    "GET"),
            new(selfHref,                    "delete",  "DELETE"),
            new(studentHref ?? string.Empty, "student", "GET"),
            new(courseHref  ?? string.Empty, "course",  "GET"),
        };

        var detail = new CertificateDetailDto
        {
            Id           = certificate.Id,
            SerialNumber = certificate.SerialNumber,
            IssuedAt     = certificate.IssuedAt,
            StudentId    = certificate.StudentId,
            CourseId     = certificate.CourseId,
            Links        = links.AsReadOnly()
        };

        return Ok(detail);
    }

    // ── GET certificates for a student ────────────────────────
    // /api/certificates?studentId=1
    // Alternative: move to StudentsController as nested route
    [HttpGet("by-student/{studentId:int}", Name = "GetCertificatesByStudent")]
    [ProducesResponseType(typeof(IReadOnlyList<CertificateResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List certificates for a student")]
    [EndpointDescription("Returns all certificates issued to the given student. Returns 404 if the student does not exist.")]
    public async Task<IActionResult> GetByStudent(int studentId, CancellationToken ct)
    {
        // 404 first — confirm student exists
        var student = await studentService.GetByIdAsync(studentId, ct);
        if (student is null) return NotFound();

        var certificates = await certificateService.GetByStudentAsync(studentId, ct);
        return Ok(certificates);
    }

    // ── GET certificates for a course ─────────────────────────
    [HttpGet("by-course/{courseId:int}", Name = "GetCertificatesByCourse")]
    [ProducesResponseType(typeof(IReadOnlyList<CertificateResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List certificates for a course")]
    [EndpointDescription("Returns all certificates issued for the given course. Returns 404 if the course does not exist.")]
    public async Task<IActionResult> GetByCourse(int courseId, CancellationToken ct)
    {
        // 404 first — confirm course exists
        var course = await courseService.GetByIdAsync(courseId, ct);
        if (course is null) return NotFound();

        var certificates = await certificateService.GetByCourseAsync(courseId, ct);
        return Ok(certificates);
    }

    // ── POST create certificate ───────────────────────────────
    // Validates student and course exist before issuing
    // 409 on duplicate serial number — same as Course duplicate code check
    [HttpPost]
    [ProducesResponseType(typeof(CertificateDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Issue a certificate")]
    [EndpointDescription("Issues a certificate to a student for a course. Returns 404 if student or course not found. Returns 409 if serial number already exists.")]
    public async Task<IActionResult> Create(
        CreateCertificateRequest request, CancellationToken ct)
    {
        // 404 — student must exist
        var student = await studentService.GetByIdAsync(request.StudentId, ct);
        if (student is null)
            return NotFound(new ProblemDetails
            {
                Title  = "Student not found",
                Detail = $"No student with ID {request.StudentId} exists.",
                Status = StatusCodes.Status404NotFound
            });

        // 404 — course must exist
        var course = await courseService.GetByIdAsync(request.CourseId, ct);
        if (course is null)
            return NotFound(new ProblemDetails
            {
                Title  = "Course not found",
                Detail = $"No course with ID {request.CourseId} exists.",
                Status = StatusCodes.Status404NotFound
            });

        // 409 — serial number must be unique
        if (await certificateService.SerialNumberExistsAsync(request.SerialNumber, ct))
            return Conflict(new ProblemDetails
            {
                Title  = "Serial number already exists",
                Detail = $"A certificate with serial number '{request.SerialNumber}' is already issued.",
                Status = StatusCodes.Status409Conflict
            });

        var created = await certificateService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetCertificateById), new { id = created.Id }, created);
    }

    // ── DELETE certificate ────────────────────────────────────
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete a certificate")]
    [EndpointDescription("Permanently removes the certificate. This action cannot be undone.")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await certificateService.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
