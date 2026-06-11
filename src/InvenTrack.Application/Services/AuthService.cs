using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using InvenTrack.Application.DTOs.Auth;
using InvenTrack.Application.Interfaces;
using InvenTrack.Domain.Entities;
using InvenTrack.Domain.Enums;
using InvenTrack.Shared.Constants;
using InvenTrack.Shared.Wrappers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace InvenTrack.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly IEmailService _emailService;

    public AuthService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration, IMemoryCache cache, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _configuration = configuration;
        _cache = cache;
        _emailService = emailService;
    }

    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request, string? ipAddress = null, string? userAgent = null)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

        if (user == null)
            return ApiResponse<LoginResponseDto>.Fail("Invalid email or password");

        if (!user.IsActive)
            return ApiResponse<LoginResponseDto>.Fail("Account is deactivated. Contact administrator.");

        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            return ApiResponse<LoginResponseDto>.Fail($"Account is locked until {user.LockoutEnd:HH:mm:ss UTC}. Try again later.");

        // SECURITY: BCrypt password verification with 12 rounds
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            // SECURITY A07: Account lockout after failed attempts
            if (user.FailedLoginAttempts >= AppConstants.Limits.AuthLockoutAttempts)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(AppConstants.Limits.LockoutMinutes);
                user.FailedLoginAttempts = 0;
            }
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<LoginResponseDto>.Fail("Invalid email or password");
        }

        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;

        var (accessToken, expiresAt) = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(AppConstants.Limits.RefreshTokenExpirationDays);

        await _unitOfWork.SaveChangesAsync();

        // SECURITY: Audit log for login
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            UserId = user.Id,
            Action = "Login",
            EntityName = "User",
            EntityId = user.Id.ToString(),
            IpAddress = ipAddress,
            UserAgent = userAgent
        });
        await _unitOfWork.SaveChangesAsync();

        var response = new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = _mapper.Map<UserInfoDto>(user)
        };

        return ApiResponse<LoginResponseDto>.Ok(response, "Login successful");
    }

    public async Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(string refreshToken, string? ipAddress = null)
    {
        var user = await _unitOfWork.Users.GetByRefreshTokenAsync(refreshToken);

        if (user == null || user.RefreshTokenExpiry <= DateTime.UtcNow)
            return ApiResponse<LoginResponseDto>.Fail("Invalid or expired refresh token");

        // SECURITY: Refresh token rotation
        var (accessToken, expiresAt) = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(AppConstants.Limits.RefreshTokenExpirationDays);

        await _unitOfWork.SaveChangesAsync();

        var response = new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = expiresAt,
            User = _mapper.Map<UserInfoDto>(user)
        };

        return ApiResponse<LoginResponseDto>.Ok(response, "Token refreshed");
    }

    public async Task<ApiResponse<string>> LogoutAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return ApiResponse<string>.Fail("User not found");

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<string>.Ok("Logged out successfully");
    }

    public async Task<ApiResponse<UserInfoDto>> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        if (existingUser != null)
            return ApiResponse<UserInfoDto>.Fail("Email already registered");

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            role = UserRole.Staff;

        // SECURITY: BCrypt with 12 rounds
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12);
        var passwordSalt = BCrypt.Net.BCrypt.GenerateSalt(12);

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Role = role
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var userDto = _mapper.Map<UserInfoDto>(user);
        return ApiResponse<UserInfoDto>.Ok(userDto, "Registration successful");
    }

    public async Task<ApiResponse<string>> ChangePasswordAsync(int userId, ChangePasswordRequestDto request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return ApiResponse<string>.Fail("User not found");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return ApiResponse<string>.Fail("Current password is incorrect");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, 12);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<string>.Ok("Password changed successfully");
    }

    private (string token, DateTime expiresAt) GenerateAccessToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "InvenTrack";
        var audience = jwtSettings["Audience"] ?? "InvenTrack.Client";
        var expirationMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "15");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
