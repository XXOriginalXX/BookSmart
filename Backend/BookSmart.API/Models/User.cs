namespace AppointmentSystem.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public UserRole Role { get; set; } = UserRole.Patient;
    }

    public enum UserRole
    {
        Patient,
        Admin
    }
}