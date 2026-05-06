namespace AppointmentSystem.API.Models
{
    public class DoctorAvailability
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public DateTime SlotStart { get; set; }
        public DateTime SlotEnd { get; set; }
        public bool IsBooked { get; set; } = false;

        public Doctor Doctor { get; set; } = null!;
    }
}