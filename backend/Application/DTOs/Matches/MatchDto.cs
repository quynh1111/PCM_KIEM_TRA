namespace PCM.Application.DTOs.Matches
{
    public class MatchDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public bool IsRanked { get; set; }
        public string MatchFormat { get; set; } = default!;
        
        public int Team1Player1Id { get; set; }
        public string Team1Player1Name { get; set; } = default!;
        public int? Team1Player2Id { get; set; }
        public string? Team1Player2Name { get; set; }
        
        public int Team2Player1Id { get; set; }
        public string Team2Player1Name { get; set; } = default!;
        public int? Team2Player2Id { get; set; }
        public string? Team2Player2Name { get; set; }
        
        public int? ScoreTeam1 { get; set; }
        public int? ScoreTeam2 { get; set; }
        public string Result { get; set; } = default!;
        public double? EloChange { get; set; }
        
        public int? TournamentId { get; set; }
        public string? TournamentName { get; set; }
        public string Status { get; set; } = default!;
    }
}
