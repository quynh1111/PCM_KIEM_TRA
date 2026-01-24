namespace PCM.Application.DTOs.Bookings
{
    public class BookingDto
    {
        public int Id { get; set; }
        public int CourtId { get; set; }
        public string CourtName { get; set; } = default!;
        public int MemberId { get; set; }
        public string MemberName { get; set; } = default!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = default!;
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? RecurringGroupId { get; set; }
    }
}
