using System;

namespace PCM.Domain.Entities
{
    /// <summary>
    /// Table name: XXX_Courts
    /// </summary>
    public class Court
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        
        public decimal HourlyRate { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
