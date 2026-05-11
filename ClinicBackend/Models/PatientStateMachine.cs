using ClinicBackend.Models.Enums;
using ClinicBackend.Models.Entities;

namespace ClinicBackend.Models;

public static class PatientStateMachine
{
    private static readonly Dictionary<PatientStatus, HashSet<PatientStatus>> Transitions = new()
    {
        { PatientStatus.Recorded, new HashSet<PatientStatus> { PatientStatus.Waiting } },
        { PatientStatus.Waiting, new HashSet<PatientStatus> { PatientStatus.InProgress } },
        { PatientStatus.InProgress, new HashSet<PatientStatus> { PatientStatus.Done } },
        { PatientStatus.Done, new HashSet<PatientStatus>() }
    };

    public static bool CanTransition(PatientStatus from, PatientStatus to)
    {
        return Transitions.TryGetValue(from, out var targets) && targets.Contains(to);
    }

    public static IEnumerable<PatientStatus> GetValidTargets(PatientStatus from)
    {
        return Transitions.TryGetValue(from, out var targets) ? targets : Enumerable.Empty<PatientStatus>();
    }

    public static void Transition(Patient patient, PatientStatus to)
    {
        if (patient.Status == to) return; // no-op
        if (!CanTransition(patient.Status, to))
            throw new InvalidOperationException(
                $"Invalid state transition from '{patient.Status}' to '{to}'.");

        if (to == PatientStatus.Waiting && patient.AssignedDoctorId == null)
            throw new InvalidOperationException(
                "Cannot transition to Waiting: no doctor is assigned.");

        patient.Status = to;
        patient.UpdatedAt = DateTime.UtcNow;
    }
}