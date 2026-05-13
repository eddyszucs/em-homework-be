using ClinicBackend.Models.Entities;
using ClinicBackend.Models.Enums;
using ClinicBackend.Models.StateMachine.States;
using ClinicBackend.Services;
using Xunit;

namespace ClinicBackend.Tests.Unit.StateMachine.States;

public class InProgressStateTests
{
    private readonly Patient _patient;
    private readonly Guid _doctorId;

    public InProgressStateTests()
    {
        _doctorId = Guid.NewGuid();
        _patient = new Patient
        {
            Name = "Test Patient",
            AssignedDoctorId = _doctorId
        };
        _patient.State = new InProgressState();
    }

    [Fact]
    public void Status_ReturnsInProgress()
    {
        var state = new InProgressState();
        Assert.Equal(PatientStatus.InProgress, state.Status);
    }

    [Fact]
    public void AssignDoctor_ThrowsInvalidOperationException()
    {
        var state = new InProgressState();
        var ex = Assert.Throws<InvalidOperationException>(
            () => state.AssignDoctor(_patient, Guid.NewGuid()));
        Assert.Contains("Invalid state transition from 'InProgress' to 'Waiting'", ex.Message);
    }

    [Fact]
    public void Call_ThrowsInvalidOperationException()
    {
        var state = new InProgressState();
        var ex = Assert.Throws<InvalidOperationException>(
            () => state.Call(_patient, _doctorId));
        Assert.Contains("Invalid state transition from 'InProgress' to 'InProgress'", ex.Message);
    }

    [Fact]
    public void Release_WithAssignedDoctor_TransitionsToDone()
    {
        var state = new InProgressState();

        var nextState = state.Release(_patient, _doctorId);

        Assert.IsType<DoneState>(nextState);
    }

    [Fact]
    public void Release_WithUnassignedDoctor_ThrowsForbiddenException()
    {
        var state = new InProgressState();
        var wrongDoctorId = Guid.NewGuid();

        var ex = Assert.Throws<ForbiddenException>(
            () => state.Release(_patient, wrongDoctorId));
        Assert.Contains("Only the assigned doctor can perform this action", ex.Message);
    }
}