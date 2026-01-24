using System;

namespace PCM.Domain.Entities
{
    /// <summary>
    /// Table name: XXX_RefreshTokens
    /// For JWT refresh token management
    /// </summary>
    public class RefreshToken
    {
        public int Id { get; set; }
        
        public string UserId { get; set; } = default!;
        public string Token { get; set; } = default!;
        public string JwtId { get; set; } = default!;
        
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
        public DateTime ExpiryDate { get; set; }
    }
}
