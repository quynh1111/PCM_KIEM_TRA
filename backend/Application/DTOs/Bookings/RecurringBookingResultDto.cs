namespace PCM.Application.DTOs.Bookings
{
    public class RecurringBookingResultDto
    {
        public List<BookingDto> SuccessfulBookings { get; set; } = new();
        public List<ConflictDto> Conflicts { get; set; } = new();
        public int TotalAttempted { get; set; }
        public int TotalSuccess { get; set; }
        public int TotalFailed { get; set; }
    }
    
    public class ConflictDto
    {
        public DateTime Date { get; set; }
        public string Reason { get; set; } = default!;
    }
}
