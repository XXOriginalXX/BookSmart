using AppointmentSystem.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/doctors")]
    public class DoctorController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DoctorController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var doctors = await _db.Doctors.ToListAsync();
            return Ok(doctors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var doctor = await _db.Doctors.FindAsync(id);
            if (doctor == null)
                return NotFound(new { message = "Doctor not found." });

            return Ok(doctor);
        }
    }
}