namespace PCM.Application.DTOs.Auth
{
    public class RefreshTokenDto
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
    }
}
