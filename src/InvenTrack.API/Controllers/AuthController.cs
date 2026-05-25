using InvenTrack.Application.DTOs.Auth;
using InvenTrack.Application.Interfaces;
using InvenTrack.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenTrack.API.Controllers;

public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        var result = await _authService.LoginAsync(request, ipAddress, userAgent);

        if (result.Success && result.Data != null)
        {
            // SECURITY A07: Store refresh token in HttpOnly cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(AppConstants.Limits.RefreshTokenExpirationDays)
            };
            HttpContext.Response.Cookies.Append("refreshToken", result.Data.RefreshToken, cookieOptions);
        }

        return ApiResponseOk(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = HttpContext.Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { success = false, message = "No refresh token provided" });

        var result = await _authService.RefreshTokenAsync(refreshToken);

        if (result.Success && result.Data != null)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(AppConstants.Limits.RefreshTokenExpirationDays)
            };
            HttpContext.Response.Cookies.Append("refreshToken", result.Data.RefreshToken, cookieOptions);
        }

        return ApiResponseOk(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await _authService.LogoutAsync(userId);

        HttpContext.Response.Cookies.Delete("refreshToken");

        return ApiResponseOk(result);
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        return ApiResponseOk(result);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await _authService.ChangePasswordAsync(userId, request);
        return ApiResponseOk(result);
    }
}
