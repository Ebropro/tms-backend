using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly TmsDbContext _context;

    public ReportsController(TmsDbContext context)
    {
        _context = context;
    }

//  PAGINATED STUDENTS LIST

    [HttpGet("students")]
    public async Task<IActionResult> GetStudents(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _context.Students
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                s.Id,
                s.Name,
                GPA = s.GPA,
                s.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(result);
    }

//TOP 5 COURSES BY ENROLLMENT
    [HttpGet("top-courses")]
    public async Task<IActionResult> GetTopCourses()
    {
        var result = await _context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .Take(5)
            .ToListAsync();

        return Ok(result);
    }


     //  fix N+1
    [HttpGet("students-enrollment-count")]
    public async Task<IActionResult> GetStudentEnrollmentCount(CancellationToken cancellationToken)
    {
        var result = await _context.Students
            .AsNoTracking()
            .GroupJoin(
                _context.Enrollments,
                s => s.Id,
                e => e.StudentId,
                (s, enrollments) => new
                {
                    s.Name,
                    Count = enrollments.Count()
                }
            )
            .ToListAsync(cancellationToken);

        return Ok(result);
    }

}