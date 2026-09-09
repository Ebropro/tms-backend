namespace TmsApi.Domain.Entities;

public class GradeSubmission
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public decimal Score { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}