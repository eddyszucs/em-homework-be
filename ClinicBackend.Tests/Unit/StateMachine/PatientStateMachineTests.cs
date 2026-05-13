using ClinicBackend.Models.StateMachine;
using ClinicBackend.Models.Enums;
using ClinicBackend.Models.Entities;
using Xunit;

namespace ClinicBackend.Tests.Unit.StateMachine;

public class PatientStateMachineTests
{
    [Theory]
    [InlineData(PatientStatus.Recorded, PatientStatus.Waiting, true)]
    [InlineData(PatientStatus.Waiting, PatientStatus.InProgress, true)]
    [InlineData(PatientStatus.InProgress, PatientStatus.Done, true)]
    public void ValidTransitions_ShouldReturnTrue(PatientStatus from, PatientStatus to, bool expected)
    {
        var result = PatientStateMachine.CanTransition(from, to);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(PatientStatus.Recorded, PatientStatus.InProgress)] // skip Waiting
    [InlineData(PatientStatus.Recorded, PatientStatus.Done)]       // skip both
    [InlineData(PatientStatus.Waiting, PatientStatus.Done)]       // skip InProgress
    [InlineData(PatientStatus.Waiting, PatientStatus.Recorded)]    // backward
    [InlineData(PatientStatus.InProgress, PatientStatus.Waiting)]  // backward
    [InlineData(PatientStatus.Done, PatientStatus.InProgress)]     // backward
    public void InvalidTransitions_ShouldReturnFalse(PatientStatus from, PatientStatus to)
    {
        var result = PatientStateMachine.CanTransition(from, to);
        Assert.False(result);
    }

    [Fact]
    public void GetValidTargets_FromRecorded_ShouldReturnWaiting()
    {
        var targets = PatientStateMachine.GetValidTargets(PatientStatus.Recorded);
        Assert.Single(targets);
        Assert.Contains(PatientStatus.Waiting, targets);
    }

    [Fact]
    public void GetValidTargets_FromDone_ShouldReturnEmpty()
    {
        var targets = PatientStateMachine.GetValidTargets(PatientStatus.Done);
        Assert.Empty(targets);
    }

    [Fact]
    public void Transition_FromRecordedToWaiting_WhenNoDoctorAssigned_ShouldThrow()
    {
        var patient = new Patient { Status = PatientStatus.Recorded, AssignedDoctorId = null };
        var ex = Assert.Throws<InvalidOperationException>(
            () => PatientStateMachine.Transition(patient, PatientStatus.Waiting));
        Assert.Contains("doctor", ex.Message.ToLower());
    }

    [Fact]
    public void Transition_ValidTransition_ShouldUpdateStatusAndTimestamp()
    {
        var patient = new Patient
        {
            Status = PatientStatus.Recorded,
            AssignedDoctorId = Guid.NewGuid(),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        var before = DateTime.UtcNow;

        PatientStateMachine.Transition(patient, PatientStatus.Waiting);

        Assert.Equal(PatientStatus.Waiting, patient.Status);
        Assert.True(patient.UpdatedAt >= before);
    }

    [Fact]
    public void Transition_InvalidTransition_ShouldThrow()
    {
        var patient = new Patient { Status = PatientStatus.Recorded, AssignedDoctorId = Guid.NewGuid() };
        Assert.Throws<InvalidOperationException>(
            () => PatientStateMachine.Transition(patient, PatientStatus.Done));
    }
}