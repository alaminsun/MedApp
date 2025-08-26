//using MedApp.Application.Appoinments.Documents;
//using MedApp.Domain.Entities;
//using QuestPDF.Fluent;

//namespace MedApp.Infrastructure.Persistence.Documents
//{
//    public class QuestPdfService : IPdfService
//    {
//        public byte[] BuildPrescriptionPdf(Appointment appt)
//        {
//            return Document.Create(container =>
//            {
//                container.Page(page =>
//                {
//                    page.Margin(20);
//                    page.Header().Text("Prescription Report").FontSize(18).Bold().AlignCenter();
//                    page.Content().Column(col =>
//                    {
//                        col.Item().Text($"Patient: {appt.Patient?.Name}").FontSize(14);
//                        col.Item().Text($"Doctor: {appt.Doctor?.Name}").FontSize(14);
//                        col.Item().Text($"Date: {appt.AppointmentDate:dd-MMM-yyyy}");
//                        col.Item().Text($"Visit Type: {appt.VisitType}");
//                        col.Item().Text($"Diagnosis: {appt.Diagnosis}");
//                        col.Item().LineHorizontal(1);
//                        col.Item().Table(t =>
//                        {
//                            t.ColumnsDefinition(c =>
//                            {
//                                c.RelativeColumn(4);
//                                c.RelativeColumn(3);
//                                c.RelativeColumn(2);
//                                c.RelativeColumn(2);
//                                c.RelativeColumn(4);
//                            });
//                            t.Header(h =>
//                            {
//                                h.Cell().Text("Medicine").FontSize(12).SemiBold();
//                                h.Cell().Text("Dosage").FontSize(12).SemiBold();
//                                h.Cell().Text("Start").FontSize(12).SemiBold();
//                                h.Cell().Text("End").FontSize(12).SemiBold();
//                                h.Cell().Text("Notes").FontSize(12).SemiBold();
//                            });
//                            foreach (var p in appt.PrescriptionDetails)
//                            {
//                                t.Cell().Text(p.Medicine?.Name ?? "");
//                                t.Cell().Text(p.Dosage);
//                                t.Cell().Text(p.StartDate.ToString("dd-MMM-yyyy"));
//                                t.Cell().Text(p.EndDate.ToString("dd-MMM-yyyy"));
//                                t.Cell().Text(p.Notes ?? "");

//                            }
//                        });
//                    });
//                    page.Footer().AlignCenter().Text(x => x.CurrentPageNumber());
//                });
//            }).GeneratePdf();
//        }
//    }
//}

using MedApp.Application.Appoinments.Documents;
using MedApp.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
// at top of file
using System.Globalization;



namespace MedApp.Infrastructure.Persistence.Documents;

public sealed class QuestPdfService : IPdfService
{
    public byte[] BuildPrescriptionPdf(Appointment appt)
    {
        // make sure in Program.cs you have:
        // QuestPDF.Settings.License = LicenseType.Community;

        //string D(DateTime? dt) => dt.HasValue ? dt.Value.ToString("dd-MMM-yyyy") : "-";
        // replace the old DateTime? helper:
        //static string D(DateOnly d) =>
        //    d.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

        static string D(DateOnly? d) =>
            d.HasValue ? d.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "-";

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(TextStyle.Default.FontSize(12));

                page.Content().Column(col =>
                {
                    col.Spacing(18);

                    // Title
                    col.Item().Text("Prescription Report")
                         .FontSize(34).Bold();

                    // Patient / Doctor / Date / Visit Type block
                    col.Item().Grid(grid =>
                    {
                        grid.Columns(2);
                        grid.Spacing(6);

                        grid.Item().Text(t =>
                        {
                            t.Span("Patient: ").Bold();
                            t.Span(appt.Patient?.Name ?? "-");
                        });

                        grid.Item().Text(t =>
                        {
                            t.Span("Doctor: ").Bold();
                            t.Span(appt.Doctor?.Name ?? "-");
                        });

                        grid.Item().Text(t =>
                        {
                            t.Span("Date: ").Bold();
                            t.Span(D(appt.AppointmentDate));
                        });

                        grid.Item().Text(t =>
                        {
                            t.Span("Visit Type: ").Bold();
                            t.Span(appt.VisitType.ToString());
                        });
                    });

                    // Subtitle
                    col.Item().Text("Prescriptions")
                        .FontSize(18).Bold();

                    // Table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Medicine
                            columns.RelativeColumn(3); // Dosage
                            columns.RelativeColumn(2); // Start
                            columns.RelativeColumn(2); // End
                        });

                        IContainer HeaderCell(IContainer c) => c
                            .Background(Colors.Grey.Lighten3)
                            .Border(1).BorderColor(Colors.Grey.Lighten2)
                            .PaddingVertical(8).PaddingHorizontal(10);

                        IContainer Cell(IContainer c) => c
                            .Border(1).BorderColor(Colors.Grey.Lighten2)
                            .PaddingVertical(10).PaddingHorizontal(10);

                        table.Header(h =>
                        {
                            h.Cell().Element(HeaderCell).Text("Medicine").SemiBold();
                            h.Cell().Element(HeaderCell).Text("Dosage").SemiBold();
                            h.Cell().Element(HeaderCell).Text("Start Date").SemiBold();
                            h.Cell().Element(HeaderCell).Text("End Date").SemiBold();
                        });

                        var rows = appt.PrescriptionDetails?
                            .OrderBy(p => p.StartDate) ?? Enumerable.Empty<PrescriptionDetail>();

                        foreach (var p in rows)
                        {
                            table.Cell().Element(Cell).Text(p.Medicine?.Name ?? "-");

                            table.Cell().Element(Cell).Text(text =>
                            {
                                // allow multi-line dosage like "500mg\n2x/day"
                                var lines = (p.Dosage ?? "-").Replace("\r", "").Split('\n');
                                foreach (var line in lines) text.Line(line);
                            });

                            table.Cell().Element(Cell).Text(D(p.StartDate));
                            table.Cell().Element(Cell).Text(D(p.EndDate));
                        }
                    });
                });
            });
        }).GeneratePdf();

        return pdf;
    }
}
