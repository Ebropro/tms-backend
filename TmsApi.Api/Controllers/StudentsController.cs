using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/students")]
[Tags("Students")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class StudentsController(
    IStudentService studentService,
    LinkGenerator linkGenerator) : ControllerBase
{
    // ── GET paginated list ────────────────────────────────────
    // Mirrors CoursesController.GetCourses exactly
    // [FromQuery] binds ?page=1&pageSize=10&search=alice from URL
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<StudentResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List students with pagination")]
    [EndpointDescription("Returns a paginated, optionally filtered list of TMS students. PageSize is capped at 50.")]
    public async Task<IActionResult> GetAll(
        [FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await studentService.GetAllAsync(request, ct);
        return Ok(result);
    }

    // ── GET single student with HATEOAS links ─────────────────
    // {id:int} route constraint — /api/students/abc → 404 at routing layer
    // Name = nameof(...) — used by CreatedAtAction and LinkGenerator
    [HttpGet("{id:int}", Name = nameof(GetStudentById))]
    [ProducesResponseType(typeof(StudentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a student by ID")]
    [EndpointDescription("Returns student details with HATEOAS links. Returns 404 if not found.")]
    public async Task<IActionResult> GetStudentById(int id, CancellationToken ct)
    {
        var student = await studentService.GetByIdAsync(id, ct);
        if (student is null) return NotFound();

        // Build hrefs using LinkGenerator — never string interpolation
        var selfHref = linkGenerator.GetPathByName(
            HttpContext, nameof(GetStudentById), new { id })!;

        var links = new List<LinkDto>
        {
            new(selfHref, "self",   "GET"),
            new(selfHref, "update", "PUT"),
            new(selfHref, "delete", "DELETE"),
            new(selfHref, "soft-delete", "DELETE"),
        };

        var detail = new StudentDetailDto
        {
            Id                 = student.Id,
            RegistrationNumber = student.RegistrationNumber,
            Name               = student.Name,
            Age                = student.Age,
            GPA                = student.GPA,
            IsActive           = student.IsActive,
            EnrollmentCount    = student.EnrollmentCount,
            Links              = links.AsReadOnly()
        };

        return Ok(detail);
    }

    [HttpPost]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new student")]
    [EndpointDescription("Creates a student with a unique registration number. Returns 409 if the registration number already exists.")]
    public async Task<IActionResult> Create(
        CreateStudentRequest request, CancellationToken ct)
    {
        // Check before hitting the database — same pattern as Course duplicate check
        if (await studentService.RegistrationNumberExistsAsync(request.RegistrationNumber, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title  = "Registration number already exists",
                Detail = $"A student with registration number '{request.RegistrationNumber}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var created = await studentService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetStudentById), new { id = created.Id }, created);
    }

    // ── PUT update student ────────────────────────────────────
    // Binds to UpdateStudentRequest — includes Version for concurrency
    // 409 on concurrency conflict (two users editing same student)
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Update a student")]
    [EndpointDescription("Updates student details. Returns 409 if the record was modified by another user since it was last read.")]
    public async Task<IActionResult> Update(
        int id, UpdateStudentRequest request, CancellationToken ct)
    {
        try
        {
            var updated = await studentService.UpdateAsync(id, request, ct);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new ProblemDetails
            {
                Title  = "Concurrency conflict",
                Detail = "The student record was updated by another user. Please reload and try again.",
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    // ── DELETE hard delete ────────────────────────────────────
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Hard delete a student")]
    [EndpointDescription("Permanently removes the student row. Use soft-delete to hide without removing.")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await studentService.DeleteAsync(id, ct) ? NoContent() : NotFound();

    // ── DELETE soft delete ────────────────────────────────────
    // Sets IsDeleted = true — student hidden from all normal queries
    [HttpDelete("{id:int}/soft")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Soft delete a student")]
    [EndpointDescription("Sets IsDeleted = true. Student disappears from normal queries but can be restored by an admin.")]
    public async Task<IActionResult> SoftDelete(int id, CancellationToken ct)
        => await studentService.SoftDeleteAsync(id, ct) ? NoContent() : NotFound();

    // ── Admin: GET all including soft-deleted ─────────────────
    [HttpGet("admin/all")]
    [ProducesResponseType(typeof(IReadOnlyList<StudentResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("Admin: list all students including soft-deleted")]
    [EndpointDescription("Bypasses HasQueryFilter via IgnoreQueryFilters(). Returns all students including IsDeleted = true rows.")]
    public async Task<IActionResult> GetAllIncludingDeleted(CancellationToken ct)
        => Ok(await studentService.GetAllIncludingDeletedAsync(ct));

    // ── Admin: POST restore soft-deleted student ──────────────
    [HttpPost("{id:int}/restore")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Admin: restore a soft-deleted student")]
    [EndpointDescription("Clears IsDeleted flag. Student reappears in normal queries.")]
    public async Task<IActionResult> Restore(int id, CancellationToken ct)
        => await studentService.RestoreAsync(id, ct) ? Ok() : NotFound();
}
