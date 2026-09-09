namespace TmsApi.Application.Dtos;

// Matches Angular's expected response: { id, success }
public record GradeSubmissionResult(string Id, bool Success);