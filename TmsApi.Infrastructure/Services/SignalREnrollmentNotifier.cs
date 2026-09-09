using Microsoft.AspNetCore.SignalR;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Hubs;

namespace TmsApi.Infrastructure.Services;

public class SignalREnrollmentNotifier(
    IHubContext<EnrollmentHub> hub)
    : IEnrollmentNotifier
{
    public async Task EnrollmentCreatedAsync(
        object enrollment,
        CancellationToken ct)
    {
        await hub.Clients.All.SendAsync(
            "EnrollmentCreated",
            enrollment,
            ct);
    }
}