using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ClinicBackend.Data;
using ClinicBackend.Models.Entities;
using ClinicBackend.Models.Enums;
using BCrypt.Net;

namespace ClinicBackend.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(s =>
                s.ServiceType == typeof(DbContextOptions<ClinicDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<ClinicDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
            db.Database.EnsureCreated();
            SeedData(db);
        });
    }

    private void SeedData(ClinicDbContext db)
    {
        var users = new List<User>
        {
            new() { Username = "assistent1", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Asst123!"), Role = UserRole.Assistant },
            new() { Username = "assistent2", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Asst123!"), Role = UserRole.Assistant },
            new() { Username = "dr_kovacs", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doc123!"), Role = UserRole.Doctor, Specialty = "Belgyógyász" },
            new() { Username = "dr_nagy", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doc123!"), Role = UserRole.Doctor, Specialty = "Bőrgyógyász" },
            new() { Username = "dr_szabo", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doc123!"), Role = UserRole.Doctor, Specialty = "Szemész" },
        };
        db.Users.AddRange(users);
        db.SaveChanges();

        var drKovacs = users.First(u => u.Username == "dr_kovacs");
        var drNagy = users.First(u => u.Username == "dr_nagy");
        var drSzabo = users.First(u => u.Username == "dr_szabo");

        db.Patients.AddRange(
            new Patient { Name = "Nagy János", Address = "Debrecen, Szécheny u. 12.", TajNumber = "111-222-333", Complaint = "Mellkasi fájdalom", Specialty = "Belgyógyász", Status = PatientStatus.Waiting, AssignedDoctorId = drKovacs.Id },
            new Patient { Name = "Szabó Eszter", Address = "Szeged, Algya u. 4.", TajNumber = "444-555-666", Complaint = "Bőrkiütés a karján", Specialty = "Bőrgyógyász", Status = PatientStatus.Waiting, AssignedDoctorId = drNagy.Id },
            new Patient { Name = "Lakatos Mária", Address = "Budapest, Virág u. 3.", TajNumber = "112-233-445", Complaint = "Fáradtság, fejfájás", Specialty = "Belgyógyász", Status = PatientStatus.InProgress, AssignedDoctorId = drKovacs.Id }
        );
        db.SaveChanges();
    }
}