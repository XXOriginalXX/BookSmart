using AppointmentSystem.API.Data;
using AppointmentSystem.API.Models;
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
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DoctorRequest request)
        {
            bool emailTaken = await _db.Doctors.AnyAsync(d => d.Email == request.Email);
            if (emailTaken)
                return BadRequest(new { message = "Email already in use." });
 
            var doctor = new Doctor
            {
                FullName = request.FullName,
                Specialization = request.Specialization,
                Email = request.Email
            };
 
            _db.Doctors.Add(doctor);
            await _db.SaveChangesAsync();
 
            return Ok(new { message = "Doctor added.", doctorId = doctor.Id });
        }


        
        }
    }

    public class DoctorRequest
    {
        public string FullName { get; set; } = "";
        public string Specialization { get; set; } = "";
        public string Email { get; set; } = "";
    }
