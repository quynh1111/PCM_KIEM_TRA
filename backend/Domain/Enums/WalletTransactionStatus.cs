namespace PCM.Domain.Enums
{
    public enum WalletTransactionStatus
    {
        Pending,    // Chờ xử lý (cho deposit)
        Success,    // Thành công
        Failed,     // Thất bại
        Cancelled   // Đã hủy
    }
}
