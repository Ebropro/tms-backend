using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using TmsApi.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence;

public class TmsDbContext : IdentityDbContext<TmsUser>

{
    public TmsDbContext(DbContextOptions<TmsDbContext> options)
        : base(options)
    {
    }

    // ======================
    // DBSets (ONLY REGISTRY)
    // ======================
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<GradeSubmission> Grades => Set<GradeSubmission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();


    // ======================
    // FLUENT CONFIGURATION LOADER
    // ======================
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(TmsDbContext).Assembly
            
        );
    
    }
}