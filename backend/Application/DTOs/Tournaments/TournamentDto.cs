namespace PCM.Application.DTOs.Tournaments
{
    public class TournamentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Type { get; set; } = default!;
        public string Format { get; set; } = default!;
        public string Status { get; set; } = default!;
        public decimal EntryFee { get; set; }
        public decimal PrizePool { get; set; }
        public int MaxParticipants { get; set; }
        public int CurrentParticipants { get; set; }
        public DateTime? RegistrationDeadline { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = default!;
    }
}
