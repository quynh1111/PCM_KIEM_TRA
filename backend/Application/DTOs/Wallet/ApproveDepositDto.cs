namespace PCM.Application.DTOs.Wallet
{
    public class ApproveDepositDto
    {
        public int TransactionId { get; set; }
        public bool Approved { get; set; }
        public string? Note { get; set; }
    }
}
