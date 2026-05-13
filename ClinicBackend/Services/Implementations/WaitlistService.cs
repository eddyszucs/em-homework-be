using Microsoft.EntityFrameworkCore;
using ClinicBackend.Data;
using ClinicBackend.Models.DTOs;
using ClinicBackend.Models.Enums;
using ClinicBackend.Models.StateMachine;
using ClinicBackend.Services.Interfaces;

namespace ClinicBackend.Services;

public class WaitlistService : IWaitlistService
{
    private readonly ClinicDbContext _db;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    public WaitlistService(ClinicDbContext db, IAuditService auditService, INotificationService notificationService)
    {
        _db = db;
        _auditService = auditService;
        _notificationService = notificationService;
    }

    public async Task<object> GetDoctorWaitlistAsync(Guid doctorId)
    {
        var patients = await _db.Patients
            .Include(p => p.AssignedDoctor)
            .Where(p => p.AssignedDoctorId == doctorId && !p.IsDeleted)
            .OrderBy(p => p.CreatedAt)
            .Select(p => new WaitlistEntryDto(
                p.Id, p.Name, p.Status, p.Specialty, p.AssignedDoctorId,
                p.AssignedDoctor != null ? p.AssignedDoctor.Username : null,
                p.CreatedAt, p.TajNumber))
            .ToListAsync();

        return patients;
    }

    public async Task<object> AssignPatientToDoctorAsync(Guid patientId, Guid doctorId, Guid assistantId)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == patientId && !p.IsDeleted)
            ?? throw new KeyNotFoundException("Patient not found");

        var doctor = await _db.Users.FirstOrDefaultAsync(u => u.Id == doctorId)
            ?? throw new KeyNotFoundException("Doctor not found");

        patient.AssignedDoctorId = doctorId;
        PatientStateMachine.AssignDoctor(patient, doctorId);
        await _db.SaveChangesAsync();

        await _auditService.LogAsync(assistantId, patient.Id, "PATIENT_ASSIGNED",
            $"Patient ID: {patient.Id}, Doctor ID: {doctorId}, Specialty: {patient.Specialty}");
        await _notificationService.NotifyPatientStatusChangedAsync(patient.Id, doctorId, patient.Status.ToString());

        return new WaitlistEntryDto(patient.Id, patient.Name, patient.Status, patient.Specialty,
            patient.AssignedDoctorId,
            patient.AssignedDoctor != null ? patient.AssignedDoctor.Username : null,
            patient.CreatedAt, patient.TajNumber);
    }

    public async Task<object> CallPatientAsync(Guid patientId, Guid doctorId)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == patientId && !p.IsDeleted)
            ?? throw new KeyNotFoundException("Patient not found");

        if (patient.AssignedDoctorId != doctorId)
            throw new ForbiddenException("Only the assigned doctor can call this patient.");

        PatientStateMachine.Call(patient, doctorId);
        await _db.SaveChangesAsync();

        await _auditService.LogAsync(doctorId, patient.Id, "PATIENT_CALLED",
            $"Patient ID: {patient.Id}, Doctor ID: {doctorId}");
        await _notificationService.NotifyPatientStatusChangedAsync(patient.Id, doctorId, patient.Status.ToString());

        return new StateTransitionResult(patient.Id, patient.Status.ToString()!);
    }

    public async Task<object> ReleasePatientAsync(Guid patientId, Guid doctorId)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == patientId && !p.IsDeleted)
            ?? throw new KeyNotFoundException("Patient not found");

        if (patient.AssignedDoctorId != doctorId)
            throw new ForbiddenException("Only the assigned doctor can release this patient.");

        PatientStateMachine.Release(patient, doctorId);
        await _db.SaveChangesAsync();

        await _auditService.LogAsync(doctorId, patient.Id, "PATIENT_RELEASED",
            $"Patient ID: {patient.Id}, Doctor ID: {doctorId}");
        await _notificationService.NotifyPatientStatusChangedAsync(patient.Id, doctorId, patient.Status.ToString());

        return new StateTransitionResult(patient.Id, patient.Status.ToString()!);
    }
}