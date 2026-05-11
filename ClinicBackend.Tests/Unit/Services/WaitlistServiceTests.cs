using Microsoft.EntityFrameworkCore;
using Moq;
using ClinicBackend.Data;
using ClinicBackend.Models.Entities;
using ClinicBackend.Models.Enums;
using ClinicBackend.Services;
using ClinicBackend.Services.Interfaces;
using Xunit;

namespace ClinicBackend.Tests.Unit.Services;

public class WaitlistServiceTests
{
    private readonly ClinicDbContext _db;
    private readonly Mock<IAuditService> _auditMock;
    private readonly Mock<INotificationService> _notifMock;
    private readonly WaitlistService _service;
    private readonly Guid _doctorId = Guid.NewGuid();

    public WaitlistServiceTests()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new ClinicDbContext(options);
        _auditMock = new Mock<IAuditService>();
        _notifMock = new Mock<INotificationService>();
        _service = new WaitlistService(_db, _auditMock.Object, _notifMock.Object);

        var doctor = new User { Id = _doctorId, Username = "dr_test", Role = UserRole.Doctor, Specialty = "Belgyógyász" };
        _db.Users.Add(doctor);
        _db.SaveChanges();
    }

    [Fact]
    public async Task CallPatient_WhenStatusIsWaiting_ShouldTransitionToInProgress()
    {
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Status = PatientStatus.Waiting,
            AssignedDoctorId = _doctorId,
            Name = "Test Patient",
            Specialty = "Belgyógyász",
            TajNumber = "111-222-333",
            Address = "Test Address",
            Complaint = "Test"
        };
        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();

        var result = await _service.CallPatientAsync(patient.Id, _doctorId);

        var updated = await _db.Patients.FindAsync(patient.Id);
        Assert.Equal(PatientStatus.InProgress, updated!.Status);
    }

    [Fact]
    public async Task CallPatient_WhenNotAssignedDoctor_ShouldThrowForbidden()
    {
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Status = PatientStatus.Waiting,
            AssignedDoctorId = Guid.NewGuid(), // different doctor
            Name = "Test Patient",
            Specialty = "Belgyógyász",
            TajNumber = "111-222-333",
            Address = "Test Address",
            Complaint = "Test"
        };
        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<ForbiddenException>(
            () => _service.CallPatientAsync(patient.Id, _doctorId));
        Assert.Contains("assigned doctor", ex.Message.ToLower());
    }

    [Fact]
    public async Task CallPatient_WhenStatusIsRecorded_ShouldThrowInvalidTransition()
    {
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Status = PatientStatus.Recorded,
            AssignedDoctorId = _doctorId,
            Name = "Test Patient",
            Specialty = "Belgyógyász",
            TajNumber = "111-222-333",
            Address = "Test Address",
            Complaint = "Test"
        };
        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CallPatientAsync(patient.Id, _doctorId));
    }

    [Fact]
    public async Task ReleasePatient_WhenStatusIsInProgress_ShouldTransitionToDone()
    {
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Status = PatientStatus.InProgress,
            AssignedDoctorId = _doctorId,
            Name = "Test Patient",
            Specialty = "Belgyógyász",
            TajNumber = "111-222-333",
            Address = "Test Address",
            Complaint = "Test"
        };
        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();

        await _service.ReleasePatientAsync(patient.Id, _doctorId);

        var updated = await _db.Patients.FindAsync(patient.Id);
        Assert.Equal(PatientStatus.Done, updated!.Status);
    }

    [Fact]
    public async Task GetDoctorWaitlist_ShouldReturnOnlyAssignedPatients()
    {
        var patient1 = new Patient
        {
            Id = Guid.NewGuid(), Status = PatientStatus.Waiting, AssignedDoctorId = _doctorId,
            Name = "Patient 1", Specialty = "Belgyógyász", TajNumber = "111-222-333",
            Address = "Addr", Complaint = "Comp"
        };
        var patient2 = new Patient
        {
            Id = Guid.NewGuid(), Status = PatientStatus.Waiting, AssignedDoctorId = Guid.NewGuid(),
            Name = "Patient 2", Specialty = "Belgyógyász", TajNumber = "222-333-444",
            Address = "Addr", Complaint = "Comp"
        };
        _db.Patients.AddRange(patient1, patient2);
        await _db.SaveChangesAsync();

        var result = await _service.GetDoctorWaitlistAsync(_doctorId);
        var list = (System.Collections.Generic.List<ClinicBackend.Models.DTOs.WaitlistEntryDto>)result!;

        Assert.Single(list);
        Assert.Equal(patient1.Id, list[0].PatientId);
    }
}