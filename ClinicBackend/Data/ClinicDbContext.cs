using Microsoft.EntityFrameworkCore;
using ClinicBackend.Models.Entities;

namespace ClinicBackend.Data;

public class ClinicDbContext : DbContext
{
    public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Diagnosis> Diagnoses => Set<Diagnosis>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Role).IsRequired();
            entity.Property(e => e.Specialty).HasMaxLength(100);
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.ToTable("Patients");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Address).HasMaxLength(500).IsRequired();
            entity.Property(e => e.TajNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Complaint).IsRequired();
            entity.Property(e => e.Specialty).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.HasIndex(e => e.TajNumber);
            entity.HasOne(e => e.AssignedDoctor)
                  .WithMany(u => u.AssignedPatients)
                  .HasForeignKey(e => e.AssignedDoctorId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        modelBuilder.Entity<Diagnosis>(entity =>
        {
            entity.ToTable("Diagnoses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired();
            entity.HasOne(e => e.Patient)
                  .WithMany(p => p.Diagnoses)
                  .HasForeignKey(e => e.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Doctor)
                  .WithMany(u => u.Diagnoses)
                  .HasForeignKey(e => e.DoctorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
            entity.HasOne(e => e.User)
                  .WithMany(u => u.AuditLogs)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Patient)
                  .WithMany(p => p.AuditLogs)
                  .HasForeignKey(e => e.PatientId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.Timestamp);
        });
    }
}