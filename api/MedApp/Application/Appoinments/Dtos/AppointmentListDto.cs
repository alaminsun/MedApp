using MedApp.Domain.Enum;

namespace MedApp.Application.Appoinments.Dtos;
    public record AppointmentListDto(
        int Id,
        string Patient,
        string Doctor,
        DateOnly Date,
        VisitType VisitType,
        //string? Notes,
        string? Diagnosis
    );
        


