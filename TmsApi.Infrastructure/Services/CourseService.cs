using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class CourseService(TmsDbContext context, ILogger<CourseService> logger) : ICourseService
{
 
        public Task<Course?> GetByCodeAsync(string code, CancellationToken ct) =>
            context.Courses
                .Include(c => c.Enrollments) // required for Enrollments.Count check
                .FirstOrDefaultAsync(c => c.Code == code, ct);

    public Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct) =>
        context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count))
            .FirstOrDefaultAsync(ct);

        

public async Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct)
{
    var course = new Course
    {
        Code = request.Code,
        Title = request.Title,
        MaxCapacity = request.MaxCapacity
    };

    context.Courses.Add(course);

await context.SaveChangesAsync(ct);

logger.LogInformation(
    "Created course {CourseId} ({Code})",
    course.Id,
    course.Code);

return (await GetByIdAsync(course.Id, ct))!;
}

    public async Task<CourseResponseDto?> UpdateAsync(int id, UpdateCourseRequest request, CancellationToken ct)
    {
        var course = await context.Courses.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (course is null)
            return null;

        course.Title = request.Title;
        if (request.MaxCapacity.HasValue)
            course.MaxCapacity = request.MaxCapacity.Value;

        await context.SaveChangesAsync(ct);

        logger.LogInformation("Updated course {CourseId} ({Code})", course.Id, course.Code);

        return await GetByIdAsync(course.Id, ct);
    }
    //

    public async Task<(bool Found, bool HasEnrollments, string? CourseCode)> DeleteAsync(
    int id,
    CancellationToken ct)
    {
        var course = await context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (course is null)
            return (false, false, null);

        if (course.Enrollments.Any())
            return (true, true, course.Code);

        context.Courses.Remove(course);

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Deleted course {CourseId} ({Code})",
            course.Id,
            course.Code);

        return (true, false, course.Code);
    }

    //
    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
    context.Courses.AsNoTracking().AnyAsync(c => c.Code == code, ct);

    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request, CancellationToken ct)
    {
 
        IQueryable<Course> query = context.Courses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c =>
                EF.Functions.ILike(c.Title, $"%{request.Search}%") ||
                EF.Functions.ILike(c.Code,  $"%{request.Search}%"));
        }

        var totalCount = await query.CountAsync(ct);

        IQueryable<Course> sorted = request.OrderBy switch
        {
            "Code"        => request.Descending ? query.OrderByDescending(c => c.Code)        : query.OrderBy(c => c.Code),
            "MaxCapacity" => request.Descending ? query.OrderByDescending(c => c.MaxCapacity) : query.OrderBy(c => c.MaxCapacity),
            _             => request.Descending ? query.OrderByDescending(c => c.Title)       : query.OrderBy(c => c.Title),
        };

        var items = await sorted
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CourseResponseDto(
                c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
            .ToListAsync(ct);

       
        return new PagedResponse<CourseResponseDto>
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = request.Page,
            PageSize   = request.PageSize
        };
    }

}


