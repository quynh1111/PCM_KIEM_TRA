namespace PCM.Application.DTOs.Members
{
    public class MemberDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public DateTime? DateOfBirth { get; set; }
        public DateTime JoinDate { get; set; }
        public double RankELO { get; set; }
        public decimal WalletBalance { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
