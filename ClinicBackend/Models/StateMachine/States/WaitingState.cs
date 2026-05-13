using ClinicBackend.Models.Entities;
using ClinicBackend.Models.Enums;

namespace ClinicBackend.Models.StateMachine.States;

public class WaitingState : PatientStateBase
{
    public override PatientStatus Status => PatientStatus.Waiting;

    public override IPatientState AssignDoctor(Patient patient, Guid doctorId)
    {
        throw new InvalidOperationException(
            "Invalid state transition from 'Waiting' to 'Waiting'.");
    }

    public override IPatientState Call(Patient patient, Guid doctorId)
    {
        GuardDoctorOwnership(patient, doctorId);
        return new InProgressState();
    }

    public override IPatientState Release(Patient patient, Guid doctorId)
    {
        throw new InvalidOperationException(
            "Invalid state transition from 'Waiting' to 'Done'.");
    }
}