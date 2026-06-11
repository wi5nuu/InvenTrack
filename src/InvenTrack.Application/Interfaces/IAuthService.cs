using InvenTrack.Application.DTOs.Auth;
using InvenTrack.Shared.Wrappers;

namespace InvenTrack.Application.Interfaces;

/// <summary>Authentication and authorization service with JWT + refresh token rotation.</summary>
public interface IAuthService
{
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request, string? ipAddress = null, string? userAgent = null);
    Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(string refreshToken, string? ipAddress = null);
    Task<ApiResponse<string>> LogoutAsync(int userId);
    Task<ApiResponse<UserInfoDto>> RegisterAsync(RegisterRequestDto request);
    Task<ApiResponse<string>> ChangePasswordAsync(int userId, ChangePasswordRequestDto request);
}
