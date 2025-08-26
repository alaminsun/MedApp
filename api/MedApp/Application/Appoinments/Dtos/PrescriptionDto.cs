namespace MedApp.Application.Appoinments.Dtos;
    public record PrescriptionDto(
        int? Id,
        int MedicineId,
        string Dosage,
        DateOnly StartDate,
        DateOnly EndDate,
        string? Notes
    );

