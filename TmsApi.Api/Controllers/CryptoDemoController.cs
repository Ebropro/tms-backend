using Microsoft.AspNetCore.Mvc;
using TmsApi.Infrastructure.Services;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/crypto-demo")]
public class CryptoDemoController : ControllerBase
{
    [HttpGet("test")]
    public IActionResult Test()
    {
        var service = new CryptoDemoService();

        string hash1 = service.HashUserPassword("Password123!");
        string hash2 = service.HashUserPassword("Password123!");

        bool match1 = service.VerifyUserPassword("Password123!", hash1);
        bool match2 = service.VerifyUserPassword("Password123!", hash2);

        Console.WriteLine($"Hash 1: {hash1}");
        Console.WriteLine($"Hash 2: {hash2}");
        Console.WriteLine($"Match 1: {match1}");
        Console.WriteLine($"Match 2: {match2}");

        return Ok(new
        {
            hash1,
            hash2,
            hashesAreDifferent = hash1 != hash2,
            match1,
            match2
        });
    }
}
