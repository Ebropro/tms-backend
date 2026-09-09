using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Configurations;
public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.RegistrationNumber).IsRequired().HasMaxLength(20);
        builder.Property(s => s.GPA).HasPrecision(3, 2);
        builder.Property(s => s.IsActive).IsRequired();
        builder.Property(s => s.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property<DateTime>("LastUpdated")
            .HasColumnType("timestamp without time zone");
        builder.Property(s => s.Version)
            .IsConcurrencyToken();
        builder.HasQueryFilter(s => !s.IsDeleted);
        builder.Property(s => s.UserId)
    .HasMaxLength(450);

        builder.HasIndex(s => s.UserId)
            .IsUnique()
            .HasFilter("\"UserId\" IS NOT NULL");
    }
}