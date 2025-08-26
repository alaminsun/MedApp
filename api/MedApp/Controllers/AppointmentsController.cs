using MedApp.Application.Appoinments.Documents;
using MedApp.Application.Appoinments.Dtos;
using MedApp.Application.Appoinments.Messaging;
using MedApp.Application.Shared;
using MedApp.Domain.Entities;
using MedApp.Domain.Enum;
using MedApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities;

namespace MedApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController(AppDbContext db, IPdfService pdf, IEmailService email) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<PagedResult<AppointmentListDto>>> List(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] int? doctorId = null,
            [FromQuery] VisitType? visitType = null
            )
        {
            var q = db.Appointments.AsNoTracking()
                .Include(a => a.Patient).Include(a => a.Doctor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(a => (a.Patient != null && a.Patient.Name.Contains(search)) || (a.Doctor != null && a.Doctor.Name.Contains(search)));

            if (doctorId.HasValue) q = q.Where(a => a.DoctorId == doctorId.Value);
            if (visitType.HasValue) q = q.Where(a => a.VisitType == visitType.Value);

            var total = await q.CountAsync();
            var items = await q
                .OrderByDescending(a => a.AppointmentDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AppointmentListDto(
                    a.Id,
                    a.Patient!.Name,
                    a.Doctor!.Name,
                    a.AppointmentDate,
                    a.VisitType,
                    //a.Notes,
                    a.Diagnosis
                ))
                .ToListAsync();
            return Ok(new PagedResult<AppointmentListDto>(items, total, pageNumber, pageSize));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PagedResult<AppointmentListDto>>> Get(int id)
        {
            var appointment = await db.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.PrescriptionDetails!)
                    .ThenInclude(pd => pd.Medicine)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null) return NotFound();
            return Ok(appointment);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AppointmentCreateUpdateDto dto)
        {
            var appointment = new Appointment
            {
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                AppointmentDate = dto.AppointmentDate,
                VisitType = dto.VisitType,
                Notes = dto.Notes,
                Diagnosis = dto.Diagnosis,
                PrescriptionDetails = dto.Prescriptions.Select(pd => new PrescriptionDetail
                {
                    MedicineId = pd.MedicineId,
                    Dosage = pd.Dosage,
                    StartDate = pd.StartDate,
                    EndDate = pd.EndDate,
                    Notes = pd.Notes
                }).ToList()
            };

            db.Appointments.Add(appointment);
            await db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = appointment.Id }, new { appointment.Id });
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] AppointmentCreateUpdateDto dto)
        {
            var appointment = await db.Appointments
                .Include(a => a.PrescriptionDetails)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null) return NotFound();
            appointment.PatientId = dto.PatientId;
            appointment.DoctorId = dto.DoctorId;
            appointment.AppointmentDate = dto.AppointmentDate;
            appointment.VisitType = dto.VisitType;
            appointment.Notes = dto.Notes;
            appointment.Diagnosis = dto.Diagnosis;

            var incoming = dto.Prescriptions;
            appointment.PrescriptionDetails.RemoveAll(x => !incoming.Any(p => p.Id.HasValue && p.Id.Value == x.Id));
            foreach (var p in incoming)
            {
                var existing = p.Id.HasValue ? appointment.PrescriptionDetails.FirstOrDefault(x => x.Id == p.Id.Value) : null;
                if (existing != null)
                {
                    existing.MedicineId = p.MedicineId;
                    existing.Dosage = p.Dosage;
                    existing.StartDate = p.StartDate;
                    existing.EndDate = p.EndDate;
                    existing.Notes = p.Notes;
                }
                else
                {
                    appointment.PrescriptionDetails.Add(new PrescriptionDetail
                    {
                        MedicineId = p.MedicineId,
                        Dosage = p.Dosage,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        Notes = p.Notes
                    });
                }
            }
            await db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await db.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();
            db.Appointments.Remove(appointment);
            await db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id:int}/pdf")]
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var appointment = await db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.PrescriptionDetails!)
                    .ThenInclude(pd => pd.Medicine)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null) return NotFound();
            var pdfBytes = pdf.BuildPrescriptionPdf(appointment);
            return File(pdfBytes, "application/pdf", $"Prescription_{appointment.Id}.pdf");

        }

        [HttpPost("{id:int}/email")]
        public async Task<IActionResult> EmailPdf(int id, [FromQuery] string toEmail)
        {
            var appointment = await db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.PrescriptionDetails!)
                    .ThenInclude(pd => pd.Medicine)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null) return NotFound();
            var pdfBytes = pdf.BuildPrescriptionPdf(appointment);
            var subject = $"Prescription for {appointment.Patient?.Name}";
            var body = $"Dear {appointment.Patient?.Name},<br/>Please find your prescription attached.";
            await email.SendEmailAsync(toEmail, subject, body, pdfBytes, $"Prescription-{id}.pdf");
            return Ok();
        }
    }
}
