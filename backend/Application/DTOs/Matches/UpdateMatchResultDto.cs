using PCM.Domain.Enums;

namespace PCM.Application.DTOs.Matches
{
    public class UpdateMatchResultDto
    {
        public int MatchId { get; set; }
        public int ScoreTeam1 { get; set; }
        public int ScoreTeam2 { get; set; }
        public WinnerSide Result { get; set; }
    }
}
