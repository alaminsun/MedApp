
using MedApp.Domain.Enum;

namespace MedApp.Domain.Entities
{
    public class Appointment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateOnly AppointmentDate { get; set; }
        public VisitType VisitType { get; set; }
        public string? Notes { get; set; }
        public string? Diagnosis { get; set; }
        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
        public List<PrescriptionDetail>? PrescriptionDetails { get; set; } = new();
    }
}
