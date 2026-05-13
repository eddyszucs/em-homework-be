using ClinicBackend.Models.Entities;
using ClinicBackend.Models.Enums;

namespace ClinicBackend.Models.StateMachine;

public interface IPatientState
{
    PatientStatus Status { get; }

    void OnEnter(Patient patient);
    void OnExit(Patient patient);

    IPatientState AssignDoctor(Patient patient, Guid doctorId);
    IPatientState Call(Patient patient, Guid doctorId);
    IPatientState Release(Patient patient, Guid doctorId);
}