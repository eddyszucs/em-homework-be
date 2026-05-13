using ClinicBackend.Models.Entities;
using ClinicBackend.Models.Enums;
using ClinicBackend.Models.StateMachine.States;
using Xunit;

namespace ClinicBackend.Tests.Unit.StateMachine.States;

public class DoneStateTests
{
    private readonly Patient _patient;
    private readonly Guid _doctorId;

    public DoneStateTests()
    {
        _doctorId = Guid.NewGuid();
        _patient = new Patient
        {
            Name = "Test Patient",
            AssignedDoctorId = _doctorId
        };
        _patient.State = new DoneState();
    }

    [Fact]
    public void Status_ReturnsDone()
    {
        var state = new DoneState();
        Assert.Equal(PatientStatus.Done, state.Status);
    }

    [Fact]
    public void AssignDoctor_ThrowsInvalidOperationException()
    {
        var state = new DoneState();
        var ex = Assert.Throws<InvalidOperationException>(
            () => state.AssignDoctor(_patient, Guid.NewGuid()));
        Assert.Contains("Invalid state transition from 'Done' to 'Waiting'", ex.Message);
    }

    [Fact]
    public void Call_ThrowsInvalidOperationException()
    {
        var state = new DoneState();
        var ex = Assert.Throws<InvalidOperationException>(
            () => state.Call(_patient, _doctorId));
        Assert.Contains("Invalid state transition from 'Done' to 'InProgress'", ex.Message);
    }

    [Fact]
    public void Release_ThrowsInvalidOperationException()
    {
        var state = new DoneState();
        var ex = Assert.Throws<InvalidOperationException>(
            () => state.Release(_patient, _doctorId));
        Assert.Contains("Invalid state transition from 'Done' to 'Done'", ex.Message);
    }
}