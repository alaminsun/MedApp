Medical Appointment System — ASP.NET Core + Angular
A full-stack demo that manages patients’ appointments and prescriptions.
Backend is an ASP.NET Core Web API (EF Core) and the frontend is Angular (standalone components, Reactive Forms).
Deliverables required by the brief are complete:
•	Master grid with server-side pagination, search, filters, and row actions (Edit / Delete / PDF / Email)
•	Appointment Create / Edit form with dynamic prescriptions grid (add/edit/delete rows inline) + validation
•	PDF prescription report generation and emailing the report as an attachment
________________________________________
Table of contents
•	Features
•	Tech stack
•	Repository structure
•	Quick start
o	1) Backend (API)
o	2) Frontend (Angular)
•	Configuration
o	Database
o	Email / SMTP
o	PDF (QuestPDF)
o	Angular dev proxy
•	API overview
•	Screens
•	Troubleshooting
•	Video walkthrough & submission
•	License notes
________________________________________
Features
Master grid
•	List appointments with server-side pagination
•	Search (patient / doctor), filter (doctor, visit type)
•	Row actions: Edit, Delete, Download PDF, Email PDF
Appointment form
•	Patient dropdown (DB)
•	Doctor dropdown (DB)
•	Appointment date (date picker)
•	Visit type (radio: First, FollowUp)
•	Notes (free text), Diagnosis (textarea)
•	Prescriptions grid:
o	Add multiple rows
o	Columns: Medicine dropdown (DB), Dosage (text), Start date, End date, Notes
o	Inline edit with validation: medicine required, dosage required, start ≤ end, dates required
o	Add / delete row in place
PDF & Email
•	Generate a Prescription Report (QuestPDF) matching the provided layout
•	Email the PDF to a user-entered address via SMTP
________________________________________
Tech stack
•	Backend: .NET 8, ASP.NET Core Web API, EF Core 8
•	Database: SQL Server (LocalDB) or SQLite (optional swap)
•	PDF: QuestPDF
•	Frontend: Angular 17/18/20 (standalone components, Reactive Forms), TypeScript
•	Styling: Lightweight, Bootstrap-like classes (no heavy UI framework)
•	Tooling: Angular CLI, Vite dev server with HTTP proxy
________________________________________
Repository structure
/api
  MedApp.sln
  /MedApp
    Program.cs
    appsettings.json
    /Controllers
    /Domain /Infrastructure /Persistence
    /Migrations
/client
  package.json
  angular.json
  proxy.conf.json
  /src
    /app
      /core/services
      /models
      /pages
        /appointments-list
        /appointment-form
      app.routes.ts
      app.ts
________________________________________
Quick start
Prereqs
•	.NET SDK 8.0+
•	Node.js 18+ (or 20+) & npm
•	SQL Server LocalDB (or change to SQLite)
1) Backend (API)
cd api
dotnet restore
dotnet ef database update   # apply migrations and seed data
dotnet run                  # runs on http://localhost:7246 by default (Kestrel)
If the port differs on your machine, update the Angular proxy (see below).
2) Frontend (Angular)
cd client
npm install
npm start       # ng serve + proxy; opens http://localhost:4200
Navigate to http://localhost:4200/appointments.
________________________________________
Configuration
Database
By default the API uses SQL Server (LocalDB). Edit /api/MedApp/appsettings.json:
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MedAppDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
Prefer SQLite? Change the EF provider in Program.cs and set a file path connection string; run dotnet ef database update.
Email / SMTP
Set your SMTP in /api/MedApp/appsettings.json:
{
  "EmailSettings": {
    "SmtpHost": "smtp.yourhost.com",
    "SmtpPort": 587,
    "UseSsl": true,
    "SmtpUser": "apikey-or-username",
    "SmtpPass": "secret",
    "From": "noreply@yourdomain.com",
    "FromName": "MedApp"
  }
}
The API reads these values and uses MailKit/MimeKit (or SmtpClient) to send the message with the PDF attachment.
If your SMTP does not require auth, leave SmtpUser empty and the code will skip authentication.
PDF (QuestPDF)
QuestPDF requires choosing a license mode in code. The project sets:
QuestPDF.Settings.License = LicenseType.Community;
This is suitable for personal / small-scale use (see license section).
Angular dev proxy
Angular calls the API through a local proxy to avoid CORS and to keep URLs tidy.
/client/proxy.conf.json:
{
  "/api": {
    "target": "http://localhost:5000",
    "secure": false,
    "changeOrigin": true,
    "logLevel": "info",
    "pathRewrite": { "^/api": "" }
  }
}
API endpoints are consumed like /api/medapp/appointments?... on the client, and the proxy forwards to the Kestrel port.
________________________________________
API overview
Base route (via proxy): /api/medapp
•	GET /appointments?pageNumber=1&pageSize=10&search=&doctorId=&visitType=
Returns paged results: { items: AppointmentListItem[], total: number }
•	GET /appointments/{id}
Appointment detail (includes prescriptionDetails)
•	POST /appointments
Create; body AppointmentCU (includes prescriptions collection)
•	PUT /appointments/{id}
Update; body AppointmentCU
•	DELETE /appointments/{id}
Remove appointment
•	GET /appointments/{id}/pdf
Returns generated PDF (application/pdf)
•	POST /appointments/{id}/email?to=someone@example.com
Sends the prescription PDF to the given email
•	GET /patients / GET /doctors / GET /medicines
Lookup lists for dropdowns
________________________________________
Screens
•	Appointments list: search, filters (doctor, visit type), server-side pagination, actions (Edit / Delete / PDF / Email)
•	Appointment form: patient, doctor, date, visit type, notes, diagnosis
Prescriptions grid with inline add/edit/delete and validation:
o	Medicine required
o	Dosage required
o	Start & End date required
o	Start date must be ≤ End date
•	PDF report: matches the provided layout
•	Email dialog: enter target email, send, success/failure toast
________________________________________
Troubleshooting
API starts but Angular calls fail
•	Check proxy target port in proxy.conf.json matches your API port.
•	If you bypass proxy, enable CORS in the API.
SMTP authentication fails
•	Verify host/port/SSL in appsettings.json
•	If server requires OAuth or app-passwords, configure accordingly.
•	For local tests you can set a dummy SMTP (e.g., Ethereal).
QuestPDF license warning
•	Ensure QuestPDF.Settings.License = LicenseType.Community; is executed once on startup.
DateOnly vs DateTime conversion
•	The API normalizes DateOnly to DateTime where needed for PDF or persistence.
________________________________________
Video walkthrough & submission
Record a short video (YouTube/Drive) covering:
1.	Project goal & requirements
2.	Tech stack and architecture
3.	Running the API and Angular app
4.	Creating/editing an appointment with multiple prescriptions
5.	Master grid search, filters, pagination
6.	Downloading the PDF and sending the email
7.	Code highlights: reactive form + prescription grid validation, server-side paging, PDF generation
Add the video link to your submission along with this GitHub repository link.
________________________________________
License notes
This repository is provided for technical interview purposes.
•	QuestPDF is open-source with a sustainable license model. If your organization’s annual gross revenue exceeds $1M USD, a commercial license is required for production use. See: https://www.questpdf.com/license/
•	Other libraries follow their respective licenses.
________________________________________
Author
Built by an ASP.NET full-stack developer as part of a technical assessment.

