using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicBackend.Services.Interfaces;

namespace ClinicBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditLogController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditLogController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs(
        [FromQuery] Guid? patientId,
        [FromQuery] Guid? userId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var result = await _auditService.QueryAsync(patientId, userId, from, to);
        return Ok(result);
    }
}