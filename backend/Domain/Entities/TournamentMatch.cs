using System;

namespace PCM.Domain.Entities
{
    /// <summary>
    /// Table name: XXX_TournamentMatches
    /// Bracket structure for knockout tournaments
    /// </summary>
    public class TournamentMatch
    {
        public int Id { get; set; }
        
        public int TournamentId { get; set; }
        public Tournament Tournament { get; set; } = default!;
        
        /// <summary>
        /// Round number: 1=Group, 2=Quarter, 3=Semi, 4=Final
        /// </summary>
        public int Round { get; set; }
        
        public int MatchId { get; set; }
        public Match Match { get; set; } = default!;
        
        /// <summary>
        /// Next match that winner advances to
        /// </summary>
        public int? NextMatchId { get; set; }
        
        /// <summary>
        /// Parent matches (where participants came from)
        /// </summary>
        public int? ParentMatchId { get; set; }
        
        /// <summary>
        /// For double elimination: WinnerBracket, LoserBracket
        /// </summary>
        public string? BracketGroup { get; set; }
        
        /// <summary>
        /// Position in bracket (for display)
        /// </summary>
        public int Position { get; set; }
    }
}
