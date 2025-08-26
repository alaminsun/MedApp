using MedApp.Domain.Entities;

namespace MedApp.Application.Appoinments.Documents
{
    public interface IPdfService
    {
        byte[] BuildPrescriptionPdf(Appointment appt);
    }
}
