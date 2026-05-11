using Microsoft.EntityFrameworkCore;
using ClinicBackend.Data;
using ClinicBackend.Models.DTOs;
using ClinicBackend.Models.Enums;
using ClinicBackend.Models;
using ClinicBackend.Services.Interfaces;
using ClinicBackend.Validators;

namespace ClinicBackend.Services;

public class PatientService : IPatientService
{
    private readonly ClinicDbContext _db;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    public PatientService(ClinicDbContext db, IAuditService auditService, INotificationService notificationService)
    {
        _db = db;
        _auditService = auditService;
        _notificationService = notificationService;
    }

    public async Task<object> GetAllAsync(Guid userId, string role)
    {
        var query = _db.Patients.AsQueryable();

        var patients = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PatientDto(
                p.Id, p.Name, p.Address, p.TajNumber, p.Complaint, p.Specialty,
                p.Status, p.AssignedDoctorId,
                p.AssignedDoctor != null ? p.AssignedDoctor.Username : null,
                p.CreatedAt, p.UpdatedAt, null))
            .ToListAsync();

        return patients;
    }

    public async Task<object?> GetByIdAsync(Guid id, Guid userId, string role)
    {
        var p = await _db.Patients
            .Include(p => p.AssignedDoctor)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (p == null) return null;

        if (role == "Doctor")
        {
            return new PatientDto(
                p.Id, p.Name, p.Address, p.TajNumber, p.Complaint, p.Specialty,
                p.Status, p.AssignedDoctorId, p.AssignedDoctor?.Username,
                p.CreatedAt, p.UpdatedAt, null);
        }

        var diagnoses = await _db.Diagnoses
            .Where(d => d.PatientId == id)
            .Include(d => d.Doctor)
            .Select(d => new DiagnosisDto(d.Id, d.PatientId, d.DoctorId, d.Doctor.Username, d.Description, d.CreatedAt))
            .ToListAsync();

        return new PatientDto(
            p.Id, p.Name, p.Address, p.TajNumber, p.Complaint, p.Specialty,
            p.Status, p.AssignedDoctorId, p.AssignedDoctor?.Username,
            p.CreatedAt, p.UpdatedAt, diagnoses);
    }

    public async Task<PatientDto> CreateAsync(CreatePatientRequest req, Guid userId)
    {
        var validation = new CreatePatientRequestValidator().Validate(req);
        if (!validation.IsValid)
        {
            var firstError = validation.Errors.First();
            throw new ValidationException("VALIDATION_ERROR", firstError.ErrorMessage, firstError.PropertyName);
        }

        // Check if a patient with this TAJ number already exists
        var existing = await _db.Patients
            .Include(p => p.AssignedDoctor)
            .FirstOrDefaultAsync(p => p.TajNumber == req.TajNumber && !p.IsDeleted);

        if (existing != null)
        {
            // Auto-assign doctor based on specialty (load balancing)
            var doctor = await _db.Users
                .Where(u => u.Role == UserRole.Doctor && u.Specialty == req.Specialty && !u.IsDeleted)
                .OrderBy(u => u.AssignedPatients.Count(p => p.Status == PatientStatus.Waiting))
                .FirstOrDefaultAsync();

            existing.Name = req.Name;
            existing.Address = req.Address;
            existing.Complaint = req.Complaint;
            existing.Specialty = req.Specialty;
            existing.AssignedDoctorId = doctor?.Id;
            existing.Status = doctor != null ? PatientStatus.Waiting : PatientStatus.Recorded;
            existing.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _auditService.LogAsync(userId, existing.Id, "PATIENT_UPDATED",
                $"Updated via duplicate TAJ: {req.TajNumber}, Assigned: {doctor?.Username ?? "none"}");

            if (doctor != null)
                await _notificationService.NotifyPatientStatusChangedAsync(existing.Id, doctor.Id, existing.Status.ToString()!);

            // Notify assistant view of updated patient so their waitlist refreshes
            await _notificationService.NotifyPatientAddedAsync(existing.Id, existing.Name, existing.Specialty, existing.Status.ToString()!);

            return new PatientDto(existing.Id, existing.Name, existing.Address, existing.TajNumber,
                existing.Complaint, existing.Specialty, existing.Status, existing.AssignedDoctorId,
                doctor?.Username ?? null, existing.CreatedAt, existing.UpdatedAt, null);
        }

        // Auto-assign a doctor based on specialty
        var newDoctor = await _db.Users
            .Where(u => u.Role == UserRole.Doctor && u.Specialty == req.Specialty && !u.IsDeleted)
            .OrderBy(u => u.AssignedPatients.Count(p => p.Status == PatientStatus.Waiting))
            .FirstOrDefaultAsync();

        var patient = new Models.Entities.Patient
        {
            Name = req.Name,
            Address = req.Address,
            TajNumber = req.TajNumber,
            Complaint = req.Complaint,
            Specialty = req.Specialty,
            Status = newDoctor != null ? PatientStatus.Waiting : PatientStatus.Recorded,
            AssignedDoctorId = newDoctor?.Id,
        };

        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();

        await _auditService.LogAsync(userId, patient.Id, "PATIENT_CREATED",
            $"Name: {patient.Name}, TAJ: {patient.TajNumber}, Assigned: {newDoctor?.Username ?? "none"}");

        if (newDoctor != null)
            await _notificationService.NotifyPatientStatusChangedAsync(patient.Id, newDoctor.Id, patient.Status.ToString()!);
        else
            await _notificationService.NotifyPatientAddedAsync(patient.Id, patient.Name, patient.Specialty, patient.Status.ToString()!);

        return new PatientDto(patient.Id, patient.Name, patient.Address, patient.TajNumber,
            patient.Complaint, patient.Specialty, patient.Status, patient.AssignedDoctorId,
            newDoctor?.Username ?? null, patient.CreatedAt, patient.UpdatedAt, null);
    }

    public async Task<object> UpdateAsync(Guid id, UpdatePatientRequest req, Guid userId)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == id);
        if (patient == null) throw new KeyNotFoundException("Patient not found");

        var changes = new List<string>();

        if (!string.IsNullOrEmpty(req.Name) && req.Name != patient.Name)
        {
            changes.Add($"Name: {patient.Name} -> {req.Name}");
            patient.Name = req.Name;
        }
        if (!string.IsNullOrEmpty(req.Address) && req.Address != patient.Address)
        {
            changes.Add($"Address: {patient.Address} -> {req.Address}");
            patient.Address = req.Address;
        }
        if (!string.IsNullOrEmpty(req.TajNumber) && req.TajNumber != patient.TajNumber)
        {
            if (!TajNumberValidator.IsValid(req.TajNumber))
                throw new ValidationException("VALIDATION_ERROR", "Invalid TAJ number format", "TajNumber");
            changes.Add($"TajNumber: {patient.TajNumber} -> {req.TajNumber}");
            patient.TajNumber = req.TajNumber;
        }
        if (!string.IsNullOrEmpty(req.Complaint) && req.Complaint != patient.Complaint)
        {
            patient.Complaint = req.Complaint;
        }
        if (!string.IsNullOrEmpty(req.Specialty) && req.Specialty != patient.Specialty)
        {
            changes.Add($"Specialty: {patient.Specialty} -> {req.Specialty}");
            patient.Specialty = req.Specialty;
        }

        patient.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        if (changes.Any())
            await _auditService.LogAsync(userId, patient.Id, "PATIENT_UPDATED", string.Join(", ", changes));

        return new PatientDto(patient.Id, patient.Name, patient.Address, patient.TajNumber,
            patient.Complaint, patient.Specialty, patient.Status, patient.AssignedDoctorId,
            patient.AssignedDoctor?.Username, patient.CreatedAt, patient.UpdatedAt, null);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == id);
        if (patient == null) throw new KeyNotFoundException("Patient not found");

        patient.IsDeleted = true;
        patient.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _auditService.LogAsync(userId, patient.Id, "PATIENT_DELETED", $"Patient ID: {patient.Id}");
    }
}

public class ValidationException : Exception
{
    public string Error { get; }
    public string? Field { get; }
    public ValidationException(string error, string message, string? field = null) : base(message)
    {
        Error = error;
        Field = field;
    }
}