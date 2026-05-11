using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using ClinicBackend.Data;
using ClinicBackend.Models.Entities;
using ClinicBackend.Models.Enums;
using ClinicBackend.Services;
using ClinicBackend.Services.Interfaces;
using Xunit;

namespace ClinicBackend.Tests.Unit.Services;

public class AuthServiceTests
{
    private readonly ClinicDbContext _db;
    private readonly Mock<IAuditService> _auditMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new ClinicDbContext(options);
        _auditMock = new Mock<IAuditService>();

        var inMemorySettings = new System.Collections.Generic.Dictionary<string, string?>
        {
            ["Jwt:Key"] = "dev-secret-key-minimum-32-characters-long-for-hs256",
            ["Jwt:Issuer"] = "ClinicBackend",
            ["Jwt:Audience"] = "ClinicDesktop",
            ["Jwt:ExpiryMinutes"] = "60"
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings!).Build();

        _service = new AuthService(_db, _auditMock.Object, config);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenAndUser()
    {
        var user = new User
        {
            Username = "testuser",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            Role = UserRole.Assistant
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var (token, refreshToken, expiresAt, userDto) = await _service.LoginAsync("testuser", "Test123!");

        Assert.NotNull(token);
        Assert.NotNull(refreshToken);
        Assert.Equal("testuser", ((ClinicBackend.Models.DTOs.UserDto)userDto).Username);
        Assert.Equal("Assistant", ((ClinicBackend.Models.DTOs.UserDto)userDto).Role);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ThrowsUnauthorized()
    {
        var user = new User
        {
            Username = "testuser2",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Correct123!"),
            Role = UserRole.Doctor
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync("testuser2", "WrongPassword"));
    }

    [Fact]
    public async Task Login_WithNonexistentUser_ThrowsUnauthorized()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync("nouser", "anypassword"));
    }

    [Fact]
    public void GenerateJwtToken_ShouldContainCorrectClaims()
    {
        var userId = Guid.NewGuid();
        var token = _service.GenerateJwtToken(userId, "testuser", "Doctor", "Belgyógyász");

        Assert.NotNull(token);
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        Assert.Contains(jwt.Claims, c => c.Type == "specialty" && c.Value == "Belgyógyász");
    }
}