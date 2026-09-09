namespace TmsApi.Application.Dtos;
public record EnrollmentSummaryDto(
    int Id,
    int StudentId,
    string StudentName,
    int CourseId,
    string CourseName,
    string Status,
    DateTime EnrolledAt);
