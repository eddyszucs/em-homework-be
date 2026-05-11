namespace ClinicBackend.Models.DTOs;

public record LoginRequest(string Username, string Password);

public record LoginResponse(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

public record UserDto(Guid Id, string Username, string Role, string? Specialty);

public record RefreshRequest(string RefreshToken);