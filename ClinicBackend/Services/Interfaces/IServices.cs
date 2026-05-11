using ClinicBackend.Models.DTOs;

namespace ClinicBackend.Services.Interfaces;

public interface IAuthService
{
    Task<(string Token, string RefreshToken, DateTime ExpiresAt, object User)> LoginAsync(string username, string password);
    Task<(string Token, string RefreshToken, DateTime ExpiresAt)> RefreshAsync(string refreshToken);
    string GenerateJwtToken(Guid userId, string username, string role, string? specialty);
    string GenerateRefreshToken();
}

public interface IPatientService
{
    Task<object> GetAllAsync(Guid userId, string role);
    Task<object?> GetByIdAsync(Guid id, Guid userId, string role);
    Task<PatientDto> CreateAsync(CreatePatientRequest request, Guid userId);
    Task<object> UpdateAsync(Guid id, UpdatePatientRequest request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}

public interface IWaitlistService
{
    Task<object> GetDoctorWaitlistAsync(Guid doctorId);
    Task<object> AssignPatientToDoctorAsync(Guid patientId, Guid doctorId, Guid assistantId);
    Task<object> CallPatientAsync(Guid patientId, Guid doctorId);
    Task<object> ReleasePatientAsync(Guid patientId, Guid doctorId);
}

public interface IAuditService
{
    Task LogAsync(Guid userId, Guid? patientId, string action, string? details);
    Task<object> QueryAsync(Guid? patientId, Guid? userId, DateTime? from, DateTime? to);
}

public interface INotificationService
{
    Task NotifyPatientAddedAsync(Guid patientId, string name, string specialty, string status);
    Task NotifyPatientStatusChangedAsync(Guid patientId, Guid doctorId, string newStatus);
    Task NotifyDiagnosisAddedAsync(Guid patientId, Guid doctorId, string diagnosis);
}