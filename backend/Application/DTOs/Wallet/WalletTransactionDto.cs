namespace PCM.Application.DTOs.Wallet
{
    public class WalletTransactionDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int MemberId { get; set; }
        public string MemberName { get; set; } = default!;
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = default!;
        public string Type { get; set; } = default!;
        public string? ReferenceId { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = default!;
        public string? ProofImageUrl { get; set; }
    }
}
