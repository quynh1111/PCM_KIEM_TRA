namespace PCM.Application.DTOs.Tournaments
{
    public class BracketDto
    {
        public int TournamentId { get; set; }
        public string TournamentName { get; set; } = default!;
        public List<BracketNode> Nodes { get; set; } = new();
        public List<BracketEdge> Edges { get; set; } = new();
    }
    
    public class BracketNode
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public int Round { get; set; }
        public int Position { get; set; }
        public string? Team1 { get; set; }
        public string? Team2 { get; set; }
        public int? ScoreTeam1 { get; set; }
        public int? ScoreTeam2 { get; set; }
        public string? Winner { get; set; }
        public string Status { get; set; } = default!;
    }
    
    public class BracketEdge
    {
        public int From { get; set; }
        public int To { get; set; }
    }
}
