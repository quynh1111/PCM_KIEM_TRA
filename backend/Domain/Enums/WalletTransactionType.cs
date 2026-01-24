namespace PCM.Domain.Enums
{
    public enum WalletTransactionType
    {
        Deposit,        // Nạp tiền
        PayBooking,     // Thanh toán đặt sân
        PayTournament,  // Thanh toán phí tham gia giải
        ReceivePrize,   // Nhận tiền thưởng
        Refund,         // Hoàn tiền
        Adjustment      // Điều chỉnh số dư (Admin)
    }
}
