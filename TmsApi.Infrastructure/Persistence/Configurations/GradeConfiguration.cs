using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Configurations;
public class GradeSubmissionConfiguration : IEntityTypeConfiguration<GradeSubmission>
{
    public void Configure(EntityTypeBuilder<GradeSubmission> builder)
    {
        builder.ToTable("GradeSubmissions");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Score).HasColumnType("decimal(5,2)");
        builder.Property(g => g.SubmittedAt).IsRequired();
    }
}