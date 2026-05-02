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

        
    }

    public class BookAppointmentRequest
    {
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime SlotDateTime { get; set; }
        public string? Notes { get; set; }
    }

}