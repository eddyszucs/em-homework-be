using ClinicBackend.Models.Entities;
using ClinicBackend.Models.Enums;
using ClinicBackend.Models.StateMachine.States;
using ClinicBackend.Services;
using Xunit;

namespace ClinicBackend.Tests.Unit.StateMachine.States;

public class RecordedStateTests
{
    private readonly Patient _patient;

    public RecordedStateTests()
    {
        _patient = new Patient
        {
            Name = "Test Patient",
            AssignedDoctorId = null
        };
    }

    [Fact]
    public void Status_ReturnsRecorded()
    {
        var state = new RecordedState();
        Assert.Equal(PatientStatus.Recorded, state.Status);
    }

    [Fact]
    public void AssignDoctor_WithNoDoctor_TransitionsToWaiting()
    {
        var state = new RecordedState();
        var doctorId = Guid.NewGuid();

        var nextState = state.AssignDoctor(_patient, doctorId);

        Assert.IsType<WaitingState>(nextState);
        Assert.Equal(doctorId, _patient.AssignedDoctorId);
    }

    [Fact]
    public void Call_ThrowsInvalidOperationException()
    {
        var state = new RecordedState();
        var ex = Assert.Throws<InvalidOperationException>(
            () => state.Call(_patient, Guid.NewGuid()));
        Assert.Contains("Invalid state transition from 'Recorded' to 'InProgress'", ex.Message);
    }

    [Fact]
    public void Release_ThrowsInvalidOperationException()
    {
        var state = new RecordedState();
        var ex = Assert.Throws<InvalidOperationException>(
            () => state.Release(_patient, Guid.NewGuid()));
        Assert.Contains("Invalid state transition from 'Recorded' to 'Done'", ex.Message);
    }
}