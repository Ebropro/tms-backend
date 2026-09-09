using Microsoft.AspNetCore.Mvc;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{
    [HttpGet("deferred")]
    public IActionResult Deferred()
    {
        Console.WriteLine("STEP 1: Building query (NO SQL yet)");

        var query = context.Students.Where(s => s.GPA >= 3.0m);

        Console.WriteLine("STEP 2: Adding ordering (still NO SQL)");

        var ordered = query.OrderBy(s => s.Name);

        Console.WriteLine("STEP 3: Executing query (NOW SQL RUNS)");

        var result = ordered.ToList(); // Execution is triggered

        Console.WriteLine("STEP 4: Done");

        return Ok(result);
    }
}