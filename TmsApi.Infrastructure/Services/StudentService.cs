using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;
public class StudentService(TmsDbContext context, ILogger<StudentService> logger) : IStudentService
{
    // ── Paginated list ────────────────────────────────────────

    public async Task<PagedResponse<StudentResponseDto>> GetAllAsync(
        PagedRequest request, CancellationToken ct)
    {
        // Step 1: no-tracking IQueryable — nothing hits DB yet
        IQueryable<Student> query = context.Students.AsNoTracking();

        // Step 2: optional search on Name or RegistrationNumber
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(s =>
                EF.Functions.ILike(s.Name, $"%{request.Search}%") ||
                EF.Functions.ILike(s.RegistrationNumber, $"%{request.Search}%"));
        }

        // Step 3: COUNT before paging — total matching rows, not page size
        var totalCount = await query.CountAsync(ct);

        // Step 4: sort — whitelist allowed columns, fall back to Name
        IQueryable<Student> sorted = request.OrderBy switch
        {
            "GPA"                => request.Descending ? query.OrderByDescending(s => s.GPA)  : query.OrderBy(s => s.GPA),
            "Age"                => request.Descending ? query.OrderByDescending(s => s.Age)  : query.OrderBy(s => s.Age),
            "RegistrationNumber" => request.Descending ? query.OrderByDescending(s => s.RegistrationNumber) : query.OrderBy(s => s.RegistrationNumber),
            _                    => request.Descending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
        };

        // Step 5: Skip/Take + Project + Execute — single SQL SELECT
        // Enrollments.Count → SQL COUNT(*) subquery, no rows loaded
        var items = await sorted
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new StudentResponseDto(
                s.Id,
                s.RegistrationNumber,
                s.Name,
                s.Age,
                s.GPA,
                s.IsActive,
                s.Enrollments.Count))
            .ToListAsync(ct);

        return new PagedResponse<StudentResponseDto>
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = request.Page,
            PageSize   = request.PageSize
        };
    }

    // ── Get single student ────────────────────────────────────
    // AsNoTracking + Select projection — no entity returned to controller
    public Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken ct) =>
        context.Students
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new StudentResponseDto(
                s.Id,
                s.RegistrationNumber,
                s.Name,
                s.Age,
                s.GPA,
                s.IsActive,
                s.Enrollments.Count))
            .FirstOrDefaultAsync(ct);

    // ── Create student ────────────────────────────────────────
    // Maps request DTO → entity → saves → re-queries through GetByIdAsync
    public async Task<StudentResponseDto> CreateAsync(
        CreateStudentRequest request, CancellationToken ct)
    {
        var student = new Student
        {
            RegistrationNumber = request.RegistrationNumber,
            Name               = request.Name,
            Age                = request.Age,
            GPA                = request.GPA,
            IsActive           = request.IsActive
        };

        context.Students.Add(student);
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created student {StudentId} ({RegistrationNumber})",
            student.Id, student.RegistrationNumber);

        return (await GetByIdAsync(student.Id, ct))!;
    }

    // ── Update student ────────────────────────────────────────
    // FindAsync respects HasQueryFilter — can't update soft-deleted students
    // Version sent back from client for optimistic concurrency
    public async Task<StudentResponseDto?> UpdateAsync(
        int id, UpdateStudentRequest request, CancellationToken ct)
    {
        var existing = await context.Students.FindAsync([id], ct);
        if (existing is null) return null;

        existing.Name      = request.Name;
        existing.Age       = request.Age;
        existing.GPA       = request.GPA;
        existing.IsActive  = request.IsActive;

        // Set original Version so EF can detect concurrent modifications
        context.Entry(existing)
            .Property(s => s.Version)
            .OriginalValue = request.Version;

        await context.SaveChangesAsync(ct);

        logger.LogInformation("Updated student {StudentId}", id);

        return (await GetByIdAsync(id, ct))!;
    }

    // ── Hard delete ───────────────────────────────────────────
    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var student = await context.Students.FindAsync([id], ct);
        if (student is null) return false;

        context.Students.Remove(student);
        await context.SaveChangesAsync(ct);
        return true;
    }

    // ── Soft delete ───────────────────────────────────────────
    // Sets IsDeleted = true — HasQueryFilter hides student from all normal queries
    public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
    {
        var student = await context.Students.FindAsync([id], ct);
        if (student is null) return false;

        student.IsDeleted = true;
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Soft-deleted student {StudentId}", id);
        return true;
    }

    // ── Admin: all students including soft-deleted ────────────
    // IgnoreQueryFilters() bypasses HasQueryFilter(!IsDeleted)
    public async Task<IReadOnlyList<StudentResponseDto>> GetAllIncludingDeletedAsync(
        CancellationToken ct) =>
        await context.Students
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Select(s => new StudentResponseDto(
                s.Id,
                s.RegistrationNumber,
                s.Name,
                s.Age,
                s.GPA,
                s.IsActive,
                s.Enrollments.Count))
            .ToListAsync(ct);

    // ── Admin: restore soft-deleted student ───────────────────
    // Must use IgnoreQueryFilters() to find the row first
    public async Task<bool> RestoreAsync(int id, CancellationToken ct)
    {
        var student = await context.Students
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (student is null) return false;

        student.IsDeleted = false;
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Restored student {StudentId}", id);
        return true;
    }

    // ── Duplicate registration number check ───────────────────
    // AnyAsync → SELECT EXISTS (SELECT 1 LIMIT 1) — stops at first match
    // Called before CreateAsync so we return 409 instead of crashing
    public Task<bool> RegistrationNumberExistsAsync(
        string registrationNumber, CancellationToken ct) =>
        context.Students
            .AsNoTracking()
            .AnyAsync(s => s.RegistrationNumber == registrationNumber, ct);

    
// ── Find student by Identity user ID ─────────────────────────
// Used by authenticated student enrollment.
// Returns the Student.Id associated with the logged-in Identity user.
public Task<int?> GetIdByUserIdAsync(
    string userId,
    CancellationToken ct) =>
    context.Students
        .AsNoTracking()
        .Where(s => s.UserId == userId)
        .Select(s => (int?)s.Id)
        .FirstOrDefaultAsync(ct);

        
}
