using System;

namespace PCM.Domain.Entities
{
    /// <summary>
    /// Table name: XXX_Members (replace XXX with last 3 digits of your student ID)
    /// </summary>
    public class Member
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Foreign key to AspNetUsers (Identity)
        /// </summary>
        public string UserId { get; set; } = default!;
        
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        
        public DateTime? DateOfBirth { get; set; }
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// ELO rating - default 1200
        /// </summary>
        public double RankELO { get; set; } = 1200.0;
        
        /// <summary>
        /// E-Wallet balance - default 0
        /// IMPORTANT: Never allow negative balance
        /// </summary>
        public decimal WalletBalance { get; set; } = 0m;
        
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Concurrency token for optimistic locking
        /// </summary>
        public byte[] RowVersion { get; set; } = default!;
        
        // Navigation properties
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
        public virtual ICollection<TreasuryTransaction> TreasuryTransactions { get; set; } = new List<TreasuryTransaction>();
        public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();
    }
}
