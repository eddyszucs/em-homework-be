using Microsoft.EntityFrameworkCore;
using ClinicBackend.Data;
using ClinicBackend.Models.DTOs;
using ClinicBackend.Services.Interfaces;

namespace ClinicBackend.Services;

public class AuditService : IAuditService
{
    private readonly ClinicDbContext _db;

    public AuditService(ClinicDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(Guid userId, Guid? patientId, string action, string? details)
    {
        var log = new Models.Entities.AuditLog
        {
            UserId = userId,
            PatientId = patientId,
            Action = action,
            Details = details
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }

    public async Task<object> QueryAsync(Guid? patientId, Guid? userId, DateTime? from, DateTime? to)
    {
        var query = _db.AuditLogs.AsQueryable();

        if (patientId.HasValue)
            query = query.Where(a => a.PatientId == patientId.Value);
        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);
        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Take(1000)
            .Select(a => new AuditLogDto(
                a.Id, a.UserId, a.User.Username,
                a.PatientId, a.Patient != null ? a.Patient.Name : null,
                a.Action, a.Details, a.Timestamp))
            .ToListAsync();

        return logs;
    }
}