using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers;

//Registrar’s Business Queries

[ApiController]
[Route("api/linq")]
public class LinqController(TmsDbContext context) : ControllerBase
{
    
    // Active students with GPA >= 3.0
   
    [HttpGet("active-students-count")]
    public async Task<IActionResult> GetActiveStudentsCount()
    {
        var count = await context.Students
            .Where(s => s.IsActive && s.GPA >= 3.0m)
            .CountAsync();

        return Ok(new { count });
    }

    // Courses with most enrollments (descending)
    [HttpGet("course-enrollments")]
    public async Task<IActionResult> GetCourseEnrollments()
    {
        var result = await context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync();

        return Ok(result);
    }

    
    // Average GPA per course  
    [HttpGet("course-average-gpa")]
    public async Task<IActionResult> GetCourseAverageGpa()
    {
        var result = await context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                Course = g.Key,
                AverageGPA = g.Average(e => e.Student.GPA)
            })
            .ToListAsync();

        return Ok(result);
    }

    //  Students with ZERO enrollments 
    [HttpGet("students-without-enrollments")]
    public async Task<IActionResult> GetStudentsWithoutEnrollments()
    {
        var result = await context.Students
            .Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .ToListAsync();

        return Ok(result);
    }
    //  Students with ZERO enrollments 
    [HttpGet("students-without-enrollments-leftjoin")]
    public async Task<IActionResult> GetStudentsWithoutEnrollmentsLeftJoin()
    {
        var result = await context.Students
            .GroupJoin(
                context.Enrollments,
                s => s.Id,
                e => e.StudentId,
                (s, e) => new { s, e }
            )
            .Where(x => !x.e.Any())
            .Select(x => x.s.Name)
            .ToListAsync();

        return Ok(result);
    }
}

