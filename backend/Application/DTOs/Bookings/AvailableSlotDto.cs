namespace PCM.Application.DTOs.Bookings
{
    public class AvailableSlotDto
    {
        public int CourtId { get; set; }
        public string CourtName { get; set; } = default!;
        public DateTime Date { get; set; }
        public List<TimeSlot> AvailableSlots { get; set; } = new();
    }
    
    public class TimeSlot
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
    }
}
