using ClinicBackend.Hubs;
using ClinicBackend.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ClinicBackend.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<ClinicNotificationHub> _hub;

    public NotificationService(IHubContext<ClinicNotificationHub> hub)
    {
        _hub = hub;
    }

    public async Task NotifyPatientAddedAsync(Guid patientId, string name, string specialty, string status)
    {
        Console.WriteLine($"[NotificationService] NotifyPatientAddedAsync to AssistantGroup: patientId={patientId}, name={name}");
        await _hub.Clients.Group("AssistantGroup").SendAsync("OnPatientAdded", new
        {
            patientId = patientId,
            name = name,
            specialty = specialty,
            status = status
        });
    }

    public async Task NotifyPatientStatusChangedAsync(Guid patientId, Guid doctorId, string newStatus)
    {
        Console.WriteLine($"[NotificationService] NotifyPatientStatusChangedAsync to DoctorGroup_{doctorId} and AssistantGroup: patientId={patientId}, newStatus={newStatus}");
        await _hub.Clients.Group($"DoctorGroup_{doctorId}").SendAsync("OnPatientStatusChanged", new
        {
            patientId = patientId,
            doctorId = doctorId,
            newStatus = newStatus,
            updatedAt = DateTime.UtcNow
        });
        await _hub.Clients.Group("AssistantGroup").SendAsync("OnPatientStatusChanged", new
        {
            patientId = patientId,
            doctorId = doctorId,
            newStatus = newStatus,
            updatedAt = DateTime.UtcNow
        });
    }

    public async Task NotifyDiagnosisAddedAsync(Guid patientId, Guid doctorId, string diagnosis)
    {
        await _hub.Clients.Group("DoctorGroup").SendAsync("OnDiagnosisAdded", new
        {
            PatientId = patientId,
            DoctorId = doctorId,
            Diagnosis = diagnosis
        });
    }
}