using PCM.Domain.Enums;

namespace PCM.Application.DTOs.Tournaments
{
    public class CreateTournamentDto
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TournamentType Type { get; set; }
        public TournamentFormat Format { get; set; }
        public decimal EntryFee { get; set; }
        public decimal PrizePool { get; set; }
        public int MaxParticipants { get; set; }
        public DateTime? RegistrationDeadline { get; set; }
    }
}
