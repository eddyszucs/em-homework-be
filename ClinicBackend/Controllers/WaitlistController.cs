using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicBackend.Models.DTOs;
using ClinicBackend.Services;
using ClinicBackend.Services.Interfaces;

namespace ClinicBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WaitlistController : ControllerBase
{
    private readonly IWaitlistService _waitlistService;

    public WaitlistController(IWaitlistService waitlistService)
    {
        _waitlistService = waitlistService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string GetUserRole() => User.FindFirstValue(ClaimTypes.Role)!;

    [HttpGet]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetDoctorWaitlist()
    {
        var result = await _waitlistService.GetDoctorWaitlistAsync(GetUserId());
        return Ok(result);
    }

    [HttpPost("{patientId}/assign")]
    [Authorize(Roles = "Assistant")]
    public async Task<IActionResult> Assign(Guid patientId, [FromBody] Guid doctorId)
    {
        try
        {
            var result = await _waitlistService.AssignPatientToDoctorAsync(patientId, doctorId, GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", ex.Message));
        }
    }

    [HttpPost("{patientId}/call")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> Call(Guid patientId)
    {
        try
        {
            var result = await _waitlistService.CallPatientAsync(patientId, GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", ex.Message));
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(403, new ErrorResponse("FORBIDDEN", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse("INVALID_STATE_TRANSITION", ex.Message));
        }
    }

    [HttpPost("{patientId}/release")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> Release(Guid patientId)
    {
        try
        {
            var result = await _waitlistService.ReleasePatientAsync(patientId, GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", ex.Message));
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(403, new ErrorResponse("FORBIDDEN", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse("INVALID_STATE_TRANSITION", ex.Message));
        }
    }
}