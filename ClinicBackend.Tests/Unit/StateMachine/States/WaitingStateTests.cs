using ClinicBackend.Models.Entities;
using ClinicBackend.Models.Enums;
using ClinicBackend.Models.StateMachine.States;
using ClinicBackend.Services;
using Xunit;

namespace ClinicBackend.Tests.Unit.StateMachine.States;

public class WaitingStateTests
{
    private readonly Patient _patient;
    private readonly Guid _doctorId;

    public WaitingStateTests()
    {
        _doctorId = Guid.NewGuid();
        _patient = new Patient
        {
            Name = "Test Patient",
            AssignedDoctorId = _doctorId
        };
        _patient.State = new WaitingState();
    }

    [Fact]
    public void Status_ReturnsWaiting()
    {
        var state = new WaitingState();
        Assert.Equal(PatientStatus.Waiting, state.Status);
    }

    [Fact]
    public void AssignDoctor_ThrowsInvalidOperationException()
    {
        var state = new WaitingState();
        var ex = Assert.Throws<InvalidOperationException>(
            () => state.AssignDoctor(_patient, Guid.NewGuid()));
        Assert.Contains("Invalid state transition from 'Waiting' to 'Waiting'", ex.Message);
    }

    [Fact]
    public void Call_WithAssignedDoctor_TransitionsToInProgress()
    {
        var state = new WaitingState();

        var nextState = state.Call(_patient, _doctorId);

        Assert.IsType<InProgressState>(nextState);
    }

    [Fact]
    public void Call_WithUnassignedDoctor_ThrowsForbiddenException()
    {
        var state = new WaitingState();
        var wrongDoctorId = Guid.NewGuid();

        var ex = Assert.Throws<ForbiddenException>(
            () => state.Call(_patient, wrongDoctorId));
        Assert.Contains("Only the assigned doctor can perform this action", ex.Message);
    }

    [Fact]
    public void Release_ThrowsInvalidOperationException()
    {
        var state = new WaitingState();
        var ex = Assert.Throws<InvalidOperationException>(
            () => state.Release(_patient, _doctorId));
        Assert.Contains("Invalid state transition from 'Waiting' to 'Done'", ex.Message);
    }
}