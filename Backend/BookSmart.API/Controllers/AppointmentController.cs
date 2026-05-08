using AppointmentSystem.API.Data;
using AppointmentSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    public class AppointmentController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AppointmentController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("specializations")]
        public async Task<IActionResult> GetSpecializations()
        {
            var specializations = await _db.Doctors
                .Select(d => d.Specialization)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            return Ok(specializations);
        }

        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctorsBySpecialization([FromQuery] string specialization)
        {
            if (string.IsNullOrWhiteSpace(specialization))
                return BadRequest(new { message = "Specialization is required." });

            var doctors = await _db.Doctors
                .Where(d => d.Specialization == specialization)
                .Select(d => new { d.Id, d.FullName, d.Specialization })
                .ToListAsync();

            return Ok(doctors);
        }

        [HttpGet("slots/{doctorId}")]
        public async Task<IActionResult> GetAvailableSlots(int doctorId)
        {
            bool doctorExists = await _db.Doctors.AnyAsync(d => d.Id == doctorId);
            if (!doctorExists)
                return NotFound(new { message = "Doctor not found." });

            var slots = await _db.DoctorAvailability
                .Where(da => da.DoctorId == doctorId && !da.IsBooked && da.SlotStart > DateTime.UtcNow)
                .OrderBy(da => da.SlotStart)
                .Select(da => new { da.Id, da.SlotStart, da.SlotEnd })
                .ToListAsync();

            return Ok(slots);
        }

        [HttpPost]
        public async Task<IActionResult> Book([FromBody] BookAppointmentRequest request)
        {
            bool patientExists = await _db.Users.AnyAsync(u => u.Id == request.PatientId);
            if (!patientExists)
                return BadRequest(new { message = "Patient not found." });

            var slot = await _db.DoctorAvailability
                .Include(da => da.Doctor)
                .FirstOrDefaultAsync(da => da.Id == request.SlotId);

            if (slot == null)
                return NotFound(new { message = "Slot not found." });

            if (slot.IsBooked)
                return Conflict(new { message = "This slot is already booked." });

            var appointment = new Appointment
            {
                PatientId = request.PatientId,
                DoctorId = slot.DoctorId,
                SlotDateTime = slot.SlotStart,
                Notes = request.Notes
            };

            slot.IsBooked = true;

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Appointment booked.", appointmentId = appointment.Id });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var appointment = await _db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound(new { message = "Appointment not found." });

            return Ok(MapToResponse(appointment));
        }

        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetByPatient(int patientId)
        {
            var appointments = await _db.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.SlotDateTime)
                .ToListAsync();

            return Ok(appointments.Select(MapToResponse));
        }

        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetByDoctor(int doctorId)
        {
            var appointments = await _db.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId)
                .OrderBy(a => a.SlotDateTime)
                .ToListAsync();

            return Ok(appointments.Select(MapToResponse));
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var appointment = await _db.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound(new { message = "Appointment not found." });

            if (!IsValidTransition(appointment.Status, request.Status))
                return BadRequest(new { message = $"Cannot transition from {appointment.Status} to {request.Status}." });

            appointment.Status = request.Status;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Status updated.", status = appointment.Status.ToString() });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _db.Appointments
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound(new { message = "Appointment not found." });

            if (appointment.Status == AppointmentStatus.Completed)
                return BadRequest(new { message = "Cannot cancel a completed appointment." });

            var slot = await _db.DoctorAvailability
                .FirstOrDefaultAsync(da =>
                    da.DoctorId == appointment.DoctorId &&
                    da.SlotStart == appointment.SlotDateTime);

            if (slot != null)
                slot.IsBooked = false;

            appointment.Status = AppointmentStatus.Cancelled;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Appointment cancelled." });
        }

        private static bool IsValidTransition(AppointmentStatus current, AppointmentStatus next)
        {
            return (current, next) switch
            {
                (AppointmentStatus.Pending, AppointmentStatus.Confirmed) => true,
                (AppointmentStatus.Pending, AppointmentStatus.Cancelled) => true,
                (AppointmentStatus.Confirmed, AppointmentStatus.Completed) => true,
                (AppointmentStatus.Confirmed, AppointmentStatus.Cancelled) => true,
                _ => false
            };
        }

        private static object MapToResponse(Appointment a) => new
        {
            a.Id,
            a.PatientId,
            patientName = a.Patient?.FullName,
            a.DoctorId,
            doctorName = a.Doctor?.FullName,
            doctorSpecialization = a.Doctor?.Specialization,
            a.SlotDateTime,
            a.ScheduledAt,
            status = a.Status.ToString(),
            a.Notes
        };
    }

    public class BookAppointmentRequest
    {
        public int PatientId { get; set; }
        public int SlotId { get; set; }
        public string? Notes { get; set; }
    }

    //public class UpdateStatusRequest
    //{
     //   public AppointmentStatus Status { get; set; }
    //}
}