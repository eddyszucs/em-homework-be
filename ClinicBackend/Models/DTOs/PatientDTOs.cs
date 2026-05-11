using ClinicBackend.Models.Enums;

namespace ClinicBackend.Models.DTOs;

public record PatientDto(
    Guid Id,
    string Name,
    string Address,
    string TajNumber,
    string Complaint,
    string Specialty,
    PatientStatus Status,
    Guid? AssignedDoctorId,
    string? AssignedDoctorName,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<DiagnosisDto>? Diagnoses = null
);

public record CreatePatientRequest(
    string Name,
    string Address,
    string TajNumber,
    string Complaint,
    string Specialty
);

public record UpdatePatientRequest(
    string? Name,
    string? Address,
    string? TajNumber,
    string? Complaint,
    string? Specialty
);

public record DiagnosisDto(
    Guid Id,
    Guid PatientId,
    Guid DoctorId,
    string DoctorName,
    string Description,
    DateTime CreatedAt
);

public record CreateDiagnosisRequest(string Description);

public record WaitlistEntryDto(
    Guid PatientId,
    string Name,
    PatientStatus Status,
    string Specialty,
    Guid? AssignedDoctorId,
    string? AssignedDoctorName,
    DateTime CreatedAt,
    string TajNumber
);

public record AuditLogDto(
    Guid Id,
    Guid UserId,
    string Username,
    Guid? PatientId,
    string? PatientName,
    string Action,
    string? Details,
    DateTime Timestamp
);

public record ErrorResponse(string Error, string Message, string? Field = null);

public record StateTransitionResult(Guid PatientId, string NewStatus);