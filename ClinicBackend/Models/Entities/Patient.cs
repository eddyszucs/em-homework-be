using ClinicBackend.Models.Enums;
using ClinicBackend.Models.StateMachine;
using Microsoft.EntityFrameworkCore;

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

    private IPatientState _state = null!;

    public IPatientState State
    {
        get
        {
            if (_state == null || _state.Status != Status)
                _state = PatientStateMachine.CreateState(Status);
            return _state;
        }
        set
        {
            if (_state != value)
            {
                _state?.OnExit(this);
                _state = value;
                Status = value.Status;
                _state.OnEnter(this);
            }
        }
    }

    public ICollection<Diagnosis> Diagnoses { get; set; } = new List<Diagnosis>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}