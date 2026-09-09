using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
[Authorize(Roles = "Instructor,Admin")]
public class CoursesController(
    TmsDbContext context,
    ICourseService courseService,
    ICachedCourseService cachedCourseService,
    IAuthorizationService authorizationService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var request = new PagedRequest
        {
            Page = page,
            PageSize = pageSize
        };

        var result = await cachedCourseService.GetCoursesAsync(request, ct);

        var rows = result.Items.Select(c => new
        {
            c.Id,
            c.Title,
            c.Code,
            c.MaxCapacity,
            c.EnrollmentCount
        });

        return Ok(new
        {
            data = rows,
            meta = new
            {
                totalCount = result.TotalCount,
                page = result.Page,
                pageSize = result.PageSize,
                totalPages = result.TotalPages,
                hasNext = result.HasNext,
                hasPrevious = result.HasPrevious
            },
            links = new
            {
                self = $"/api/v2/courses?page={page}&pageSize={pageSize}",

                next = result.HasNext
                    ? $"/api/v2/courses?page={page + 1}&pageSize={pageSize}"
                    : (string?)null,

                prev = result.HasPrevious
                    ? $"/api/v2/courses?page={page - 1}&pageSize={pageSize}"
                    : (string?)null,

                enroll = "/api/v2/enrollments"
            }
        });
    }
}