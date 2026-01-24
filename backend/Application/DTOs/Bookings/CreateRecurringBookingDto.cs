namespace PCM.Application.DTOs.Bookings
{
    public class CreateRecurringBookingDto
    {
        public int CourtId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public List<DayOfWeek> DaysOfWeek { get; set; } = new();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Note { get; set; }
    }
}
