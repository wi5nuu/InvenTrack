using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using InvenTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace InvenTrack.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AccountController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        var result = await _authService.LoginAsync(new Application.DTOs.Auth.LoginRequestDto
        {
            Email = email,
            Password = password
        }, ipAddress, userAgent);

        if (result.Success && result.Data != null)
        {
            var token = result.Data.AccessToken;
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var identity = new ClaimsIdentity(jwtToken.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties { IsPersistent = true });

            // SECURITY: Store refresh token in HttpOnly cookie
            HttpContext.Response.Cookies.Append("refreshToken", result.Data.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return RedirectToAction("Index", "Dashboard");
        }

        ViewBag.Error = result.Message;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            await _authService.LogoutAsync(userId);
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Response.Cookies.Delete("refreshToken");

        return RedirectToAction("Login");
    }
}
