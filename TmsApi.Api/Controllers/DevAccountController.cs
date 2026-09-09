using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Identity;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers.Dev;


///   Development-only account provisioning tools.
///
/// The public sign-up flow only ever creates Student-role accounts
/// (AuthController.Register), and self-registration never creates or
/// links a Student domain row — so a freshly registered account can never
/// become an Instructor/Admin, and can never successfully enroll, without
/// some kind of manual provisioning. This controller is that provisioning
/// path for local development and demos.
///

[ApiController]
[Route("api/dev/accounts")]
[Authorize(Roles = "Admin")]
[Tags("Dev Tools (Development only)")]
public class DevAccountsController(
    UserManager<TmsUser> userManager,
    RoleManager<IdentityRole> roleManager,
    TmsDbContext context,
    IHostEnvironment environment) : ControllerBase
{
    public record PromoteRoleRequest(string Email, string Role);
    public record LinkStudentRequest(string Email, int? StudentId);
    private static readonly string[] PromotableRoles = ["Admin", "Instructor"];

    // Replaces all of the target account's roles with a single role —
    // "Admin" or "Instructor". Matches the rest of the app's
    [HttpPost("promote-role")]
    public async Task<IActionResult> PromoteRole(PromoteRoleRequest request)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        if (!PromotableRoles.Contains(request.Role))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid role",
                detail: $"Role must be one of: {string.Join(", ", PromotableRoles)}.");
        }

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Account not found",
                detail: $"No account exists for {request.Email}. Register the account first.");
        }

        if (!await roleManager.RoleExistsAsync(request.Role))
        {
            var createRole = await roleManager.CreateAsync(new IdentityRole(request.Role));
            if (!createRole.Succeeded)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Could not create role",
                    detail: string.Join("; ", createRole.Errors.Select(e => e.Description)));
            }
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Could not clear existing roles",
                    detail: string.Join("; ", removeResult.Errors.Select(e => e.Description)));
            }
        }

        var addResult = await userManager.AddToRoleAsync(user, request.Role);
        if (!addResult.Succeeded)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Could not assign role",
                detail: string.Join("; ", addResult.Errors.Select(e => e.Description)));
        }

        return Ok(new
        {
            message = $"{request.Email} is now {request.Role}. " +
                      "They must log out and back in for the new role to appear in their token.",
        });
    }

    // <summary>
    // Links a Student-role account to a Student record so it can actually enroll 
    // If StudentId is omitted, creates a brand-new Student row with
    // placeholder demo values 
    // </summary>
    [HttpPost("link-student")]
    public async Task<IActionResult> LinkStudent(LinkStudentRequest request, CancellationToken ct)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Account not found",
                detail: $"No account exists for {request.Email}. Register the account first.");
        }
        var alreadyLinked = await context.Students
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.UserId == user.Id, ct);

        if (alreadyLinked is not null)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Already linked",
                detail: $"{request.Email} is already linked to student #{alreadyLinked.Id}.");
        }

        Student student;

        if (request.StudentId is { } id)
        {
            var existing = await context.Students
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == id, ct);

            if (existing is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Student not found",
                    detail: $"No student with id {id} exists.");
            }

            if (existing.UserId is not null)
            {
                return Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Student already linked",
                    detail: $"Student #{id} is already linked to a different account.");
            }

            existing.UserId = user.Id;
            student = existing;
        }
        else
        {
            student = new Student
            {
                RegistrationNumber = $"DEV-{user.Id[..Math.Min(8, user.Id.Length)]}",
                Name = $"{user.FirstName} {user.LastName}".Trim(),
                Age = 20,
                GPA = 0,
                IsActive = true,
                UserId = user.Id,
            };

            context.Students.Add(student);
        }
        await context.SaveChangesAsync(ct);

        return Ok(new
        {
            studentId = student.Id,
            message = $"{request.Email} is now linked to student #{student.Id}.",
        });
    }
}
