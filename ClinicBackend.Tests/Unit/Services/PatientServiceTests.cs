using Microsoft.EntityFrameworkCore;
using Moq;
using ClinicBackend.Data;
using ClinicBackend.Models.Entities;
using ClinicBackend.Models.Enums;
using ClinicBackend.Services;
using ClinicBackend.Services.Interfaces;
using Xunit;

namespace ClinicBackend.Tests.Unit.Services;

public class PatientServiceTests
{
    private readonly ClinicDbContext _db;
    private readonly Mock<IAuditService> _auditMock;
    private readonly Mock<INotificationService> _notifMock;
    private readonly PatientService _service;

    public PatientServiceTests()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new ClinicDbContext(options);
        _auditMock = new Mock<IAuditService>();
        _notifMock = new Mock<INotificationService>();
        _service = new PatientService(_db, _auditMock.Object, _notifMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesPatient()
    {
        var userId = Guid.NewGuid();
        var dto = await _service.CreateAsync(new ClinicBackend.Models.DTOs.CreatePatientRequest(
            "Kovács Anna",
            "Budapest, Fő utca 5.",
            "123-456-789",
            "Fejfájás",
            "Belgyógyász"
        ), userId);

        Assert.Equal("Kovács Anna", dto.Name);
        Assert.Equal(PatientStatus.Recorded, dto.Status);
        _auditMock.Verify(a => a.LogAsync(userId, It.IsAny<Guid>(), "PATIENT_CREATED", It.IsAny<string>()), Times.Once);
        _notifMock.Verify(n => n.NotifyPatientAddedAsync(It.IsAny<Guid>(), "Kovács Anna", "Belgyógyász", "Recorded"), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidTaj_ThrowsValidationException()
    {
        var ex = await Assert.ThrowsAsync<ClinicBackend.Services.ValidationException>(
            () => _service.CreateAsync(new ClinicBackend.Models.DTOs.CreatePatientRequest(
            "Kovács Anna",
            "Budapest",
            "invalid",
            "Fejfájás",
            "Belgyógyász"
        ), Guid.NewGuid()));
        Assert.Equal("TajNumber", ex.Field);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllNonDeletedPatients()
    {
        _db.Patients.AddRange(
            new Patient { Name = "Patient 1", Status = PatientStatus.Waiting, Specialty = "Belgyógyász", TajNumber = "111-222-333", Address = "Addr", Complaint = "Comp" },
            new Patient { Name = "Patient 2", Status = PatientStatus.Recorded, Specialty = "Szemész", TajNumber = "222-333-444", Address = "Addr", Complaint = "Comp", IsDeleted = true }
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetAllAsync(Guid.NewGuid(), "Assistant");
        var list = (System.Collections.Generic.List<ClinicBackend.Models.DTOs.PatientDto>)result!;

        Assert.Single(list);
        Assert.Equal("Patient 1", list[0].Name);
    }

    [Fact]
    public async Task DeleteAsync_SetsIsDeletedFlag()
    {
        var patient = new Patient
        {
            Name = "ToDelete",
            Status = PatientStatus.Recorded,
            Specialty = "Belgyógyász",
            TajNumber = "111-222-333",
            Address = "Addr",
            Complaint = "Comp"
        };
        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();

        await _service.DeleteAsync(patient.Id, Guid.NewGuid());

        var found = await _db.Patients.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == patient.Id);
        Assert.NotNull(found);
        Assert.True(found.IsDeleted);
    }
}