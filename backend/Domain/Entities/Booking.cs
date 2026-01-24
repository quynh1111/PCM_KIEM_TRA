using System;
using PCM.Domain.Enums;

namespace PCM.Domain.Entities
{
    /// <summary>
    /// Table name: XXX_Bookings
    /// CRITICAL: Uses RowVersion for optimistic concurrency control
    /// </summary>
    public class Booking
    {
        public int Id { get; set; }
        
        public int CourtId { get; set; }
        public Court Court { get; set; } = default!;
        
        public int MemberId { get; set; }
        public Member Member { get; set; } = default!;
        
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// For recurring bookings - group identifier
        /// </summary>
        public string? RecurringGroupId { get; set; }
        
        /// <summary>
        /// CRITICAL: Concurrency token for race condition prevention
        /// </summary>
        public byte[] RowVersion { get; set; } = default!;
    }
}
