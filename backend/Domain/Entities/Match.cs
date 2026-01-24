using System;
using PCM.Domain.Enums;

namespace PCM.Domain.Entities
{
    /// <summary>
    /// Table name: XXX_Matches
    /// Core match entity for both daily matches and tournament matches
    /// </summary>
    public class Match
    {
        public int Id { get; set; }
        
        public DateTime Date { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Ranked matches affect ELO rating
        /// </summary>
        public bool IsRanked { get; set; }
        
        public MatchFormat MatchFormat { get; set; }
        
        // Team 1
        public int Team1Player1Id { get; set; }
        public int? Team1Player2Id { get; set; }
        
        // Team 2
        public int Team2Player1Id { get; set; }
        public int? Team2Player2Id { get; set; }
        
        // Scores
        public int? ScoreTeam1 { get; set; }
        public int? ScoreTeam2 { get; set; }
        
        public WinnerSide Result { get; set; }
        
        /// <summary>
        /// ELO points change after match
        /// </summary>
        public double? EloChange { get; set; }
        
        /// <summary>
        /// Null for daily matches
        /// </summary>
        public int? TournamentId { get; set; }
        public Tournament? Tournament { get; set; }
        
        public MatchStatus Status { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
