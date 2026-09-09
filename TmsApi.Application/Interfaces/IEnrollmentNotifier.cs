namespace TmsApi.Application.Interfaces;

public interface IEnrollmentNotifier
{
    Task EnrollmentCreatedAsync(
        object enrollment,
        CancellationToken ct);
}