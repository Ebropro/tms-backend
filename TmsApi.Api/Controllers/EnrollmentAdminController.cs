using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TmsApi.Api.Hubs;
using TmsApi.Application.Hubs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;

// instructor dashboard's SignalStore talk
[ApiController]
[Route("api/enrollments")]
public class EnrollmentAdminController(IEnrollmentService enrollmentService,
IHubContext<TmsHub, ITmsHubClient> hubContext
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var enrollments = await enrollmentService.GetAllSummaryAsync(ct);
        return Ok(enrollments);
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        var updated = await enrollmentService.ApproveAsync(id, ct);
        if (updated is null)
            return NotFound();
        
        // broadcast to all connected Angular clients
                await hubContext.Clients.All
        .ReceiveEnrollmentStatusUpdated(id.ToString(), "Approved");

        return Ok(updated);
    }
}
