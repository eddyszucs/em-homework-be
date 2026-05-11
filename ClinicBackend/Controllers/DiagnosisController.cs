using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicBackend.Data;
using ClinicBackend.Models.DTOs;
using ClinicBackend.Models.Entities;
using ClinicBackend.Services.Interfaces;

namespace ClinicBackend.Controllers;

[ApiController]
[Route("api/patients/{patientId}/diagnosis")]
[Authorize(Roles = "Doctor")]
public class DiagnosisController : ControllerBase
{
    private readonly ClinicDbContext _db;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    public DiagnosisController(ClinicDbContext db, IAuditService auditService, INotificationService notificationService)
    {
        _db = db;
        _auditService = auditService;
        _notificationService = notificationService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> AddDiagnosis(Guid patientId, [FromBody] CreateDiagnosisRequest request)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == patientId && !p.IsDeleted);
        if (patient == null) return NotFound(new ErrorResponse("NOT_FOUND", "Patient not found"));

        var doctorId = GetUserId();
        var diagnosis = new Diagnosis
        {
            PatientId = patientId,
            DoctorId = doctorId,
            Description = request.Description
        };

        _db.Diagnoses.Add(diagnosis);
        await _db.SaveChangesAsync();

        await _auditService.LogAsync(doctorId, patientId, "DIAGNOSIS_ADDED",
            $"Patient ID: {patientId}, Doctor ID: {doctorId}");
        await _notificationService.NotifyDiagnosisAddedAsync(patientId, doctorId, request.Description);

        var result = new DiagnosisDto(diagnosis.Id, diagnosis.PatientId, diagnosis.DoctorId,
            User.FindFirstValue(ClaimTypes.Name)!, diagnosis.Description, diagnosis.CreatedAt);
        return CreatedAtAction(nameof(GetDiagnoses), new { patientId }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetDiagnoses(Guid patientId)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == patientId && !p.IsDeleted);
        if (patient == null) return NotFound(new ErrorResponse("NOT_FOUND", "Patient not found"));

        var doctorId = GetUserId();
        var diagnoses = await _db.Diagnoses
            .Where(d => d.PatientId == patientId && d.DoctorId == doctorId)
            .Include(d => d.Doctor)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DiagnosisDto(d.Id, d.PatientId, d.DoctorId, d.Doctor.Username,
                d.Description, d.CreatedAt))
            .ToListAsync();

        return Ok(diagnoses);
    }
}