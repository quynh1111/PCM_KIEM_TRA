using System;
using PCM.Domain.Enums;

namespace PCM.Domain.Entities
{
    /// <summary>
    /// Table name: XXX_Tournaments
    /// </summary>
    public class Tournament
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public TournamentType Type { get; set; }
        public TournamentFormat Format { get; set; }
        public TournamentStatus Status { get; set; }
        
        public decimal EntryFee { get; set; }
        public decimal PrizePool { get; set; }
        
        /// <summary>
        /// JSON configuration for bracket or rules
        /// </summary>
        public string? ConfigData { get; set; }
        
        public int MaxParticipants { get; set; }
        public DateTime? RegistrationDeadline { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = default!;
        
        // Navigation properties
        public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();
        public virtual ICollection<TournamentMatch> TournamentMatches { get; set; } = new List<TournamentMatch>();
        public virtual ICollection<Match> Matches { get; set; } = new List<Match>();
    }
}
