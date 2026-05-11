using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ClinicBackend.Data;
using ClinicBackend.Models.DTOs;
using ClinicBackend.Models.Enums;
using ClinicBackend.Services.Interfaces;
using BCrypt.Net;

namespace ClinicBackend.Services;

public class AuthService : IAuthService
{
    private readonly ClinicDbContext _db;
    private readonly IAuditService _auditService;
    private readonly IConfiguration _config;

    public AuthService(ClinicDbContext db, IAuditService auditService, IConfiguration config)
    {
        _db = db;
        _auditService = auditService;
        _config = config;
    }

    public async Task<(string Token, string RefreshToken, DateTime ExpiresAt, object User)> LoginAsync(string username, string password)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            await _auditService.LogAsync(Guid.Empty, null, "LOGIN_FAILED", $"Username attempted: {username}");
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        var role = user.Role == UserRole.Assistant ? "Assistant" : "Doctor";
        var token = GenerateJwtToken(user.Id, user.Username, role, user.Specialty);
        var refreshToken = GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        await _auditService.LogAsync(user.Id, null, "LOGIN_SUCCESS", $"Username: {user.Username}");

        var userDto = new UserDto(user.Id, user.Username, role, user.Specialty);
        return (token, refreshToken, expiresAt, userDto);
    }

    public async Task<(string Token, string RefreshToken, DateTime ExpiresAt)> RefreshAsync(string refreshToken)
    {
        // Simplified: in production, store refresh tokens in DB
        throw new NotImplementedException();
    }

    public string GenerateJwtToken(Guid userId, string username, string role, string? specialty)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim("specialty", specialty ?? "")
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60")),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
    }
}