using System;
using PCM.Domain.Enums;

namespace PCM.Domain.Entities
{
    /// <summary>
    /// Table name: XXX_Participants
    /// </summary>
    public class Participant
    {
        public int Id { get; set; }
        
        public int TournamentId { get; set; }
        public Tournament Tournament { get; set; } = default!;
        
        public int MemberId { get; set; }
        public Member Member { get; set; } = default!;
        
        public string? TeamName { get; set; }
        public ParticipantStatus Status { get; set; }
        
        /// <summary>
        /// Seed number for bracket generation
        /// </summary>
        public int? SeedNo { get; set; }
        
        public DateTime RegisteredDate { get; set; } = DateTime.UtcNow;
        public DateTime? CheckInDate { get; set; }
    }
}
