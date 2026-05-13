using ClinicBackend.Models.Entities;
using ClinicBackend.Models.Enums;
using ClinicBackend.Services;

namespace ClinicBackend.Models.StateMachine;

public abstract class PatientStateBase : IPatientState
{
    public abstract PatientStatus Status { get; }

    public virtual void OnEnter(Patient patient)
        => patient.UpdatedAt = DateTime.UtcNow;

    public virtual void OnExit(Patient patient) { }

    protected void GuardDoctorIsAssigned(Patient patient)
    {
        if (patient.AssignedDoctorId == null)
            throw new InvalidOperationException(
                "Cannot transition to Waiting: no doctor is assigned.");
    }

    protected void GuardDoctorOwnership(Patient patient, Guid doctorId)
    {
        if (patient.AssignedDoctorId != doctorId)
            throw new ForbiddenException(
                "Only the assigned doctor can perform this action.");
    }

    public abstract IPatientState AssignDoctor(Patient patient, Guid doctorId);
    public abstract IPatientState Call(Patient patient, Guid doctorId);
    public abstract IPatientState Release(Patient patient, Guid doctorId);
}