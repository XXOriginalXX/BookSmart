namespace AppointmentSystem.API.Models
{
    public class AppointmentDocument
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public Appointment Appointment { get; set; } = null!;
    }
}