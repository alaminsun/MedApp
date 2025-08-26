using MedApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupsController(AppDbContext db) : ControllerBase
    {
        [HttpGet("patient")]
        public async Task<IActionResult> Patients([FromQuery] string? q= null)
            => Ok(await db.Patients.AsNoTracking()
                .Where( p => q == null || p.Name.Contains(q))
                .OrderBy(p => p.Name)
                .Take(50)
                .ToListAsync());

        [HttpGet("doctor")]
        public async Task<IActionResult> Doctors([FromQuery] string? q = null)
            => Ok(await db.Doctors.AsNoTracking()
                .Where(d => q == null || d.Name.Contains(q) || d.Specialty.Contains(q))
                .OrderBy(d => d.Name)
                .Take(50)
                .ToListAsync());

        [HttpGet("medicine")]
        public async Task<IActionResult> Medicines([FromQuery] string? q = null)
            => Ok(await db.Medicines.AsNoTracking()
                .Where(m => q == null || m.Name.Contains(q) || (m.Manufacturer != null && m.Manufacturer.Contains(q)))
                .OrderBy(m => m.Name)
                .Take(50)
                .ToListAsync());

    }
}
