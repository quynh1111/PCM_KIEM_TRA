using PCM.Domain.Enums;

namespace PCM.Application.DTOs.Matches
{
    public class CreateMatchDto
    {
        public bool IsRanked { get; set; }
        public MatchFormat MatchFormat { get; set; }
        
        public int Team1Player1Id { get; set; }
        public int? Team1Player2Id { get; set; }
        
        public int Team2Player1Id { get; set; }
        public int? Team2Player2Id { get; set; }
        
        public int? TournamentId { get; set; }
    }
}
