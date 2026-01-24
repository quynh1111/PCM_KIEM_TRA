namespace PCM.Application.DTOs.Bookings
{
    public class CreateBookingDto
    {
        public int CourtId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Note { get; set; }
    }
}
