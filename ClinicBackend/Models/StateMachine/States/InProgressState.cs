using ClinicBackend.Models.Entities;
using ClinicBackend.Models.Enums;

namespace ClinicBackend.Models.StateMachine.States;

public class InProgressState : PatientStateBase
{
    public override PatientStatus Status => PatientStatus.InProgress;

    public override IPatientState AssignDoctor(Patient patient, Guid doctorId)
    {
        throw new InvalidOperationException(
            "Invalid state transition from 'InProgress' to 'Waiting'.");
    }

    public override IPatientState Call(Patient patient, Guid doctorId)
    {
        throw new InvalidOperationException(
            "Invalid state transition from 'InProgress' to 'InProgress'.");
    }

    public override IPatientState Release(Patient patient, Guid doctorId)
    {
        GuardDoctorOwnership(patient, doctorId);
        return new DoneState();
    }
}