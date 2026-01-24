namespace PCM.Application.DTOs.Auth
{
    public class RegisterDto
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public DateTime? DateOfBirth { get; set; }
        public double InitialRankELO { get; set; } = 1200.0;
    }
}
