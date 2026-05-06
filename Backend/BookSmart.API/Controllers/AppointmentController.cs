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

        [HttpPost]
        public async Task<IActionResult> Book([FromBody] BookAppointmentRequest request)
        {
            bool patientExists = await _db.Users.AnyAsync(u => u.Id == request.PatientId);
            if (!patientExists)
                return BadRequest(new { message = "Patient not found." });

            bool doctorExists = await _db.Doctors.AnyAsync(d => d.Id == request.DoctorId);
            if (!doctorExists)
                return BadRequest(new { message = "Doctor not found." });

            bool slotTaken = await _db.Appointments.AnyAsync(a =>
                a.DoctorId == request.DoctorId &&
                a.SlotDateTime == request.SlotDateTime &&
                a.Status != AppointmentStatus.Cancelled);

            if (slotTaken)
                return Conflict(new { message = "This slot is already booked." });

            var appointment = new Appointment
            {
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                SlotDateTime = request.SlotDateTime,
                Notes = request.Notes
            };

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
            var appointment = await _db.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound(new { message = "Appointment not found." });
 
            if (appointment.Status == AppointmentStatus.Completed)
                return BadRequest(new { message = "Cannot cancel a completed appointment." });
 
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
        public int DoctorId { get; set; }
        public DateTime SlotDateTime { get; set; }
        public string? Notes { get; set; }
    }
    public class UpdateStatusRequest
    {
        public AppointmentStatus Status { get; set; }
    }

}