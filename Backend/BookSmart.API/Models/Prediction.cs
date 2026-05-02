namespace AppointmentSystem.API.Models
{
    public class PredictionRequest
    {
        public int Gender { get; set; }
        public int Age { get; set; }
        public int Neighbourhood { get; set; }
        public int Scholarship { get; set; }
        public int Hypertension { get; set; }
        public int Diabetes { get; set; }
        public int Alcoholism { get; set; }
        public int Handicap { get; set; }
        public int SMS_received { get; set; }
        public int WaitingDays { get; set; }
        public int AppointmentDayOfWeek { get; set; }
        public int AppointmentHour { get; set; }
    }

    public class PredictionResult
    {
        public double NoShowProbability { get; set; }
        public bool WillNoShow { get; set; }
        public double Threshold { get; set; }
    }
}