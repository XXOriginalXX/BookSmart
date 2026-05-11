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

        public int Gender { get; set; }
        public int Age { get; set; }
        public int Neighbourhood { get; set; }
        public int Scholarship { get; set; }
        public int Hypertension { get; set; }
        public int Diabetes { get; set; }
        public int Alcoholism { get; set; }
        public int Handicap { get; set; }
        public int SmsReceived { get; set; }

        public double? NoShowProbability { get; set; }
        public bool IsHighRisk { get; set; } = false;

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