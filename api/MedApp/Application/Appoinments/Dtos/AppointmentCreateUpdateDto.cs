using MedApp.Domain.Enum;

namespace MedApp.Application.Appoinments.Dtos;
public record AppointmentCreateUpdateDto(
        int PatientId,
        int DoctorId,
        DateOnly AppointmentDate, 
        string? Notes, 
        string? Diagnosis, 
        VisitType VisitType, 
        List<PrescriptionDto> Prescriptions
 );

