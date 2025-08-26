using MedApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedApp.Infrastructure.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> opt) : DbContext(opt)
    {
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<Medicine> Medicines => Set<Medicine>(); 
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<PrescriptionDetail> PrescriptionDetails => Set<PrescriptionDetail>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Patient>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).ValueGeneratedOnAdd();
                e.Property(x => x.Name).IsRequired().HasMaxLength(200);
                e.Property(x => x.Age).IsRequired();
                e.HasIndex(x => x.Name);
                e.ToTable(tb => tb.HasCheckConstraint("CK_Patient_Age", "[Age] >= 0"));
            });
            modelBuilder.Entity<Doctor>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).ValueGeneratedOnAdd();
                e.Property(x => x.Name).IsRequired().HasMaxLength(200);
                e.Property(x => x.Specialty).IsRequired().HasMaxLength(100);
                e.HasIndex(x => x.Name);
                e.HasIndex(x => x.Specialty);

            });
            modelBuilder.Entity<Medicine>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).ValueGeneratedOnAdd();
                e.Property(x => x.Name).IsRequired().HasMaxLength(200);
                e.Property(x => x.Dosage).HasMaxLength(100);
                e.Property(x => x.Manufacturer).HasMaxLength(200);
                e.HasIndex(x => x.Name);
            });

            modelBuilder.Entity<Appointment>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).ValueGeneratedOnAdd();
                e.HasOne(x => x.Patient)
                 .WithMany()
                 .HasForeignKey(x => x.PatientId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Doctor)
                 .WithMany()
                 .HasForeignKey(x => x.DoctorId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.Property(x => x.AppointmentDate).IsRequired().HasColumnType("date");
                e.Property(x => x.VisitType).IsRequired();
                e.Property(x => x.Notes).HasMaxLength(1000);
                e.Property(x => x.Diagnosis).HasMaxLength(1000);


                e.HasIndex(x => new { x.PatientId, x.AppointmentDate });
                e.HasIndex(x => new { x.DoctorId, x.AppointmentDate });
            });
                
            modelBuilder.Entity<PrescriptionDetail>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).ValueGeneratedOnAdd();
                e.HasOne<Appointment>()
                 .WithMany(x => x.PrescriptionDetails)
                 .HasForeignKey(x => x.AppointmentId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Medicine)
                 .WithMany()
                 .HasForeignKey(x => x.MedicineId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.Property(x => x.Dosage).IsRequired().HasMaxLength(100);
                e.Property(x => x.Notes).HasMaxLength(1000);
                e.Property(x => x.StartDate).HasColumnType("date");
                e.Property(x => x.EndDate).HasColumnType("date");
                e.ToTable(tb => tb.HasCheckConstraint("CK_PrescriptionDetail_DateRange", "[EndDate] >= [StartDate]"));         
            });


        }

    }
}
