using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicBackend.Models.DTOs;
using ClinicBackend.Services;
using ClinicBackend.Services.Interfaces;

namespace ClinicBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var (token, refreshToken, expiresAt, user) = await _authService.LoginAsync(request.Username, request.Password);
            return Ok(new LoginResponse(token, refreshToken, expiresAt, (UserDto)user));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ErrorResponse("INVALID_CREDENTIALS", ex.Message));
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        try
        {
            var (token, refreshToken, expiresAt) = await _authService.RefreshAsync(request.RefreshToken);
            return Ok(new { Token = token, RefreshToken = refreshToken, ExpiresAt = expiresAt });
        }
        catch (NotImplementedException)
        {
            return BadRequest(new ErrorResponse("NOT_IMPLEMENTED", "Refresh token not yet implemented"));
        }
    }
}