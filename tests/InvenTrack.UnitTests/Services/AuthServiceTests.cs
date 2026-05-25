using AutoMapper;
using InvenTrack.Application.DTOs.Auth;
using InvenTrack.Application.Interfaces;
using InvenTrack.Application.Services;
using InvenTrack.Domain.Entities;
using InvenTrack.Domain.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace InvenTrack.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly Mock<IEmailService> _emailMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _configMock = new Mock<IConfiguration>();
        _cacheMock = new Mock<IMemoryCache>();
        _emailMock = new Mock<IEmailService>();

        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(x => x["SecretKey"]).Returns("TestSuperSecretKeyThatIsLongEnough12345");
        jwtSection.Setup(x => x["Issuer"]).Returns("InvenTrack");
        jwtSection.Setup(x => x["Audience"]).Returns("InvenTrack.Client");
        jwtSection.Setup(x => x["AccessTokenExpirationMinutes"]).Returns("15");
        _configMock.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSection.Object);

        _authService = new AuthService(_unitOfWorkMock.Object, _mapperMock.Object, _configMock.Object, _cacheMock.Object, _emailMock.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccess()
    {
        var user = new User { Id = 1, Email = "test@test.com", Username = "test", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123", 12), Role = UserRole.Admin, IsActive = true };
        _unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync("test@test.com")).ReturnsAsync(user);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        _mapperMock.Setup(x => x.Map<UserInfoDto>(It.IsAny<User>())).Returns(new UserInfoDto { Id = 1, Username = "test", Email = "test@test.com", Role = "Admin" });

        var result = await _authService.LoginAsync(new LoginRequestDto { Email = "test@test.com", Password = "Password123" });

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ReturnsFailure()
    {
        var user = new User { Id = 1, Email = "test@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword", 12), IsActive = true };
        _unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync("test@test.com")).ReturnsAsync(user);

        var result = await _authService.LoginAsync(new LoginRequestDto { Email = "test@test.com", Password = "WrongPassword" });

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid");
    }

    [Fact]
    public async Task LoginAsync_WithLockedAccount_ReturnsFailure()
    {
        var user = new User { Id = 1, Email = "locked@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123", 12), IsActive = true, LockoutEnd = DateTime.UtcNow.AddMinutes(15) };
        _unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync("locked@test.com")).ReturnsAsync(user);

        var result = await _authService.LoginAsync(new LoginRequestDto { Email = "locked@test.com", Password = "Password123" });

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("locked");
    }

    [Fact]
    public async Task LoginAsync_WithDeactivatedAccount_ReturnsFailure()
    {
        var user = new User { Id = 1, Email = "inactive@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123", 12), IsActive = false };
        _unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync("inactive@test.com")).ReturnsAsync(user);

        var result = await _authService.LoginAsync(new LoginRequestDto { Email = "inactive@test.com", Password = "Password123" });

        result.Success.Should().BeFalse();
    }
}
