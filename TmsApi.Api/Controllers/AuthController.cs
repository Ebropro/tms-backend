using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Api.DTOs;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Identity;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using Microsoft.AspNetCore.RateLimiting;

namespace TmsApi.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<TmsUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly TmsDbContext _context;
    private readonly TokenService _tokenService;

    //
    private const string AccessTokenCookie = "tms_access_token";
    private const string RefreshTokenCookie = "tms_refresh_token";

    //

    public AuthController(
        UserManager<TmsUser> userManager,
        RoleManager<IdentityRole> roleManager,
        TmsDbContext context,
        TokenService tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _tokenService = tokenService;
    }

    // =========================================================
    // REGISTER
    // =========================================================

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequestDto request)
    {
        var existingUser =
            await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            // Prevent account enumeration
            return Ok(new
            {
                message = "Registration request received."
            });
        }

        var user = new TmsUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result =
            await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);

            return BadRequest(new { errors });
        }

        // Public registration is always a Student account.
        // Admin and Instructor accounts are provisioned separately.
        const string studentRole = "Student";

        if (!await _roleManager.RoleExistsAsync(studentRole))
        {
            var roleResult = await _roleManager.CreateAsync(
                new IdentityRole(studentRole));

            if (!roleResult.Succeeded)
            {
                return StatusCode(500, new
                {
                    detail = "Unable to provision the Student role."
                });
            }
        }

        var roleResultForUser = await _userManager.AddToRoleAsync(
            user,
            studentRole);

        if (!roleResultForUser.Succeeded)
        {
            return StatusCode(500, new
            {
                detail = "Unable to assign the Student role."
            });
        }

        return Ok(new
        {
            message = "Registration successful."
        });
    }

    // =========================================================
    // LOGIN
    // =========================================================
    [EnableRateLimiting("AuthLimiter")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto request)
    {
        var user =
            await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return Unauthorized(new
            {
                detail = "Invalid credentials."
            });
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return StatusCode(423, new
            {
                detail =
                    "Account locked due to multiple failed login attempts."
            });
        }

        var validPassword =
            await _userManager.CheckPasswordAsync(
                user,
                request.Password);

        if (!validPassword)
        {
            await _userManager.AccessFailedAsync(user);

            return Unauthorized(new
            {
                detail = "Invalid credentials."
            });
        }

        // Reset failed attempts after successful login
        await _userManager.ResetAccessFailedCountAsync(user);

        // Get user's roles
        var roles = await _userManager.GetRolesAsync(user);

        // Generate JWT access token
        var accessToken =
            _tokenService.GenerateJwt(user, roles);

        // Create initial refresh token
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);

        await _context.SaveChangesAsync();

        Response.Cookies.Append(
    "tms_access_token",
    accessToken,
    new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        Expires = DateTimeOffset.UtcNow.AddMinutes(15)
    });

        Response.Cookies.Append(
            "tms_refresh_token",
            refreshToken.Token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

        return Ok(new
        {
            accessToken,
            refreshToken = refreshToken.Token
        });
    }

    // =========================================================
    // REFRESH TOKEN ROTATION
    // =========================================================

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshRequestDto request)
    {
        var storedToken =
            await _context.RefreshTokens
                .FirstOrDefaultAsync(
                    rt => rt.Token == request.RefreshToken);

        // Token does not exist
        if (storedToken == null)
        {
            return Unauthorized(new
            {
                detail = "Invalid refresh token."
            });
        }

        // =====================================================
        // THEFT DETECTION
        // =====================================================
        // If an already-used refresh token is submitted,
        // revoke ALL refresh tokens belonging to that user.
        if (storedToken.IsUsed)
        {
            var userTokens =
                await _context.RefreshTokens
                    .Where(rt => rt.UserId == storedToken.UserId)
                    .ToListAsync();

            foreach (var token in userTokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();

            return Unauthorized(new
            {
                detail =
                    "Token theft detected. All user sessions revoked."
            });
        }

        // Token has been revoked or expired
        if (storedToken.IsRevoked ||
            storedToken.ExpiresAt < DateTime.UtcNow)
        {
            return Unauthorized(new
            {
                detail = "Refresh token expired or revoked."
            });
        }

        // =====================================================
        // ROTATE REFRESH TOKEN
        // =====================================================

        // Mark old token as used
        storedToken.IsUsed = true;

        // Create a completely new refresh token
        var newRefreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = storedToken.UserId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(newRefreshToken);

        // Find the user
        var user =
            await _userManager.FindByIdAsync(
                storedToken.UserId);

        if (user == null)
        {
            return Unauthorized(new
            {
                detail = "User associated with token not found."
            });
        }

        // Get user's roles
        var roles =
            await _userManager.GetRolesAsync(user);

        // Generate new JWT
        var newAccessToken =
            _tokenService.GenerateJwt(user, roles);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            accessToken = newAccessToken,
            refreshToken = newRefreshToken.Token
        });
    }
}
