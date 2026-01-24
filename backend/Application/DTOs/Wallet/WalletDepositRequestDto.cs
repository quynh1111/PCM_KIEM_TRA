namespace PCM.Application.DTOs.Wallet
{
    public class WalletDepositRequestDto
    {
        public decimal Amount { get; set; }
        public string? ProofImageUrl { get; set; }
        public string? Description { get; set; }
    }
}
