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
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string GetUserRole() => User.FindFirstValue(ClaimTypes.Role)!;

    [HttpGet]
    [Authorize(Roles = "Assistant")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _patientService.GetAllAsync(GetUserId(), GetUserRole());
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _patientService.GetByIdAsync(id, GetUserId(), GetUserRole());
        if (result == null) return NotFound(new ErrorResponse("NOT_FOUND", "Patient not found"));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Assistant")]
    public async Task<IActionResult> Create([FromBody] CreatePatientRequest request)
    {
        try
        {
            var result = await _patientService.CreateAsync(request, GetUserId());
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ErrorResponse(ex.Error, ex.Message, ex.Field));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Assistant")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientRequest request)
    {
        try
        {
            var result = await _patientService.UpdateAsync(id, request, GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Patient not found"));
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ErrorResponse(ex.Error, ex.Message, ex.Field));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Assistant")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _patientService.DeleteAsync(id, GetUserId());
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Patient not found"));
        }
    }
}