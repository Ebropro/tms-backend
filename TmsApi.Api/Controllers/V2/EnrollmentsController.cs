
using System.Security.Claims;

using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Enrollments.Queries;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers.V2;

// This controller receives HTTP requests and translates them
// into application commands/results.

[ApiController]
[Route("api/v{version:apiVersion}/enrollments")]
[ApiVersion("2.0")]
[Tags("Enrollments V2")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
[Authorize(Roles = "Student")]
public class EnrollmentsController(
    IMediator mediator,
    IStudentService studentService) : ControllerBase
{
    // ── POST enroll a student ─────────────────────────────────

    [HttpPost]
    [ProducesResponseType(
        typeof(EnrollmentCreated),
        StatusCodes.Status201Created)]

    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]

    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]

    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]

    [EndpointSummary("Enrol a student (V2)")]

    [EndpointDescription(
        "Routes through MediatR CQRS pipeline. " +
        "The authenticated student's Identity account determines " +
        "which student is enrolled.")]
    public async Task<IActionResult> Enroll(
        EnrollStudentCommand command,
        CancellationToken ct)
    {
        // ---------------------------------------------------------
        // 1. Get the authenticated Identity user ID.
        // ---------------------------------------------------------

        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        // ---------------------------------------------------------
        // 2. Resolve the Student associated with that Identity user.
        // ---------------------------------------------------------

        var studentId = await studentService.GetIdByUserIdAsync(
            userId,
            ct);

        if (studentId is null)
        {
            return Forbid();
        }

        // ---------------------------------------------------------
        // 3. IMPORTANT:
        //    Do NOT trust command.StudentId.
        //
        //    Replace it with the authenticated student's ID.
        // ---------------------------------------------------------

        var authenticatedCommand = new EnrollStudentCommand(
            studentId.Value,
            command.CourseCode);

        var result = await mediator.Send(
            authenticatedCommand,
            ct);

        // ---------------------------------------------------------
        // 4. Translate application result to HTTP.
        // ---------------------------------------------------------

        return result.Match<IActionResult>(
            onSuccess: created => CreatedAtAction(
                nameof(GetSchedule),
                new { studentId = created.StudentId },
                created),

            onFailure: error =>
            {
                var status = error.Code switch
                {
                    "course_not_found"
                        => StatusCodes.Status404NotFound,

                    "course_full" or "already_enrolled"
                        => StatusCodes.Status409Conflict,

                    _
                        => StatusCodes.Status400BadRequest
                };

                return Problem(
                    statusCode: status,
                    title: "Enrollment rejected",
                    detail: error.Message,
                    type: $"https://tms.local/errors/{error.Code}");
            });
    }

    // ── GET student schedule ──────────────────────────────────

    [HttpGet("{studentId:int}/schedule")]
    [ProducesResponseType(
        typeof(ScheduleDto),
        StatusCodes.Status200OK)]

    [EndpointSummary(
        "Get a student's course schedule (V2)")]

    public async Task<IActionResult> GetSchedule(
        int studentId,
        CancellationToken ct)
    {
        var schedule = await mediator.Send(
            new GetStudentScheduleQuery(studentId),
            ct);

        return Ok(schedule);
    }
}

