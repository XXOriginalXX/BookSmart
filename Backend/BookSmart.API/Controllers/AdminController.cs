using AppointmentSystem.API.Data;
using AppointmentSystem.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        // ── Doctors ──

        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _db.Doctors.ToListAsync();
            return Ok(doctors);
        }

        [HttpPost("doctors")]
        public async Task<IActionResult> AddDoctor([FromBody] DoctorRequest request)
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

        [HttpPut("doctors/{id}")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromBody] DoctorRequest request)
        {
            var doctor = await _db.Doctors.FindAsync(id);
            if (doctor == null)
                return NotFound(new { message = "Doctor not found." });

            doctor.FullName = request.FullName;
            doctor.Specialization = request.Specialization;
            doctor.Email = request.Email;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Doctor updated." });
        }

        [HttpDelete("doctors/{id}")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _db.Doctors.FindAsync(id);
            if (doctor == null)
                return NotFound(new { message = "Doctor not found." });

            bool hasAppointments = await _db.Appointments.AnyAsync(a => a.DoctorId == id);
            if (hasAppointments)
                return BadRequest(new { message = "Cannot delete a doctor with existing appointments." });

            _db.Doctors.Remove(doctor);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Doctor removed." });
        }

        // ── Availability ──

        [HttpPost("doctors/{doctorId}/availability")]
        public async Task<IActionResult> AddSlots(int doctorId, [FromBody] AddSlotsRequest request)
        {
            bool doctorExists = await _db.Doctors.AnyAsync(d => d.Id == doctorId);
            if (!doctorExists)
                return NotFound(new { message = "Doctor not found." });

            var slots = new List<DoctorAvailability>();
            var current = request.From;

            while (current.AddMinutes(request.SlotDurationMinutes) <= request.To)
            {
                bool overlap = await _db.DoctorAvailability.AnyAsync(da =>
                    da.DoctorId == doctorId &&
                    da.SlotStart < current.AddMinutes(request.SlotDurationMinutes) &&
                    da.SlotEnd > current);

                if (!overlap)
                {
                    slots.Add(new DoctorAvailability
                    {
                        DoctorId = doctorId,
                        SlotStart = current,
                        SlotEnd = current.AddMinutes(request.SlotDurationMinutes)
                    });
                }

                current = current.AddMinutes(request.SlotDurationMinutes);
            }

            _db.DoctorAvailability.AddRange(slots);
            await _db.SaveChangesAsync();

            return Ok(new { message = $"{slots.Count} slot(s) added." });
        }

        [HttpDelete("availability/{slotId}")]
        public async Task<IActionResult> DeleteSlot(int slotId)
        {
            var slot = await _db.DoctorAvailability.FindAsync(slotId);
            if (slot == null)
                return NotFound(new { message = "Slot not found." });

            if (slot.IsBooked)
                return BadRequest(new { message = "Cannot delete a booked slot." });

            _db.DoctorAvailability.Remove(slot);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Slot removed." });
        }

        [HttpGet("doctors/{doctorId}/availability")]
        public async Task<IActionResult> GetSlots(int doctorId)
        {
            var slots = await _db.DoctorAvailability
                .Where(da => da.DoctorId == doctorId)
                .OrderBy(da => da.SlotStart)
                .Select(da => new { da.Id, da.SlotStart, da.SlotEnd, da.IsBooked })
                .ToListAsync();

            return Ok(slots);
        }

        // ── Appointments ──

        [HttpGet("appointments")]
        public async Task<IActionResult> GetAllAppointments([FromQuery] AppointmentStatus? status, [FromQuery] bool? highRiskOnly)
        {
            var query = _db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            if (highRiskOnly == true)
                query = query.Where(a => a.IsHighRisk);

            var appointments = await query
                .OrderByDescending(a => a.SlotDateTime)
                .ToListAsync();

            return Ok(appointments.Select(a => new
            {
                a.Id,
                a.PatientId,
                patientName = a.Patient.FullName,
                a.DoctorId,
                doctorName = a.Doctor.FullName,
                doctorSpecialization = a.Doctor.Specialization,
                a.SlotDateTime,
                a.ScheduledAt,
                status = a.Status.ToString(),
                a.Notes,
                a.NoShowProbability,
                a.IsHighRisk
            }));
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var total = await _db.Appointments.CountAsync();
            var highRisk = await _db.Appointments.CountAsync(a => a.IsHighRisk);
            var doctors = await _db.Doctors.CountAsync();
            var patients = await _db.Users.CountAsync(u => u.Role == UserRole.Patient);
            var pending = await _db.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending);

            return Ok(new { total, highRisk, doctors, patients, pending });
        }

        [HttpPatch("appointments/{id}/status")]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var appointment = await _db.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound(new { message = "Appointment not found." });

            appointment.Status = request.Status;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Status updated.", status = appointment.Status.ToString() });
        }

        // ── Patients ──

        [HttpGet("patients")]
        public async Task<IActionResult> GetAllPatients()
        {
            var patients = await _db.Users
                .Where(u => u.Role == UserRole.Patient)
                .Select(u => new { u.Id, u.FullName, u.Email })
                .ToListAsync();

            return Ok(patients);
        }
    }

    public class DoctorRequest
    {
        public string FullName { get; set; } = "";
        public string Specialization { get; set; } = "";
        public string Email { get; set; } = "";
    }

    public class AddSlotsRequest
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int SlotDurationMinutes { get; set; } = 30;
    }

    public class UpdateStatusRequest
    {
        public AppointmentStatus Status { get; set; }
    }
}