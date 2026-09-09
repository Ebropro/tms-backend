using Microsoft.AspNetCore.Identity;
using TmsApi.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence;
public static class DataSeeder
{
    // Deterministic 
    private static readonly (string Code, string Title, int MaxCapacity)[] Courses =
    [
        ("CSE-101", "Web Development Fundamentals", 30),
        ("CSE-102", "TypeScript Essentials", 30),
        ("CSE-103", "Git and Collaborative Workflows", 25),
        ("CSE-201", "ASP.NET Core Basics", 28),
        ("CSE-202", "Entity Framework Core and PostgreSQL", 28),
        ("CSE-203", "Building RESTful Web APIs", 28),
        ("CSE-301", "Advanced Web API Patterns", 24),
        ("CSE-302", "Angular Basics", 26),
        ("CSE-303", "Angular Advanced", 24),
        ("CSE-304", "Full-Stack Integration", 22),
        ("CSE-305", "Testing and Quality Assurance", 22),
        ("CSE-306", "Security and Authentication", 20),
        ("DAT-101", "Database Design Foundations", 30),
        ("DAT-201", "Advanced SQL and Indexing", 26),
        ("DAT-202", "Data Modelling for the Web", 26),
        ("ARC-101", "Software Architecture Patterns", 22),
        ("ARC-201", "Cloud-Native Architecture", 22),
        ("DEV-101", "DevOps Foundations", 24),
        ("DEV-201", "Continuous Delivery Pipelines", 22),
        ("MOB-101", "Mobile App Foundations", 24),
        ("MOB-201", "Cross-Platform Mobile", 22),
        ("AI-101", "Applied Machine Learning", 20),
        ("AI-201", "Generative AI for Developers", 18),
        ("UX-101", "UX Research and Wireframing", 24),
        ("UX-201", "Design Systems and Tokens", 22),
    ];

    private static readonly (
        string Email,
        string FirstName,
        string LastName,
        string Password
    )[] Instructors =
    [
        (
            "leul.instructor@cotbe.edu.et",
            "Leul",
            "Instructor",
            "Leul.Instructor2026!" // was "" — fixed, would have thrown on CreateAsync
        ),
        (
            "lel.instructor@cotbe.edu.et",
            "Lel",
            "Instructor",
            "Lel.Instructor2026!" // was "" — fixed, would have thrown on CreateAsync
        ),
        (
            "instructor3@cotbe.edu.et",
            "Instructor",
            "Three",
            "Instructor3@TMS2026!"
        ),
        (
            "instructor4@cotbe.edu.et",
            "Instructor",
            "Four",
            "Instructor4@TMS2026!"
        ),
        (
            "instructor5@cotbe.edu.et",
            "Instructor",
            "Five",
            "Instructor5@TMS2026!"
        )
    ];

    private static readonly (string RegistrationNumber, string Name, int Age, decimal GPA)[] Students =
    [
        ("TMS-2026-0006", "Abebe Mola", 22, 3.8m),
        ("TMS-2026-0007", "John Brown", 24, 3.5m),
        ("TMS-2026-0008", "Sara Ahmed", 21, 3.9m),
        ("TMS-2026-0009", "David Wilson", 23, 3.29m),
        ("TMS-2026-0010", "Seidu Ketta", 20, 3.7m),
        ("TMS-2026-0011", "Alice Smith", 27, 2.81m),
        ("TMS-2026-0012", "Andrew Haris", 24, 2.56m),
        ("TMS-2026-0013", "Peter Smith", 26, 2.93m),
        ("TMS-2026-0014", "John Doe", 23, 2.2m),
        ("TMS-2026-0015", "Bilal Amin", 28, 2.77m),
        ("TMS-2026-0016", "kebede chala", 22, 3.1m),
        ("TMS-2026-0017", "Allian Robben", 24, 3.6m),
        ("TMS-2026-0018", "Hikma Hassen", 21, 3.38m),
        ("TMS-2026-0019", "Girum Beyene", 23, 3.27m),
        ("TMS-2026-0020", "Justin King", 25, 3.7m),
        ("TMS-2026-0021", "Zahara Ali", 29, 3.8m),
        ("TMS-2026-0022", "Tyson Fury", 24, 3.5m),
        ("TMS-2026-0023", "Amanda Thomas", 21, 2.2m),
        ("TMS-2026-0024", "Ashley Taylor", 23, 3.2m),
        ("TMS-2026-0025", "Mary Johnson", 27, 3.42m),
        ("TMS-2026-0026", "Jessica Miller", 25, 3.8m),
        ("TMS-2026-0027", "Sarah Davis", 24, 2.6m),
        ("TMS-2026-0028", "Michael Brown", 21, 2.9m),
        ("TMS-2026-0029", "Emily Johnson", 23, 4.0m),
        ("TMS-2026-0030", "John Smith", 29, 3.7m),
    ];

    private static async Task<Dictionary<string, TmsUser>> SeedInstructorsAsync(
        UserManager<TmsUser> userManager,
        RoleManager<IdentityRole> roleManager,
        CancellationToken ct)
    {
        const string instructorRole = "Instructor";

        if (!await roleManager.RoleExistsAsync(instructorRole))
        {
            var roleResult = await roleManager.CreateAsync(
                new IdentityRole(instructorRole));

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not create Instructor role: " +
                    $"{string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }

        var instructors = new Dictionary<string, TmsUser>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var seed in Instructors)
        {
            var user = await userManager.FindByEmailAsync(seed.Email);

            if (user is null)
            {
                user = new TmsUser
                {
                    UserName = seed.Email,
                    Email = seed.Email,
                    EmailConfirmed = true,
                    FirstName = seed.FirstName,
                    LastName = seed.LastName
                };

                if (string.IsNullOrWhiteSpace(seed.Password))
                {
                    throw new InvalidOperationException(
                        $"Password is required when creating seeded user {seed.Email}.");
                }

                var createResult = await userManager.CreateAsync(
                    user,
                    seed.Password);

                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Could not create {seed.Email}: " +
                        $"{string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, instructorRole))
            {
                var roleResult = await userManager.AddToRoleAsync(
                    user,
                    instructorRole);

                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Could not assign Instructor role to {seed.Email}: " +
                        $"{string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }

            instructors[seed.Email] = user;
        }

        return instructors;
    }

    private static async Task AssignCourseInstructorsAsync(
        TmsDbContext context,
        Dictionary<string, TmsUser> instructors,
        CancellationToken ct)
    {
        var assignments = new Dictionary<string, string[]>
        {
            ["leul.instructor@cotbe.edu.et"] =
            [
                "CSE-101",
                "CSE-102",
                "CSE-103",
                "CSE-201",
                "CSE-202"
            ],

            ["lel.instructor@cotbe.edu.et"] =
            [
                "CSE-203",
                "CSE-301",
                "CSE-302",
                "CSE-303",
                "CSE-304"
            ],

            ["instructor3@cotbe.edu.et"] =
            [
                "CSE-305",
                "CSE-306",
                "DAT-101",
                "DAT-201",
                "DAT-202"
            ],

            ["instructor4@cotbe.edu.et"] =
            [
                "ARC-101",
                "ARC-201",
                "DEV-101",
                "DEV-201",
                "MOB-101"
            ],

            ["instructor5@cotbe.edu.et"] =
            [
                "MOB-201",
                "AI-101",
                "AI-201",
                "UX-101",
                "UX-201"
            ]
        };

        foreach (var (email, courseCodes) in assignments)
        {
            var instructor = instructors[email];

            foreach (var courseCode in courseCodes)
            {
                var course = await context.Courses
                    .FirstOrDefaultAsync(
                        c => c.Code == courseCode,
                        ct);

                if (course is null)
                {
                    continue;
                }

                course.InstructorId = instructor.Id;
            }
        }

        await context.SaveChangesAsync(ct);
    }

    private static async Task SeedStudentAccountsAsync(
        TmsDbContext context,
        UserManager<TmsUser> userManager,
        CancellationToken ct)
    {
        const string password = "Student@2026!";

        var studentAccounts = new[]
        {
            ("TMS-2026-0006", "abebe.student@cotbe.edu.et", "Abebe", "Mola"),
            ("TMS-2026-0007", "john.student@cotbe.edu.et", "John", "Brown"),
            ("TMS-2026-0008", "sara.student@cotbe.edu.et", "Sara", "Ahmed"),
            ("TMS-2026-0009", "david.student@cotbe.edu.et", "David", "Wilson"),
            ("TMS-2026-0010", "seidu.student@cotbe.edu.et", "Seidu", "Ketta")
        };

        foreach (var account in studentAccounts)
        {
            var student = await context.Students
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    s => s.RegistrationNumber == account.Item1,
                    ct);

            if (student is null)
                continue;

            var user = await userManager.FindByEmailAsync(account.Item2);

            if (user is null)
            {
                user = new TmsUser
                {
                    UserName = account.Item2,
                    Email = account.Item2,
                    EmailConfirmed = true,
                    FirstName = account.Item3,
                    LastName = account.Item4
                };

                var result = await userManager.CreateAsync(
                    user,
                    password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        result.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(
                        $"Failed to create student user {account.Item2}: {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, "Student"))
            {
                var result = await userManager.AddToRoleAsync(
                    user,
                    "Student");

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        result.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(
                        $"Failed to assign Student role to {account.Item2}: {errors}");
                }
            }

            if (student.UserId is not null &&
                student.UserId != user.Id)
            {
                throw new InvalidOperationException(
                    $"Student {student.RegistrationNumber} is already linked to another user.");
            }

            var alreadyLinked = await context.Students
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    s => s.UserId == user.Id &&
                         s.Id != student.Id,
                    ct);

            if (alreadyLinked is not null)
            {
                throw new InvalidOperationException(
                    $"User {account.Item2} is already linked to " +
                    $"student {alreadyLinked.RegistrationNumber}.");
            }

            if (student.UserId != user.Id)
            {
                student.UserId = user.Id;
            }
        }

        await context.SaveChangesAsync(ct);
    }

    public static async Task SeedAsync(
        TmsDbContext context,
        UserManager<TmsUser> userManager,
        RoleManager<IdentityRole> roleManager,
        CancellationToken ct = default)
    {
        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync(ct);
        }

        var instructors = await SeedInstructorsAsync(
            userManager,
            roleManager,
            ct);

        // Seed courses
        if (!await context.Courses.AnyAsync(ct))
        {
            foreach (var (code, title, maxCapacity) in Courses)
            {
                context.Courses.Add(new Course
                {
                    Code = code,
                    Title = title,
                    MaxCapacity = maxCapacity
                });
            }

            await context.SaveChangesAsync(ct);
        }

        await AssignCourseInstructorsAsync(
            context,
            instructors,
            ct);

        // Seed students
        if (!await context.Students.AnyAsync(ct))
        {
            foreach (var (registrationNumber, name, age, gpa) in Students)
            {
                context.Students.Add(new Student
                {
                    RegistrationNumber = registrationNumber,
                    Name = name,
                    Age = age,
                    GPA = gpa,
                    IsActive = true,
                    IsDeleted = false
                });
            }

            await context.SaveChangesAsync(ct);
        }

        // Seed Identity accounts and link them to students
        await SeedStudentAccountsAsync(
            context,
            userManager,
            ct);

        if (!await context.Assessments.AnyAsync(ct))
        {
            // Find the courses we need by code — safer than assuming IDs
            var cse101 = await context.Courses.FirstOrDefaultAsync(c => c.Code == "CSE-101", ct);
            var cse201 = await context.Courses.FirstOrDefaultAsync(c => c.Code == "CSE-201", ct);
            var cse202 = await context.Courses.FirstOrDefaultAsync(c => c.Code == "CSE-202", ct);
            var dat101 = await context.Courses.FirstOrDefaultAsync(c => c.Code == "DAT-101", ct);

            var assessments = new List<Assessment>();

            // CSE-101 Web Development Fundamentals
            if (cse101 is not null)
            {
                assessments.AddRange([
                    new Assessment { CourseId = cse101.Id, Title = "HTML & CSS Quiz",       MaxScore = 100, Weight = 20 },
                    new Assessment { CourseId = cse101.Id, Title = "JavaScript Assignment",  MaxScore = 100, Weight = 30 },
                    new Assessment { CourseId = cse101.Id, Title = "Final Project",          MaxScore = 100, Weight = 50 },
                ]);
            }

            // CSE-201 ASP.NET Core Fundamentals
            if (cse201 is not null)
            {
                assessments.AddRange([
                    new Assessment { CourseId = cse201.Id, Title = "Middleware Lab",         MaxScore = 100, Weight = 25 },
                    new Assessment { CourseId = cse201.Id, Title = "Controller Assignment",  MaxScore = 100, Weight = 35 },
                    new Assessment { CourseId = cse201.Id, Title = "API Capstone",           MaxScore = 100, Weight = 40 },
                ]);
            }

            // CSE-202 EF Core and PostgreSQL
            if (cse202 is not null)
            {
                assessments.AddRange([
                    new Assessment { CourseId = cse202.Id, Title = "Migration Exercise",     MaxScore = 100, Weight = 30 },
                    new Assessment { CourseId = cse202.Id, Title = "Query Optimisation Lab", MaxScore = 100, Weight = 70 },
                ]);
            }

            // DAT-101 Database Design Foundations
            if (dat101 is not null)
            {
                assessments.AddRange([
                    new Assessment { CourseId = dat101.Id, Title = "ER Diagram Assignment",  MaxScore = 100, Weight = 40 },
                    new Assessment { CourseId = dat101.Id, Title = "Normalisation Quiz",     MaxScore = 100, Weight = 60 },
                ]);
            }

            if (assessments.Count > 0)
            {
                context.Assessments.AddRange(assessments);
                await context.SaveChangesAsync(ct);
            }
        }

        // ── Certificates ──────────────────────────────────────
        // Certificates require both a Student and a Course to exist
        // Idempotent: skip if any certificates already exist
        if (!await context.Certificates.AnyAsync(ct))
        {
            // Look up by known data — never assume IDs
            var firstStudent = await context.Students
                .IgnoreQueryFilters()   // bypass IsDeleted filter
                .FirstOrDefaultAsync(ct);

            var cse101 = await context.Courses
                .FirstOrDefaultAsync(c => c.Code == "CSE-101", ct);

            var cse201 = await context.Courses
                .FirstOrDefaultAsync(c => c.Code == "CSE-201", ct);

            // Only seed certificates if we have at least one student and course
            if (firstStudent is not null && cse101 is not null)
            {
                context.Certificates.Add(new Certificate
                {
                    SerialNumber = "CERT-2026-000001",
                    StudentId = firstStudent.Id,
                    CourseId = cse101.Id,
                    IssuedAt = new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc)
                });
            }

            if (firstStudent is not null && cse201 is not null)
            {
                context.Certificates.Add(new Certificate
                {
                    SerialNumber = "CERT-2026-000002",
                    StudentId = firstStudent.Id,
                    CourseId = cse201.Id,
                    IssuedAt = new DateTime(2026, 9, 20, 0, 0, 0, DateTimeKind.Utc)
                });
            }

            await context.SaveChangesAsync(ct);
        }

        // ── Enrollments ──────────────────────────────────────
        if (!await context.Enrollments.AnyAsync(ct))
        {
            // Lookup students by RegistrationNumber
            var student1 = await context.Students.FirstOrDefaultAsync(
                s => s.RegistrationNumber == "TMS-2026-0006", ct);

            var student2 = await context.Students.FirstOrDefaultAsync(
                s => s.RegistrationNumber == "TMS-2026-0007", ct);

            var student4 = await context.Students.FirstOrDefaultAsync(
                s => s.RegistrationNumber == "TMS-2026-0009", ct);

            // Lookup courses by Code
            var cse101 = await context.Courses.FirstOrDefaultAsync(
                c => c.Code == "CSE-101", ct);

            var cse201 = await context.Courses.FirstOrDefaultAsync(
                c => c.Code == "CSE-201", ct);

            var enrollments = new List<Enrollment>();

            if (student1 is not null && cse101 is not null)
            {
                enrollments.Add(new Enrollment
                {
                    StudentId = student1.Id,
                    CourseId = cse101.Id,
                    Grade = 4.0m
                });
            }

            if (student1 is not null && cse201 is not null)
            {
                enrollments.Add(new Enrollment
                {
                    StudentId = student1.Id,
                    CourseId = cse201.Id,
                    Grade = 3.6m
                });
            }

            if (student2 is not null && cse101 is not null)
            {
                enrollments.Add(new Enrollment
                {
                    StudentId = student2.Id,
                    CourseId = cse101.Id,
                    Grade = 2.8m
                });
            }

            if (student4 is not null && cse201 is not null)
            {
                enrollments.Add(new Enrollment
                {
                    StudentId = student4.Id,
                    CourseId = cse201.Id,
                    Grade = 3.9m
                });
            }

            if (enrollments.Count > 0)
            {
                context.Enrollments.AddRange(enrollments);
                await context.SaveChangesAsync(ct);
            }
        }
    }
}
