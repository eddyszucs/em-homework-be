using Microsoft.EntityFrameworkCore;
using ClinicBackend.Data;
using ClinicBackend.Models.Entities;
using ClinicBackend.Models.Enums;
using BCrypt.Net;

namespace ClinicBackend.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ClinicDbContext db)
    {
        if (await db.Users.AnyAsync()) return; // Already seeded

        var users = new List<User>
        {
            new() { Username = "assistent1", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Asst123!"), Role = UserRole.Assistant },
            new() { Username = "assistent2", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Asst123!"), Role = UserRole.Assistant },
            new() { Username = "dr_kovacs", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doc123!"), Role = UserRole.Doctor, Specialty = "Belgyógyász" },
            new() { Username = "dr_nagy", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doc123!"), Role = UserRole.Doctor, Specialty = "Bőrgyógyász" },
            new() { Username = "dr_szabo", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doc123!"), Role = UserRole.Doctor, Specialty = "Szemész" },
            new() { Username = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"), Role = UserRole.Assistant },
        };

        db.Users.AddRange(users);
        await db.SaveChangesAsync();

        var drKovacs = users.First(u => u.Username == "dr_kovacs");
        var drNagy = users.First(u => u.Username == "dr_nagy");
        var drSzabo = users.First(u => u.Username == "dr_szabo");

        var patients = new List<Patient>
        {
            new() { Name = "Nagy János", Address = "Debrecen, Szécheny u. 12.", TajNumber = "111-222-333", Complaint = "Mellkasi fájdalom", Specialty = "Belgyógyász", Status = PatientStatus.Waiting, AssignedDoctorId = drKovacs.Id },
            new() { Name = "Szabó Eszter", Address = "Szeged, Algya u. 4.", TajNumber = "444-555-666", Complaint = "Bőrkiütés a karján", Specialty = "Bőrgyógyász", Status = PatientStatus.Waiting, AssignedDoctorId = drNagy.Id },
            new() { Name = "Tóth Péter", Address = "Pécs, Komény u. 8.", TajNumber = "777-888-999", Complaint = "Homályos látás", Specialty = "Szemész", Status = PatientStatus.Waiting, AssignedDoctorId = drSzabo.Id },
            new() { Name = "Lakatos Mária", Address = "Budapest, Virág u. 3.", TajNumber = "112-233-445", Complaint = "Fáradtság, fejfájás", Specialty = "Belgyógyász", Status = PatientStatus.InProgress, AssignedDoctorId = drKovacs.Id },
            new() { Name = "Farkas Gábor", Address = "Miskolc, Akác u. 22.", TajNumber = "556-677-889", Complaint = "Szemviszketés", Specialty = "Szemész", Status = PatientStatus.Waiting, AssignedDoctorId = drSzabo.Id },
        };

        db.Patients.AddRange(patients);
        await db.SaveChangesAsync();
    }
}