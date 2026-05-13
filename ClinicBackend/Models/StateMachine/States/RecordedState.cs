using ClinicBackend.Models.Entities;
using ClinicBackend.Models.Enums;

namespace ClinicBackend.Models.StateMachine.States;

public class RecordedState : PatientStateBase
{
    public override PatientStatus Status => PatientStatus.Recorded;

    public override IPatientState AssignDoctor(Patient patient, Guid doctorId)
    {
        patient.AssignedDoctorId = doctorId;
        return new WaitingState();
    }

    public override IPatientState Call(Patient patient, Guid doctorId)
    {
        throw new InvalidOperationException(
            "Invalid state transition from 'Recorded' to 'InProgress'.");
    }

    public override IPatientState Release(Patient patient, Guid doctorId)
    {
        throw new InvalidOperationException(
            "Invalid state transition from 'Recorded' to 'Done'.");
    }
}