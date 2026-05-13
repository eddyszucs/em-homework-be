using ClinicBackend.Models.Entities;
using ClinicBackend.Models.Enums;

namespace ClinicBackend.Models.StateMachine.States;

public class DoneState : PatientStateBase
{
    public override PatientStatus Status => PatientStatus.Done;

    public override IPatientState AssignDoctor(Patient patient, Guid doctorId)
    {
        throw new InvalidOperationException(
            "Invalid state transition from 'Done' to 'Waiting'.");
    }

    public override IPatientState Call(Patient patient, Guid doctorId)
    {
        throw new InvalidOperationException(
            "Invalid state transition from 'Done' to 'InProgress'.");
    }

    public override IPatientState Release(Patient patient, Guid doctorId)
    {
        throw new InvalidOperationException(
            "Invalid state transition from 'Done' to 'Done'.");
    }
}