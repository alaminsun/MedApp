using MedApp.Domain.Entities;
using MedApp.Domain.Enum;
using MedApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedApp.Infrastructure.Persistence.Services
{
    public static class DemoSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if( await db.Patients.AnyAsync()) return;

            var p1 = new Patient { Name = "John Doe", Age = 30 };
            var p2 = new Patient { Name = "Jane Smith", Age = 25 };
            var p3 = new Patient { Name = "Alice Johnson", Age = 40 };
            var d1 = new Doctor {  Name = "Dr. Emily Brown", Specialty = "Cardiology" };
            var d2 = new Doctor {  Name = "Dr. Michael Green", Specialty = "Dermatology" };
            var m1 = new Medicine { Name = "Aspirin", Dosage = "100mg", Manufacturer = "Pharma Inc." };
            var m2 = new Medicine { Name = "Ibuprofen", Dosage = "200mg", Manufacturer = "HealthCorp" };

            await db.AddRangeAsync(p1, p2, p3, d1, d2, m1, m2);
            await db.SaveChangesAsync();

            var appoit = new Appointment
            {
                PatientId = p1.Id,
                DoctorId = d1.Id,
                AppointmentDate = DateOnly.FromDateTime(DateTime.Today),
                VisitType = VisitType.First,
                Notes = "Patient needs to drink water every hour",
                Diagnosis = "Viral fever",
                PrescriptionDetails = new List<PrescriptionDetail>
                {
                    new PrescriptionDetail
                    {

                        MedicineId = m1.Id,
                        Dosage = "500mg once daily",
                        StartDate = DateOnly.FromDateTime(DateTime.Today),
                        EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
                        Notes = "Take with food."
                    }
                }
            };
            db.Add(appoit);
            await db.SaveChangesAsync();
        }
    }
}
