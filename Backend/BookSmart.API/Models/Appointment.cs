namespace AppointmentSystem.API.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime SlotDateTime { get; set; }
        public DateTime ScheduledAt { get; set; } = DateTime.UtcNow;
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
        public string? Notes { get; set; }

        public User Patient { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
    }

    public enum AppointmentStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed
    }
}