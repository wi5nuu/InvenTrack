using InvenTrack.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace InvenTrack.UnitTests.Services;

public class EmailServiceTests
{
    [Fact]
    public async Task QueueEmailAsync_ShouldNotThrow()
    {
        var configMock = new Mock<IConfiguration>();
        var loggerMock = new Mock<ILogger<EmailService>>();

        var service = new EmailService(configMock.Object, loggerMock.Object);

        await service.QueueEmailAsync("test@test.com", "Test Subject", "<p>Test body</p>");
    }
}
