using ClinicBackend.Models.Enums;

namespace ClinicBackend.Models.Entities;

public class Patient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string TajNumber { get; set; } = string.Empty;
    public string Complaint { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public PatientStatus Status { get; set; } = PatientStatus.Recorded;
    public Guid? AssignedDoctorId { get; set; }
    public User? AssignedDoctor { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    public ICollection<Diagnosis> Diagnoses { get; set; } = new List<Diagnosis>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}