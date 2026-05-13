using ClinicBackend.Models.Entities;
using ClinicBackend.Models.Enums;
using ClinicBackend.Models.StateMachine.States;

namespace ClinicBackend.Models.StateMachine;

public static class PatientStateMachine
{
    private static readonly Dictionary<PatientStatus, HashSet<PatientStatus>> Transitions = new()
    {
        { PatientStatus.Recorded, new HashSet<PatientStatus> { PatientStatus.Waiting } },
        { PatientStatus.Waiting, new HashSet<PatientStatus> { PatientStatus.InProgress } },
        { PatientStatus.InProgress, new HashSet<PatientStatus> { PatientStatus.Done } },
        { PatientStatus.Done, new HashSet<PatientStatus>() }
    };

    private static readonly Dictionary<PatientStatus, Func<IPatientState>> _factories = new()
    {
        { PatientStatus.Recorded,   () => new RecordedState() },
        { PatientStatus.Waiting,    () => new WaitingState() },
        { PatientStatus.InProgress, () => new InProgressState() },
        { PatientStatus.Done,       () => new DoneState() },
    };

    public static bool CanTransition(PatientStatus from, PatientStatus to)
    {
        return Transitions.TryGetValue(from, out var targets) && targets.Contains(to);
    }

    public static IEnumerable<PatientStatus> GetValidTargets(PatientStatus from)
    {
        return Transitions.TryGetValue(from, out var targets) ? targets : Enumerable.Empty<PatientStatus>();
    }

    public static IPatientState CreateState(PatientStatus status)
        => _factories[status]();

    public static void AssignDoctor(Patient patient, Guid doctorId)
    {
        if (patient.Status != PatientStatus.Recorded) return;
        Transition(patient, patient.State.AssignDoctor(patient, doctorId));
    }

    public static void Call(Patient patient, Guid doctorId)
        => Transition(patient, patient.State.Call(patient, doctorId));

    public static void Release(Patient patient, Guid doctorId)
        => Transition(patient, patient.State.Release(patient, doctorId));

    public static void Transition(Patient patient, PatientStatus to)
    {
        if (patient.Status == to) return;
        if (!CanTransition(patient.Status, to))
            throw new InvalidOperationException(
                $"Invalid state transition from '{patient.Status}' to '{to}'.");

        if (to == PatientStatus.Waiting && patient.AssignedDoctorId == null)
            throw new InvalidOperationException(
                "Cannot transition to Waiting: no doctor is assigned.");

        var newState = CreateState(to);
        Transition(patient, newState);
    }

    private static void Transition(Patient patient, IPatientState newState)
    {
        patient.State.OnExit(patient);
        patient.Status = newState.Status;
        patient.State = newState;
        patient.State.OnEnter(patient);
    }
}